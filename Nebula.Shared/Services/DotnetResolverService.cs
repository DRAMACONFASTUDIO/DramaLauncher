using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class DotnetResolverService(DebugService debugService, ConfigurationService configurationService)
{
    private HttpClient _httpClient = new HttpClient();
    
    private static readonly string FullPath = Path.Join(FileService.RootPath, "dotnet", DotnetUrlHelper.GetRuntimeIdentifier());
    private static readonly string ExecutePath = Path.Join(FullPath, "dotnet" + DotnetUrlHelper.GetExtension());
    
    public async Task<string> EnsureDotnet(){
        if(!Directory.Exists(FullPath))
            await Download();
        
        return ExecutePath;
    }

    private async Task Download(){
        
        debugService.GetLogger("DotnetResolver").Log($"Downloading dotnet {DotnetUrlHelper.GetRuntimeIdentifier()}...");
        var ridExt =
            DotnetUrlHelper.GetCurrentPlatformDotnetUrl(configurationService.GetConfigValue(CurrentConVar.DotnetUrl)!);
        using var response = await _httpClient.GetAsync(ridExt);
        using var zipArchive = new ZipArchive(await response.Content.ReadAsStreamAsync());
        Directory.CreateDirectory(FullPath);
        zipArchive.ExtractToDirectory(FullPath);
        debugService.GetLogger("DotnetResolver").Log($"Downloading dotnet complete.");
    }
}

public static class DotnetUrlHelper
{
    public static string GetExtension()
    {
        if (OperatingSystem.IsWindows()) return ".exe";
        return "";
    }
    
    public static string GetCurrentPlatformDotnetUrl(Dictionary<string, string> dotnetUrl)
    {
        string? rid = GetRuntimeIdentifier();

        if (dotnetUrl.TryGetValue(rid, out var url))
        {
            return url;
        }

        throw new PlatformNotSupportedException($"No download URL available for the current platform: {rid}");
    }

    public static string GetRuntimeIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.Is64BitProcess ? "win-x64" : "win-x86";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux-x64";
        }

        throw new PlatformNotSupportedException("Unsupported operating system");
    }
}