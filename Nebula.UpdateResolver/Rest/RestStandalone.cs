using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.UpdateResolver.Rest;

public static class RestStandalone
{
    private static readonly HttpClient _client = new();

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static async Task<T> GetAsync<T>(Uri uri, CancellationToken cancellationToken) where T : notnull
    {
        var response = await _client.GetAsync(uri, cancellationToken);
        return await ReadResult<T>(response, cancellationToken);
    }

    public static async Task<T> GetAsyncDefault<T>(Uri uri, T defaultValue, CancellationToken cancellationToken) where T : notnull
    {
        try
        {
            return await GetAsync<T>(uri, cancellationToken);
        }
        catch (Exception e)
        {
            return defaultValue;
        }
    }

    public static async Task<K> PostAsync<K, T>(T information, Uri uri, CancellationToken cancellationToken) where K : notnull
    {
        var json = JsonSerializer.Serialize(information, _serializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(uri, content, cancellationToken);
        return await ReadResult<K>(response, cancellationToken);
    }

    public static async Task<T> PostAsync<T>(Stream stream, Uri uri, CancellationToken cancellationToken) where T : notnull
    {
        using var multipartFormContent =
            new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        multipartFormContent.Add(new StreamContent(stream), "formFile", "image.png");
        var response = await _client.PostAsync(uri, multipartFormContent, cancellationToken);
        return await ReadResult<T>(response, cancellationToken);
    }

    public static async Task<T> DeleteAsync<T>(Uri uri, CancellationToken cancellationToken) where T : notnull
    {
        var response = await _client.DeleteAsync(uri, cancellationToken);
        return await ReadResult<T>(response, cancellationToken);
    }

    private static async Task<T> ReadResult<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : notnull
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (typeof(T) == typeof(string) && content is T t)
            return t;
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.AsJson<T>();
        }
        
        throw new RestRequestException(response.Content, response.StatusCode);
    }
}