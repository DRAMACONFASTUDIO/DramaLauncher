using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.ContentView;
using Nebula.Launcher.ViewModels.Popup;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Models;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels.Pages;

[ViewModelRegister(typeof(ContentBrowserView))]
[ConstructGenerator]
public sealed partial class ContentBrowserViewModel : ViewModelBase , IViewModelPage
{
    private readonly List<ContentEntry> _root = new();

    private readonly List<string> _history = new();

    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _searchText = "";

    private ContentEntry? _selectedEntry;
    [ObservableProperty] private string _serverText = "";
    [ObservableProperty] private ContentViewBase? _contentView;
    public bool IsCustomContenView => ContentView != null;


    [GenerateProperty] private ContentService ContentService { get; } = default!;
    [GenerateProperty] private CancellationService CancellationService { get; } = default!;
    [GenerateProperty] private FileService FileService { get; } = default!;
    [GenerateProperty] private DebugService DebugService { get; } = default!;
    [GenerateProperty] private PopupMessageService PopupService { get; } = default!;
    [GenerateProperty] private HubService HubService { get; } = default!;
    [GenerateProperty, DesignConstruct] private ViewHelperService ViewHelperService { get; } = default!;

    public ObservableCollection<ContentEntry> Entries { get; } = new();

    private Dictionary<string, Type> _contentContainers = new();

    public ContentEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            SearchText = value?.GetPath().ToString() ?? "";
            ContentView = null;

            if (value is { Item: not null })
            {
                if (FileService.ContentFileApi.TryOpen(value.Item.Value.Hash, out var stream))
                {
                    var ext = Path.GetExtension(value.Item.Value.Path);
                    if(TryGetContentViewer(ext, out var contentViewBase)){
                        contentViewBase.InitialiseWithData(value.GetPath(), stream);
                        ContentView = contentViewBase;
                        return;
                    }

                    var myTempFile = Path.Combine(Path.GetTempPath(), "tempie" + ext);

                    using (var sw = new FileStream(myTempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.CopyTo(sw);
                    }

                    stream.Dispose();

                    var startInfo = new ProcessStartInfo(myTempFile)
                    {
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }

                return;
            }

            Entries.Clear();
            _selectedEntry = value;

            if (value == null) return;

            foreach (var (_, entryCh) in value.Childs) Entries.Add(entryCh);
        }
    }

    private bool TryGetContentViewer(string type,[NotNullWhen(true)] out ContentViewBase? contentViewBase){
        contentViewBase = null;
        if(!_contentContainers.TryGetValue(type, out var contentViewType) || 
           !contentViewType.IsAssignableTo(typeof(ContentViewBase))) 
            return false;

        
        contentViewBase = (ContentViewBase)Activator.CreateInstance(contentViewType)!;
        return true;
    }


    protected override void InitialiseInDesignMode()
    {
        var a = new ContentEntry(this, "A:", "A", "");
        var b = new ContentEntry(this, "B", "B", "");
        a.TryAddChild(b);
        Entries.Add(a);
    }

    protected override void Initialise()
    {
        FillRoot(HubService.ServerList);

        HubService.HubServerChangedEventArgs += HubServerChangedEventArgs;
        HubService.HubServerLoaded += GoHome;

        if (!HubService.IsUpdating) GoHome();
    }

    private void GoHome()
    {
        SelectedEntry = null;
        foreach (var entry in _root) Entries.Add(entry);
    }

    private void HubServerChangedEventArgs(HubServerChangedEventArgs obj)
    {
        if (obj.Action == HubServerChangeAction.Clear) _root.Clear();
        if (obj.Action == HubServerChangeAction.Add) FillRoot(obj.Items);
    }

    private void FillRoot(IEnumerable<ServerHubInfo> infos)
    {
        foreach (var info in infos) _root.Add(new ContentEntry(this, info.StatusData.Name, info.Address, info.Address));
    }

    public async void Go(ContentPath path, bool appendHistory = true)
    {
        if (path.Pathes.Count > 0 && (path.Pathes[0].StartsWith("ss14://") || path.Pathes[0].StartsWith("ss14s://")))
        {
            ServerText = path.Pathes[0];
            path = new ContentPath("");
        }

        if (string.IsNullOrEmpty(ServerText))
        {
            SearchText = "";
            GoHome();
            return;
        }

        if (ServerText != SelectedEntry?.ServerName) SelectedEntry = await CreateEntry(ServerText);

        DebugService.Debug("Going to:" + path.Path);

        var oriPath = path.Clone();
        try
        {
            if (SelectedEntry == null || !SelectedEntry.GetRoot().TryGetEntry(path, out var centry))
                throw new Exception("Not found! " + oriPath.Path);

            SelectedEntry = centry;
        }
        catch (Exception e)
        {
            PopupService.Popup(e);
        }
    }


