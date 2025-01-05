namespace Nebula.Shared.Models.Auth;

public sealed record AuthenticateRequest(string? Username, Guid? UserId, string Password, string? TfaCode = null)
{
    public AuthenticateRequest(string username, string password) : this(username, null, password)
    {
    }
}