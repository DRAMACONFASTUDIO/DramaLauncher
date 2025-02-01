namespace Nebula.Shared.Models.Auth;

public sealed record LoginToken(string Token, DateTimeOffset ExpireTime);