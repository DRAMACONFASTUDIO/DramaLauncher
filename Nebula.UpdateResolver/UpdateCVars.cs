using System.Collections.Generic;
using Nebula.UpdateResolver.Configuration;

namespace Nebula.UpdateResolver;

public static class UpdateConVars
{
    public static readonly ConVar<string> UpdateCacheUrl =
        ConVarBuilder.Build<string>("update.url","https://durenko.tatar/nebula/manifest/");
    public static readonly ConVar<LauncherManifest> CurrentLauncherManifest = 
        ConVarBuilder.Build<LauncherManifest>("update.manifest");
    
    public static readonly ConVar<Dictionary<string,string>> DotnetUrl = ConVarBuilder.Build<Dictionary<string,string>>("dotnet.url",
        new(){
            {"win-x64", "https://builds.dotnet.microsoft.com/dotnet/Runtime/9.0.6/dotnet-runtime-9.0.6-win-x64.zip"},
            {"win-x86", "https://builds.dotnet.microsoft.com/dotnet/Runtime/9.0.6/dotnet-runtime-9.0.6-win-x86.zip"},
            {"linux-x64", "https://builds.dotnet.microsoft.com/dotnet/Runtime/9.0.6/dotnet-runtime-9.0.6-linux-x64.tar.gz"}
        });
}