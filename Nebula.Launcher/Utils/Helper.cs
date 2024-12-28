using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nebula.Launcher.Utils;

public static class Helper
{
    public static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);
    
    public static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            
        }
    }
    
    public static async Task<T> AsJson<T>(this HttpContent content) where T : notnull
    {
        var str = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(str, JsonWebOptions) ??
               throw new JsonException("AsJson: did not expect null response");
    }
}