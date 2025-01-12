using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Popup;
using Nebula.Shared.Models;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(LoadingContextView), false)]
public sealed partial class LoadingContextViewModel : PopupViewModelBase, ILoadingHandler
{
    public LoadingContextViewModel() :base(){}
    public LoadingContextViewModel(IServiceProvider provider) : base(provider){}
    
    public string LoadingName { get; set; } = "Loading...";
    public override bool IsClosable => false;

    public override string Title => LoadingName;
    
    [ObservableProperty]
    private int _currJobs;
    [ObservableProperty]
    private int _resolvedJobs;
    
    public void SetJobsCount(int count)
    {
        CurrJobs = count;
    }

    public int GetJobsCount()
    {
        return CurrJobs;
    }

    public void SetResolvedJobsCount(int count)
    {
        ResolvedJobs = count;

    }

    public int GetResolvedJobsCount()
    {
        return ResolvedJobs;
    }
}