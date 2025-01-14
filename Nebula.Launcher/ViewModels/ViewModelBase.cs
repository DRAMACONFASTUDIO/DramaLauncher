using CommunityToolkit.Mvvm.ComponentModel;

namespace Nebula.Launcher.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected abstract void InitialiseInDesignMode();
    protected abstract void Initialise();
}