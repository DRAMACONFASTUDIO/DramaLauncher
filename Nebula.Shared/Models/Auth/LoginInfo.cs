namespace Nebula.Shared.Models.Auth;

public class LoginInfo
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = default!;
    public LoginToken Token { get; set; }

    public override string ToString()
    {
        return $"{Username}/{UserId}";
    }
}