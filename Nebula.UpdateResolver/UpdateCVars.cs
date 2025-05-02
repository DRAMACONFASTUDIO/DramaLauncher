using Nebula.UpdateResolver.Configuration;

namespace Nebula.UpdateResolver;

public static class UpdateConVars
{
    public static readonly ConVar<string> UpdateCacheUrl =
        ConVarBuilder.Build<string>("update.url","https://cinka.ru/nebula-launcher/files/publish/release");
    public static readonly ConVar<LauncherManifest> CurrentLauncherManifest = 
        ConVarBuilder.Build<LauncherManifest>("update.manifest");
}