using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(ContentBrowserView))]
public sealed partial class ContentBrowserViewModel : ViewModelBase
{
    private readonly IServiceProvider _provider;
    private readonly ContentService _contentService;
    private readonly CancellationService _cancellationService;
    private readonly DebugService _debugService;
    private readonly PopupMessageService _popupService;
    public ObservableCollection<ContentEntry> Entries { get; } = new();
    private readonly List<ContentEntry> _root = new();

    private List<string> _history = new();
    
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _searchText = "";

    private ContentEntry? _selectedEntry;

    public ContentEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            _selectedEntry = value;
            Entries.Clear();
            
            Console.WriteLine("Entries clear!");
            
            if(value == null) return;
            
            foreach (var (_,entryCh) in value.Childs)
            {
                Entries.Add(entryCh);
            }
        }
    }


    public ContentBrowserViewModel() : base()
    {
        var a = new ContentEntry(this, "A:", "");
        var b = new ContentEntry(this, "B", "");
        a.TryAddChild(b);
           Entries.Add(a);
    }

    public ContentBrowserViewModel(IServiceProvider provider, ContentService contentService, CancellationService cancellationService, 
        FileService fileService, HubService hubService, DebugService debugService, PopupMessageService popupService) : base(provider)
    {
        _provider = provider;
        _contentService = contentService;
        _cancellationService = cancellationService;
        _debugService = debugService;
        _popupService = popupService;

        hubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        hubService.HubServerLoaded += GoHome;
    }

    private void GoHome()
    {
        SelectedEntry = null;
        foreach (var entry in _root)
        {
            Entries.Add(entry);
        }
    }

    private void HubServerChangedEventArgs(HubServerChangedEventArgs obj)
    {
        if(obj.Action == HubServerChangeAction.Clear) _root.Clear();
        if (obj.Action == HubServerChangeAction.Add)
        {
            foreach (var info in obj.Items)
            {
                _root.Add(new ContentEntry(this, ToContentUrl(info.Address.ToRobustUrl()),info.Address));
            }   
        };
    }

    public async void Go(ContentPath path)
    {
        if (path.Pathes.Count == 0)
        {
            SearchText = "";
            GoHome();
            return;
        }

        if (path.Pathes[0] != "content:" || path.Pathes.Count < 3)
        {
            return;
        }
        
        SearchText = path.Path;
        
        path.Pathes.RemoveAt(0);
        path.Pathes.RemoveAt(0);

        var serverUrl = path.Pathes[0];
        path.Pathes.RemoveAt(0);
        
        _debugService.Debug(path.Path + " " + serverUrl +" "+ SelectedEntry?.ServerName);
        
        try
        {
            ContentEntry entry;
            if (serverUrl == SelectedEntry?.ServerName)
                entry = SelectedEntry.GetRoot();
            else
                entry = await CreateEntry(serverUrl);
            
            if (!entry.TryGetEntry(path, out var centry))
            {
                throw new Exception("Not found!");
            }
            
            SelectedEntry = centry;
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _popupService.Popup(e);
            //throw;
        }
    }


    public void OnBackEnter()
    {
        Go(new ContentPath(GetHistory()));
    }
    
    public void OnGoEnter()
    {
        Go(new ContentPath(SearchText));
    }

    private async Task<ContentEntry> CreateEntry(string serverUrl)
    {
        var rurl = serverUrl.ToRobustUrl();
        var info = await _contentService.GetBuildInfo(rurl, _cancellationService.Token);
        var loading = _provider.GetService<LoadingContextViewModel>()!;
        loading.LoadingName = "Loading entry";
        _popupService.Popup(loading);
        var items = await _contentService.EnsureItems(info.RobustManifestInfo, loading,
            _cancellationService.Token);

        var rootEntry = new ContentEntry(this,ToContentUrl(rurl), serverUrl);

        foreach (var item in items)
        {
            var path = new ContentPath(item.Path);
            rootEntry.CreateItem(path, item);
        }
        
        loading.Dispose();

        return rootEntry;
    }

    private void AppendHistory(string str)
    {
        if(_history.Count >= 10) _history.RemoveAt(9);
        _history.Insert(0, str);
    }

    private string GetHistory()
    {
        if (_history.Count == 0) return "";
        var h = _history[0];
        _history.RemoveAt(0);
        return h;
    }
    
    private string ToContentUrl(RobustUrl serverUrl)
    {
        var port = serverUrl.Uri.Port != -1 ? (":"+serverUrl.Uri.Port) : "";
        return "content://" + serverUrl.Uri.Host + port;
    }
}

