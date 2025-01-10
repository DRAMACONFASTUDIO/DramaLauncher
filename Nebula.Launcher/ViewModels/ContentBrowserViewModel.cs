using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared.Models;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(ContentBrowserView))]
public sealed partial class ContentBrowserViewModel : ViewModelBase
{
    public ObservableCollection<ContentEntry> Entries = new();
    
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _searchText = "";


    public ContentBrowserViewModel() : base()
    {
           
    }

    public ContentBrowserViewModel(IServiceProvider provider) : base(provider)
    {
    }


    public void OnBackEnter()
    {
        
    }
    
    public void OnGoEnter()
    {
        
    }
}

public sealed class ContentEntry
{
    
}

public sealed class ContentPath
{
    public RobustUrl ServerUrl;
}