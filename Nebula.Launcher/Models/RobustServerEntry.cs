using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.Launcher.Models;

public sealed record Auth(string Mode, string PublicKey);

public sealed record Build(
    string EngineVersion,
    string ForkId,
    string Version,
    string DownloadUrl,
    string ManifestUrl,
    bool Acz,
    string Hash,
    string ManifestHash);

public sealed record Link(string Name, string Icon, string Url);
public sealed record Info(string ConnectAddress, Auth Auth, Build Build, string Desc, List<Link> Links);

public sealed record Status(
    string Name,
    int Players,
    List<object> Tags,
    string Map,
    int RoundId,
    int SoftMaxPlayer,
    bool PanicBunker,
    int RunLevel,
    string Preset);

public enum ContentCompressionScheme
{
    None = 0,
    Deflate = 1,

    /// <summary>
    ///     ZStandard compression. In the future may use SS14 specific dictionary IDs in the frame header.
    /// </summary>
    ZStd = 2
}

public sealed record VersionInfo(
    bool Insecure,
    [property: JsonPropertyName("redirect")]
    string? RedirectVersion,
    Dictionary<string, BuildInfo> Platforms);

public sealed class BuildInfo
{
    [JsonInclude] [JsonPropertyName("sha256")]
    public string Sha256 = default!;

    [JsonInclude] [JsonPropertyName("sig")]
    public string Signature = default!;

    [JsonInclude] [JsonPropertyName("url")]
    public string Url = default!;
}

public sealed record ServerInfo(string Address, StatusData StatusData, List<string> InferredTags);

public sealed record StatusData(
    string Map,
    string Name,
    List<string> Tags,
    string Preset,
    int Players,
    int RoundId,
    int RunLevel, 
    bool PanicBunker, 
    DateTime RoundStartTime, 
    int SoftMaxPlayer);

public sealed record ModulesInfo(Dictionary<string, Module> Modules);

public sealed record Module(Dictionary<string, ModuleVersionInfo> Versions);

public sealed record ModuleVersionInfo(Dictionary<string, BuildInfo> Platforms);