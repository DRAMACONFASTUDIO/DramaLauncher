using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nebula.Launcher.Services;
using Nebula.Launcher.Utils;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;

namespace Nebula.Launcher.ViewModels;

[ViewRegister(typeof(AccountInfoView))]
public partial class AccountInfoViewModel : ViewModelBase
{
    private readonly PopupMessageService _popupMessageService;
    private readonly ConfigurationService _configurationService;
    private readonly AuthService _authService;
    
    public ObservableCollection<AuthLoginPasswordModel> Accounts { get; } = new();
    
    [ObservableProperty]
    private string _currentLogin = String.Empty;
    
    [ObservableProperty]
    private string _currentPassword = String.Empty;
    
    [ObservableProperty]
    private string _currentAuthServer = String.Empty;

    public ObservableCollection<string> AuthUrls { get; } = new();

    [ObservableProperty] private bool _pageEnabled = true;

    private AuthLoginPassword CurrentAlp
    {
        get => new(CurrentLogin, CurrentPassword, CurrentAuthServer);
        set
        {
            CurrentLogin = value.Login;
            CurrentPassword = value.Password;
            CurrentAuthServer = value.AuthServer;
        }
    }
    
    //Design think
    public AccountInfoViewModel()
    {
        AddAccount(new AuthLoginPassword("Binka","12341",""));
        AuthUrls.Add("https://cinka.ru");
        AuthUrls.Add("https://cinka.ru");
    }
    
    //Real think
    public AccountInfoViewModel(IServiceProvider serviceProvider, PopupMessageService popupMessageService, 
        ConfigurationService configurationService, AuthService authService) : base(serviceProvider)
    {
        //_popupMessageService = mainViewModel;
        _popupMessageService = popupMessageService;
        _configurationService = configurationService;
        _authService = authService;

        ReadAuthConfig();
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
        _popupMessageService.PopupInfo("Auth think, please wait...");
        
        if(await _authService.Auth(CurrentAlp))
        {
            _popupMessageService.ClosePopup();
            _popupMessageService.PopupInfo("Hello, " + _authService.SelectedAuth!.Username);
        }
        else
        {
            _popupMessageService.ClosePopup();
            _popupMessageService.PopupInfo("Well, shit is happened");
        }
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

    private void ReadAuthConfig()
    {
        foreach (var profile in 
                 _configurationService.GetConfigValue<AuthLoginPassword[]>(CurrentConVar.AuthProfiles)!)
        {
            AddAccount(profile);
        }

        var currProfile = _configurationService.GetConfigValue<AuthLoginPassword>(CurrentConVar.AuthProfiles);

        if (currProfile != null)
        {
            CurrentAlp = currProfile;
            DoAuth();
        }
        
        AuthUrls.Clear();
        var authUrls = _configurationService.GetConfigValue<string[]>(CurrentConVar.AuthServers)!;
        foreach (var url in authUrls)
        {
            AuthUrls.Add(url);
        } 
    }

    [RelayCommand]
    public void OnSaveProfile()
    {
        AddAccount(CurrentAlp);
        _configurationService.SetConfigValue(CurrentConVar.AuthProfiles, Accounts.Select(a => (AuthLoginPassword) a).ToArray());
    }

    public string AuthItemSelect
    {
        set => CurrentAuthServer = value;
    }
}

public record AuthLoginPasswordModel(string Login, string Password, string AuthServer, ICommand OnSelect = default!, ICommand OnDelete = default!) 
    : AuthLoginPassword(Login, Password, AuthServer);