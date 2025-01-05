using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.ViewHelper;
using Nebula.Shared.Models;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(isSingleton:false)]
public partial class ServerEntryModelView : ViewModelBase
{
    [ObservableProperty] private bool _runVisible = true;

    public ServerHubInfo ServerHubInfo { get; set; }

    public ServerEntryModelView() : base()
    {
    }

    public ServerEntryModelView(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public void RunInstance()
    {
        var p = Process.Start("./Nebula.Runner", "a b c");
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
    }
    

    public void ReadLog()
    {
        
    }

    public void StopInstance()
    {
        
    }
}