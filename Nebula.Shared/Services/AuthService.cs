using System.Net.Http.Headers;
using Nebula.Shared.Models.Auth;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class AuthService(
    RestService restService,
    DebugService debugService,
    CancellationService cancellationService)
{
    private readonly HttpClient _httpClient = new();

    public string Reason = "";
    public CurrentAuthInfo? SelectedAuth { get; internal set; }

    public async Task<bool> Auth(AuthLoginPassword authLoginPassword)
    {
        var authServer = authLoginPassword.AuthServer;
        var login = authLoginPassword.Login;
        var password = authLoginPassword.Password;

        debugService.Debug($"Auth to {authServer}api/auth/authenticate {login}");

        var authUrl = new Uri($"{authServer}api/auth/authenticate");

        var result =
            await restService.PostAsync<AuthenticateResponse, AuthenticateRequest>(
                new AuthenticateRequest(login, password), authUrl, cancellationService.Token);

        if (result.Value is null)
        {
            Reason = result.Message;
            return false;
        }

        SelectedAuth = new CurrentAuthInfo(result.Value.UserId,
            new LoginToken(result.Value.Token, result.Value.ExpireTime), authLoginPassword);

        return true;
    }

    public void ClearAuth()
    {
        SelectedAuth = null;
    }

    public void SetAuth(Guid guid, string token, string login, string authServer)
    {
        SelectedAuth = new CurrentAuthInfo(guid, new LoginToken(token, DateTimeOffset.Now),
            new AuthLoginPassword(login, "", authServer));
    }

    public async Task<bool> EnsureToken()
    {
        if (SelectedAuth is null) return false;

        var authUrl = new Uri($"{SelectedAuth.AuthLoginPassword.AuthServer}api/auth/ping");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, authUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("SS14Auth", SelectedAuth.Token.Token);
        using var resp = await _httpClient.SendAsync(requestMessage, cancellationService.Token);

        if (!resp.IsSuccessStatusCode) SelectedAuth = null;

        return resp.IsSuccessStatusCode;
    }
}

public sealed record CurrentAuthInfo(Guid UserId, LoginToken Token, AuthLoginPassword AuthLoginPassword);

public record AuthLoginPassword(string Login, string Password, string AuthServer);