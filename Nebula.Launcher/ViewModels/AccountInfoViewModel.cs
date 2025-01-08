using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nebula.Launcher.ViewHelper;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels;

[ViewModelRegister(typeof(AccountInfoView))]
public partial class AccountInfoViewModel : ViewModelBase
{
    private readonly PopupMessageService _popupMessageService;
    private readonly ConfigurationService _configurationService;
    private readonly AuthService _authService;
    
    public ObservableCollection<AuthLoginPasswordModel> Accounts { get; } = new();
    public ObservableCollection<string> AuthUrls { get; } = new();
    
    [ObservableProperty]
    private string _currentLogin = String.Empty;
    
    [ObservableProperty]
    private string _currentPassword = String.Empty;
    
    [ObservableProperty]
    private string _currentAuthServer = String.Empty;

    [ObservableProperty] private bool _authUrlConfigExpand;

    [ObservableProperty] private int _authViewSpan = 1;
    
    [ObservableProperty] private bool _authMenuExpand;

    private bool _isProfilesEmpty;

    [ObservableProperty] private bool _isLogged;

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

    public void AuthByAlp(AuthLoginPassword authLoginPassword)
    {
        CurrentAlp = authLoginPassword;
        DoAuth();
    }

    public async void DoAuth()
    {
        var message = GetViewModel<InfoPopupViewModel>();
        message.InfoText = "Auth think, please wait...";
        _popupMessageService.Popup(message);
        
        if(await _authService.Auth(CurrentAlp))
        {
            message.Dispose();
            IsLogged = true;
            _configurationService.SetConfigValue(CurrentConVar.AuthCurrent, CurrentAlp);
        }
        else
        {
            message.Dispose();
            Logout();
            _popupMessageService.Popup("Well, shit is happened: " + _authService.Reason);
        }
    }

    public void Logout()
    {
        IsLogged = false;
        //CurrentAlp = new AuthLoginPassword("", "", "");
        _authService.ClearAuth();
    }

    private void UpdateAuthMenu()
    {
        if (AuthMenuExpand || _isProfilesEmpty)
        {
            AuthViewSpan = 2;
        }
        else
        {
            AuthViewSpan = 1;
        }
    }

    private void AddAccount(AuthLoginPassword authLoginPassword)
    {
        var onDelete = new DelegateCommand<AuthLoginPasswordModel>(OnDeleteProfile);
        var onSelect = new DelegateCommand<AuthLoginPasswordModel>(AuthByAlp);
        
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
                 _configurationService.GetConfigValue(CurrentConVar.AuthProfiles)!)
        {
            AddAccount(profile);
        }

        if (Accounts.Count == 0)
        {
            UpdateAuthMenu();
        }

        var currProfile = _configurationService.GetConfigValue(CurrentConVar.AuthCurrent);

        if (currProfile != null)
        {
            CurrentAlp = currProfile;
            DoAuth();
        }
        
        AuthUrls.Clear();
        var authUrls = _configurationService.GetConfigValue(CurrentConVar.AuthServers)!;
        foreach (var url in authUrls)
        {
            AuthUrls.Add(url);
        } 
    }

    [RelayCommand]
    private void OnSaveProfile()
    {
        AddAccount(CurrentAlp);
        _isProfilesEmpty = Accounts.Count == 0;
        UpdateAuthMenu();
        DirtyProfile();
    }
    
    private void OnDeleteProfile(AuthLoginPasswordModel account)
    {
        Accounts.Remove(account);
        _isProfilesEmpty = Accounts.Count == 0;
        UpdateAuthMenu();
        DirtyProfile();
    }

    [RelayCommand]
    private void OnExpandAuthUrl()
    {
        AuthUrlConfigExpand = !AuthUrlConfigExpand;
    }

    [RelayCommand]
    private void OnExpandAuthView()
    {
        AuthMenuExpand = !AuthMenuExpand;
        UpdateAuthMenu();
    }

    private void DirtyProfile()
    {
        _configurationService.SetConfigValue(CurrentConVar.AuthProfiles, 
            Accounts.Select(a => (AuthLoginPassword) a).ToArray());
    }

    public string AuthItemSelect
    {
        set => CurrentAuthServer = value;
    }
}

public record AuthLoginPasswordModel(string Login, string Password, string AuthServer, ICommand OnSelect = default!, ICommand OnDelete = default!) 
    : AuthLoginPassword(Login, Password, AuthServer);