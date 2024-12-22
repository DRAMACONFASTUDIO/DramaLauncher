using System;

namespace Nebula.Launcher.Models.Auth;

public sealed record AuthenticateResponse(string Token, string Username, Guid UserId, DateTimeOffset ExpireTime);