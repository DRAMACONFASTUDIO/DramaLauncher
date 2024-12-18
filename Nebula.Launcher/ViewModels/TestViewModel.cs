using System;
using ReactiveUI;

namespace Nebula.Launcher.ViewModels;

public sealed class TestViewModel : ViewModelBase
{
    private string _greeting = "Welcome to Avalonia!";

    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting,  value);
    }

    public void ButtonAction()
    {
        Console.WriteLine("HAS");
        Greeting = "Another greeting from Avalonia";
    }
}