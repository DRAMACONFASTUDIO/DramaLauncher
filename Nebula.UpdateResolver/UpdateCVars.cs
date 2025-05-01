using System;
using Nebula.Shared.Models;
using Nebula.Shared.Services;

namespace Nebula.UpdateResolver;

public static class UpdateConVars
{
    public static readonly ConVar<Uri> UpdateCacheUrl =
        ConVarBuilder.Build<Uri>("update.url",new Uri("https://cinka.ru/nebula-launcher/files/publish/release"));
    public static readonly ConVar<LauncherManifest> CurrentLauncherManifest = 
        ConVarBuilder.Build<LauncherManifest>("update.manifest");
}