using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.Shared;

public static class CurrentConVar
{
    public static readonly ConVar<string> EngineManifestUrl =
        ConVarBuilder.Build("engine.manifestUrl", "https://robust-builds.cdn.spacestation14.com/manifest.json");

    public static readonly ConVar<string> EngineModuleManifestUrl =
        ConVarBuilder.Build("engine.moduleManifestUrl", "https://robust-builds.cdn.spacestation14.com/modules.json");

    public static readonly ConVar<int> ManifestDownloadProtocolVersion =
        ConVarBuilder.Build("engine.manifestDownloadProtocolVersion", 1);

    public static readonly ConVar<string> RobustAssemblyName =
        ConVarBuilder.Build("engine.robustAssemblyName", "Robust.Client");

    public static readonly ConVar<string[]> Hub = ConVarBuilder.Build<string[]>("launcher.hub", [
        "https://hub.spacestation14.com/api/servers"
    ]);

    public static readonly ConVar<Dictionary<string, EngineVersionInfo>> EngineManifestBackup =
        ConVarBuilder.Build<Dictionary<string, EngineVersionInfo>>("engine.manifest.backup");
    public static readonly ConVar<ModulesInfo> ModuleManifestBackup =
        ConVarBuilder.Build<ModulesInfo>("module.manifest.backup");
}