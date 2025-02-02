using Nebula.Launcher.ViewModels.Pages;
using Nebula.Shared.Services;

namespace Nebula.Launcher;

public static class LauncherConVar
{
    public static readonly ConVar<ProfileAuthCredentials[]> AuthProfiles =
        ConVarBuilder.Build<ProfileAuthCredentials[]>("auth.profiles.v2", []);

    public static readonly ConVar<CurrentAuthInfo?> AuthCurrent =
        ConVarBuilder.Build<CurrentAuthInfo?>("auth.current.v2");
    
    public static readonly ConVar<string[]> Favorites =
        ConVarBuilder.Build<string[]>("server.favorites", []);
    
    public static readonly ConVar<string[]> AuthServers = ConVarBuilder.Build<string[]>("launcher.authServers", [
        "https://auth.spacestation14.com/"
    ]);

    public static readonly ConVar<string> CurrentLang = ConVarBuilder.Build<string>("launcher.language", "en-US");
}