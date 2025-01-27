using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nebula.Launcher.Services;
using Nebula.Launcher.ViewModels.Popup;
using Nebula.Launcher.Views.Pages;
using Nebula.Shared;
using Nebula.Shared.Services;
using Nebula.Shared.Utils;

namespace Nebula.Launcher.ViewModels.Pages;

[ViewModelRegister(typeof(AccountInfoView))]
[ConstructGenerator]
public partial class AccountInfoViewModel : ViewModelBase
{
    [ObservableProperty] private bool _authMenuExpand;

    [ObservableProperty] private bool _authUrlConfigExpand;

    [ObservableProperty] private int _authViewSpan = 1;

    [ObservableProperty] private string _currentAuthServer = string.Empty;

    [ObservableProperty] private string _currentLogin = string.Empty;

    [ObservableProperty] private string _currentPassword = string.Empty;

    [ObservableProperty] private bool _isLogged;

    private bool _isProfilesEmpty;
    [GenerateProperty] private PopupMessageService PopupMessageService { get; } = default!;
    [GenerateProperty] private ConfigurationService ConfigurationService { get; } = default!;
    [GenerateProperty] private AuthService AuthService { get; } = default!;
    [GenerateProperty, DesignConstruct] private ViewHelperService ViewHelperService { get; } = default!;

    public ObservableCollection<AuthLoginPasswordModel> Accounts { get; } = new();
    public ObservableCollection<string> AuthUrls { get; } = new();

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

    public string AuthItemSelect
    {
        set => CurrentAuthServer = value;
    }

    //Design think
    protected override void InitialiseInDesignMode()
    {
        AddAccount(new AuthLoginPassword("Binka", "12341", ""));
        AuthUrls.Add("https://cinka.ru");
        AuthUrls.Add("https://cinka.ru");
    }

    //Real think
    protected override void Initialise()
    {
        ReadAuthConfig();
    }

    public void AuthByAlp(AuthLoginPassword authLoginPassword)
    {
        CurrentAlp = authLoginPassword;
        DoAuth();
    }

    public void DoAuth()
    {
        var message = ViewHelperService.GetViewModel<InfoPopupViewModel>();
        message.InfoText = "Auth think, please wait...";
        message.IsInfoClosable = false;
        Console.WriteLine("AUTH SHIT");
        PopupMessageService.Popup(message);

        Task.Run(async () =>
        {
            if (await AuthService.Auth(CurrentAlp))
            {
                message.Dispose();
                IsLogged = true;
                ConfigurationService.SetConfigValue(CurrentConVar.AuthCurrent, CurrentAlp);
            }
            else
            {
                message.Dispose();
                Logout();
                PopupMessageService.Popup("Well, shit is happened: " + AuthService.Reason);
            }
        });
    }

    public void Logout()
    {
        IsLogged = false;
        AuthService.ClearAuth();
    }

    private void UpdateAuthMenu()
    {
        if (AuthMenuExpand || _isProfilesEmpty)
            AuthViewSpan = 2;
        else
            AuthViewSpan = 1;
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
                 ConfigurationService.GetConfigValue(CurrentConVar.AuthProfiles)!)
            AddAccount(profile);

        if (Accounts.Count == 0) UpdateAuthMenu();

        var currProfile = ConfigurationService.GetConfigValue(CurrentConVar.AuthCurrent);

        if (currProfile != null)
        {
            CurrentAlp = currProfile;
            DoAuth();
        }

        AuthUrls.Clear();
        var authUrls = ConfigurationService.GetConfigValue(CurrentConVar.AuthServers)!;
        foreach (var url in authUrls) AuthUrls.Add(url);
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
        ConfigurationService.SetConfigValue(CurrentConVar.AuthProfiles,
            Accounts.Select(a => (AuthLoginPassword)a).ToArray());
    }
}

public record AuthLoginPasswordModel(
    string Login,
    string Password,
    string AuthServer,
    ICommand OnSelect = default!,
    ICommand OnDelete = default!)
    : AuthLoginPassword(Login, Password, AuthServer);