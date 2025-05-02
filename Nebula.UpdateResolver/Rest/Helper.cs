using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nebula.UpdateResolver.Rest;

public static class Helper
{
    public static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);
    public static void SafeOpenBrowser(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
        {
            Console.WriteLine("Unable to parse URI in server-provided link: {Link}", uri);
            return;
        }

        if (parsedUri.Scheme is not ("http" or "https"))
        {
            Console.WriteLine("Refusing to open server-provided link {Link}, only http/https are allowed", parsedUri);
            return;
        }

        OpenBrowser(parsedUri.ToString());
    }
    public static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public static async Task<T> AsJson<T>(this HttpContent content) where T : notnull
    {
        var str = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(str, JsonWebOptions) ??
               throw new JsonException("AsJson: did not expect null response");
    }
}