public class ContentEntry
{
    private readonly ContentBrowserViewModel _viewModel;

    public static IImage DirImage = new Bitmap(AssetLoader.Open(new Uri("avares://Nebula.Launcher/Assets/dir.png")));
    public static IImage IconImage = new Bitmap(AssetLoader.Open(new Uri("avares://Nebula.Launcher/Assets/file.png")));

    public RobustManifestItem? Item;
    public bool IsDirectory => Item == null;

    public string Name { get; private set; }
    public string ServerName { get; private set; }
    public IImage IconPath { get; set; } = DirImage;

    public ContentEntry? Parent { get; private set; }
    public bool IsRoot => Parent == null;

    private readonly Dictionary<string, ContentEntry> _childs = new();

    public IReadOnlyDictionary<string, ContentEntry> Childs => _childs.ToFrozenDictionary();

    public bool TryGetChild(string name,[NotNullWhen(true)] out ContentEntry? child)
    {
        return _childs.TryGetValue(name, out child);
    }

    public bool TryAddChild(ContentEntry contentEntry)
    {
        if(_childs.TryAdd(contentEntry.Name, contentEntry))
        {
            contentEntry.Parent = this;
            return true;
        }

        return false;
    }

    internal ContentEntry(ContentBrowserViewModel viewModel, string name, string serverName)
    {
        Name = name;
        ServerName = serverName;
        _viewModel = viewModel;
    }

    public ContentPath GetPath()
    {
        if (Parent != null)
        {
            var path = Parent.GetPath();
            path.Pathes.Add(Name);
            return path;
        }
        return new ContentPath(Name);
    }

    public ContentEntry GetOrCreateDirectory(ContentPath rootPath)
    {
        if (rootPath.Pathes.Count == 0) return this;
        
        var fName = rootPath.Pathes[0];
        rootPath.Pathes.RemoveAt(0);
        
        if(!TryGetChild(fName, out var child))
        {
            child = new ContentEntry(_viewModel, fName, ServerName);
            TryAddChild(child);
        }

        return child.GetOrCreateDirectory(rootPath);
    }

    public ContentEntry GetRoot()
    {
        if (Parent == null) return this;
        return Parent.GetRoot();
    }

    public ContentEntry CreateItem(ContentPath path, RobustManifestItem item)
    {
        var dir = path.GetDirectory();
        var dirEntry = GetOrCreateDirectory(dir);

        var entry = new ContentEntry(_viewModel, path.GetName(), ServerName)
        {
            Item = item
        };
        
        dirEntry.TryAddChild(entry);
        entry.IconPath = IconImage;
        return entry;
    }

    public bool TryGetEntry(ContentPath path, out ContentEntry? entry)
    {
        entry = null;
        
        if (path.Pathes.Count == 0)
        {
            entry = this;
            return true;
        }
        
        var fName = path.Pathes[0];
        path.Pathes.RemoveAt(0);
        
        if(!TryGetChild(fName, out var child))
        {
            return false;
        }

        return child.TryGetEntry(path, out entry);
    }

    public void OnPathGo()
    {
        _viewModel.Go(GetPath());
    }
}

public struct ContentPath
{
    public List<string> Pathes;

    public ContentPath(List<string> pathes)
    {
        Pathes = pathes;
    }

    public ContentPath(string path)
    {
        Pathes = path.Split("/").ToList();
    }

    public ContentPath GetDirectory()
    {
        var p = Pathes.ToList();
        p.RemoveAt(Pathes.Count - 1);
        return new ContentPath(p);
    }

    public string GetName()
    {
        return Pathes.Last();
    }

    public string Path => string.Join("/", Pathes);
}