    public void OnBackEnter()
    {
        Go(new ContentPath(GetHistory()), false);
    }

    public void OnGoEnter()
    {
        Go(new ContentPath(SearchText));
    }

    private async Task<ContentEntry> CreateEntry(string serverUrl)
    {
        var loading = ViewHelperService.GetViewModel<LoadingContextViewModel>();
        loading.LoadingName = "Loading entry";
        PopupService.Popup(loading);

        var rurl = serverUrl.ToRobustUrl();
        var info = await ContentService.GetBuildInfo(rurl, CancellationService.Token);
        var hashApi = await ContentService.EnsureItems(info.RobustManifestInfo, loading,
            CancellationService.Token);

        var rootEntry = new ContentEntry(this, "", "", serverUrl);

        foreach (var item in hashApi.Manifest.Values)
        {
            var path = new ContentPath(item.Path);
            rootEntry.CreateItem(path, item);
        }

        loading.Dispose();

        return rootEntry;
    }

    private void AppendHistory(string str)
    {
        if (_history.Count >= 10) _history.RemoveAt(9);
        _history.Insert(0, str);
    }

    private string GetHistory()
    {
        if (_history.Count == 0) return "";
        var h = _history[0];
        _history.RemoveAt(0);
        return h;
    }

    public void OnPageOpen(object? args)
    {
    }
}

public class ContentEntry
{
    private readonly Dictionary<string, ContentEntry> _childs = new();
    private readonly ContentBrowserViewModel _viewModel;

    public RobustManifestItem? Item;

    internal ContentEntry(ContentBrowserViewModel viewModel, string name, string pathName, string serverName)
    {
        Name = name;
        ServerName = serverName;
        PathName = pathName;
        _viewModel = viewModel;
    }

    public bool IsDirectory => Item == null;

    public string Name { get; private set; }
    public string PathName { get; }
    public string ServerName { get; }
    public string IconPath { get; set; } = "/Assets/svg/folder.svg";

    public ContentEntry? Parent { get; private set; }
    public bool IsRoot => Parent == null;

    public IReadOnlyDictionary<string, ContentEntry> Childs => _childs.ToFrozenDictionary();

    public bool TryGetChild(string name, [NotNullWhen(true)] out ContentEntry? child)
    {
        return _childs.TryGetValue(name, out child);
    }

    public bool TryAddChild(ContentEntry contentEntry)
    {
        if (_childs.TryAdd(contentEntry.PathName, contentEntry))
        {
            contentEntry.Parent = this;
            return true;
        }

        return false;
    }

    public ContentPath GetPath()
    {
        if (Parent != null)
        {
            var path = Parent.GetPath();
            path.Pathes.Add(PathName);
            return path;
        }

        return new ContentPath([PathName]);
    }

    public ContentEntry GetOrCreateDirectory(ContentPath rootPath)
    {
        if (rootPath.Pathes.Count == 0) return this;

        var fName = rootPath.GetNext();

        if (!TryGetChild(fName, out var child))
        {
            child = new ContentEntry(_viewModel, fName, fName, ServerName);
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

        var name = path.GetName();
        var entry = new ContentEntry(_viewModel, name, name, ServerName)
        {
            Item = item
        };

        dirEntry.TryAddChild(entry);
        entry.IconPath = "/Assets/svg/file.svg";
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

        var fName = path.GetNext();

        if (!TryGetChild(fName, out var child)) return false;

        return child.TryGetEntry(path, out entry);
    }

    public void OnPathGo()
    {
        _viewModel.Go(GetPath());
    }
}

public struct ContentPath
{
    public List<string> Pathes { get; }

    public ContentPath(List<string> pathes)
    {
        Pathes = pathes;
    }

    public ContentPath(string path)
    {
        Pathes = string.IsNullOrEmpty(path)
            ? new List<string>()
            : path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public ContentPath GetDirectory()
    {
        if (Pathes.Count == 0)
            return this; // Root remains root when getting the directory.

        var directoryPathes = Pathes.Take(Pathes.Count - 1).ToList();
        return new ContentPath(directoryPathes);
    }

    public string GetName()
    {
        if (Pathes.Count == 0)
            throw new InvalidOperationException("Cannot get the name of the root path.");

        return Pathes.Last();
    }

    public string GetNext()
    {
        if (Pathes.Count == 0)
            throw new InvalidOperationException("No elements left to retrieve from the root.");

        var nextName = Pathes[0];
        Pathes.RemoveAt(0);

        return string.IsNullOrWhiteSpace(nextName) ? GetNext() : nextName;
    }

    public ContentPath Clone()
    {
        return new ContentPath(new List<string>(Pathes));
    }

    public string Path => Pathes.Count == 0 ? "/" : string.Join("/", Pathes);

    public override string ToString()
    {
        return Path;
    }
}