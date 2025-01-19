using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Nebula.Shared.Utils;

namespace Nebula.Shared.Services;

[ServiceRegister]
public class RestService
{
    private readonly HttpClient _client = new();
    private readonly DebugService _debug;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public RestService(DebugService debug)
    {
        _debug = debug;
    }

    public async Task<RestResult<T>> GetAsync<T>(Uri uri, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync(uri, cancellationToken);
        return await ReadResult<T>(response, cancellationToken);
    }

    public async Task<T> GetAsyncDefault<T>(Uri uri, T defaultValue, CancellationToken cancellationToken)
    {
        var result = await GetAsync<T>(uri, cancellationToken);
        return result.Value ?? defaultValue;
    }

    public async Task<RestResult<K>> PostAsync<K, T>(T information, Uri uri, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(information, _serializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(uri, content, cancellationToken);
        return await ReadResult<K>(response, cancellationToken);
    }

    public async Task<RestResult<T>> PostAsync<T>(Stream stream, Uri uri, CancellationToken cancellationToken)
    {
        using var multipartFormContent =
            new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        multipartFormContent.Add(new StreamContent(stream), "formFile", "image.png");
        var response = await _client.PostAsync(uri, multipartFormContent, cancellationToken);
        return await ReadResult<T>(response, cancellationToken);
    }

    public async Task<RestResult<T>> DeleteAsync<T>(Uri uri, CancellationToken cancellationToken)
    {
        var response = await _client.DeleteAsync(uri, cancellationToken);
        return await ReadResult<T>(response, cancellationToken);
    }

    private async Task<RestResult<T>> ReadResult<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : notnull
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _debug.Debug($"SUCCESSFUL GET CONTENT {typeof(T)}");
            if (typeof(T) == typeof(RawResult))
                return (new RestResult<RawResult>(new RawResult(content), null, response.StatusCode) as RestResult<T>)!;

            return new RestResult<T>(await response.Content.AsJson<T>(), null,
                response.StatusCode);
        }

        _debug.Error("ERROR " + response.StatusCode + " " + content);
        return new RestResult<T>(default, "response code:" + response.StatusCode, response.StatusCode);
    }
}

public class RestResult<T>
{
    public string Message = "Ok";
    public HttpStatusCode StatusCode;
    public T? Value;

    public RestResult(T? value, string? message, HttpStatusCode statusCode)
    {
        Value = value;
        if (message != null) Message = message;
        StatusCode = statusCode;
    }

    public static implicit operator T?(RestResult<T> result)
    {
        return result.Value;
    }
}

public class RawResult
{
    public string Result;

    public RawResult(string result)
    {
        Result = result;
    }

    public static implicit operator string(RawResult result)
    {
        return result.Result;
    }
}