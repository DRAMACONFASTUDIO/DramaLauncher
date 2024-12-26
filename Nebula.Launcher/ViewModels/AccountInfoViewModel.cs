using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(AccountInfoView))]
public partial class AccountInfoViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    
    public ObservableCollection<AuthLoginPasswordModel> Accounts { get; } = new();
    
    [ObservableProperty]
    private string _currentLogin = String.Empty;
    
    [ObservableProperty]
    private string _currentPassword = String.Empty;
    
    [ObservableProperty]
    private string _currentAuthServer = String.Empty;

    [ObservableProperty] private bool _pageEnabled = true;

    public AuthLoginPassword CurrentALP => new AuthLoginPassword(CurrentLogin, CurrentPassword, CurrentAuthServer);
    
    //Design think
    public AccountInfoViewModel()
    {
        AddAccount(new AuthLoginPassword("Binka","12341",""));
    }
    
    //Real think
    public AccountInfoViewModel(IServiceProvider serviceProvider, AuthService authService) : base(serviceProvider)
    {
        _authService = authService;
    }

    public void AuthByALP(AuthLoginPassword authLoginPassword)
    {
        CurrentLogin = authLoginPassword.Login;
        CurrentPassword = authLoginPassword.Password;
        CurrentAuthServer = authLoginPassword.AuthServer;
        
        DoAuth();
    }

    public async void DoAuth()
    {
        PageEnabled = false;
        if(await _authService.Auth(CurrentALP))
            Console.WriteLine("Hello, " + _authService.SelectedAuth!.Username);
        else
            Console.WriteLine("Shit!");
        PageEnabled = true;
    }

    private void AddAccount(AuthLoginPassword authLoginPassword)
    {
        var onDelete = new DelegateCommand<AuthLoginPasswordModel>(a => Accounts.Remove(a));
        var onSelect = new DelegateCommand<AuthLoginPasswordModel>(AuthByALP);
        
        var alpm = new AuthLoginPasswordModel(
            authLoginPassword.Login, 
            authLoginPassword.Password,
            authLoginPassword.AuthServer, 
            onSelect, 
            onDelete);

        onDelete.TRef.Value = alpm;
        onSelect.TRef.Value = alpm;
        
        Accounts.Add(alpm);
    }

    [RelayCommand]
    public void OnSaveProfile()
    {
        AddAccount(CurrentALP);
    }
}

public class Ref<T>
{
    public T Value = default!;
}

public class DelegateCommand<T> : ICommand
{
    private readonly Action<T> _func;
    public readonly Ref<T> TRef = new();

    public DelegateCommand(Action<T> func)
    {
        _func = func;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _func(TRef.Value);
    }

    public event EventHandler? CanExecuteChanged;
}

public record AuthLoginPasswordModel(string Login, string Password, string AuthServer, ICommand OnSelect = default!, ICommand OnDelete = default!) 
    : AuthLoginPassword(Login, Password, AuthServer);