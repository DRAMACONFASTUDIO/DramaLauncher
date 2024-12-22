using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Nebula.Launcher.Models.Auth;

namespace Nebula.Launcher.Services;

[ServiceRegister]
public class AuthService
{
    private readonly HttpClient _httpClient = new();
    private readonly RestService _restService;
    private readonly DebugService _debugService;

    public CurrentAuthInfo? SelectedAuth;

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
        
        _debugService.Debug($"Auth to {authServer}/authenticate {login}");
        
        var authUrl = new Uri($"{authServer}/authenticate");

        var result =
            await _restService.PostAsync<AuthenticateResponse, AuthenticateRequest>(
                new AuthenticateRequest(login, password), authUrl, CancellationToken.None);
        _debugService.Debug("RESULT " + result.Value);
        if (result.Value is null) return false;

        SelectedAuth = new CurrentAuthInfo(result.Value.UserId, result.Value.Username, 
            new LoginToken(result.Value.Token, result.Value.ExpireTime), authServer);

        return true;
    }

    public async Task<bool> EnsureToken()
    {
        if (SelectedAuth is null) return false;

        var authUrl = new Uri($"{SelectedAuth.AuthServer}/ping");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, authUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("SS14Auth", SelectedAuth.Token.Token);
        using var resp = await _httpClient.SendAsync(requestMessage);

        if (!resp.IsSuccessStatusCode) SelectedAuth = null;

        return resp.IsSuccessStatusCode;
    }
}

public sealed record CurrentAuthInfo(Guid UserId, string Username, LoginToken Token, string AuthServer);
public record AuthLoginPassword(string Login, string Password, string AuthServer);
