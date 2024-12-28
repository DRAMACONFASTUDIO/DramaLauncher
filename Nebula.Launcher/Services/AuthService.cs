using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Nebula.Launcher.Models.Auth;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public partial class AuthService : ObservableObject
{
    private readonly HttpClient _httpClient = new();
    private readonly RestService _restService;
    private readonly DebugService _debugService;

    [ObservableProperty]
    private CurrentAuthInfo? _selectedAuth;

    public string Reason = "";

    public AuthService(RestService restService, DebugService debugService)
    {
        _restService = restService;
        _debugService = debugService;
    }

    public async Task<bool> Auth(AuthLoginPassword authLoginPassword)
    {
        var authServer = authLoginPassword.AuthServer;
        var login = authLoginPassword.Login;
        var password = authLoginPassword.Password;
        
        _debugService.Debug($"Auth to {authServer}api/auth/authenticate {login}");
        
        var authUrl = new Uri($"{authServer}api/auth/authenticate");

        var result =
            await _restService.PostAsync<AuthenticateResponse, AuthenticateRequest>(
                new AuthenticateRequest(login, password), authUrl, CancellationToken.None);
        
        if (result.Value is null)
        {
            Reason = result.Message;
            return false;
        }

        SelectedAuth = new CurrentAuthInfo(result.Value.UserId, 
            new LoginToken(result.Value.Token, result.Value.ExpireTime), authLoginPassword);

        return true;
    }

    public async Task<bool> EnsureToken()
    {
        if (SelectedAuth is null) return false;

        var authUrl = new Uri($"{SelectedAuth.AuthLoginPassword.AuthServer}api/auth/ping");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, authUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("SS14Auth", SelectedAuth.Token.Token);
        using var resp = await _httpClient.SendAsync(requestMessage);

        if (!resp.IsSuccessStatusCode) SelectedAuth = null;

        return resp.IsSuccessStatusCode;
    }
}

public sealed record CurrentAuthInfo(Guid UserId, LoginToken Token, AuthLoginPassword AuthLoginPassword);
public record AuthLoginPassword(string Login, string Password, string AuthServer);
