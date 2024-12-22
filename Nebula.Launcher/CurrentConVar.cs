using Nebula.Launcher.Services;

namespace Nebula.Launcher;

public static class CurrentConVar
{
    public static readonly ConVar EngineManifestUrl = 
        ConVar.Build<string>("engine.manifestUrl", "https://robust-builds.cdn.spacestation14.com/manifest.json");
    public static readonly ConVar EngineModuleManifestUrl = 
        ConVar.Build<string>("engine.moduleManifestUrl", "https://robust-builds.cdn.spacestation14.com/modules.json");
    public static readonly ConVar ManifestDownloadProtocolVersion =
        ConVar.Build<int>("engine.manifestDownloadProtocolVersion", 1);
    public static readonly ConVar RobustAssemblyName = 
        ConVar.Build("engine.robustAssemblyName", "Robust.Client");
    
    public static readonly ConVar Hub = ConVar.Build<string[]>("launcher.hub", [
        "https://hub.spacestation14.com/api/servers"
    ]);
    public static readonly ConVar AuthServers = ConVar.Build<string[]>("launcher.authServers", [
        "https://auth.spacestation14.com/api/auth"
    ]);
}