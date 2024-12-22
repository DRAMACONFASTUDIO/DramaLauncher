using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.Launcher.Models;

public sealed record AuthInfo(string Mode, string PublicKey);

public sealed record BuildInfo(
    string EngineVersion,
    string ForkId,
    string Version,
    string DownloadUrl,
    string ManifestUrl,
    bool Acz,
    string Hash,
    string ManifestHash);

public sealed record ServerLink(string Name, string Icon, string Url);
public sealed record ServerInfo(string ConnectAddress, AuthInfo Auth, BuildInfo Build, string Desc, List<ServerLink> Links);

public sealed record EngineVersionInfo(
    bool Insecure,
    [property: JsonPropertyName("redirect")]
    string? RedirectVersion,
    Dictionary<string, EngineBuildInfo> Platforms);

public sealed class EngineBuildInfo
{
    [JsonInclude] [JsonPropertyName("sha256")]
    public string Sha256 = default!;

    [JsonInclude] [JsonPropertyName("sig")]
    public string Signature = default!;

    [JsonInclude] [JsonPropertyName("url")]
    public string Url = default!;
}

public sealed record ServerHubInfo(string Address, ServerStatus StatusData, List<string> InferredTags);

public sealed record ServerStatus(
    string Map,
    string Name,
    List<string> Tags,
    string Preset,
    int Players,
    int RoundId,
    int RunLevel, 
    bool PanicBunker, 
    DateTime? RoundStartTime, 
    int SoftMaxPlayers);

public sealed record ModulesInfo(Dictionary<string, Module> Modules);

public sealed record Module(Dictionary<string, ModuleVersionInfo> Versions);

public sealed record ModuleVersionInfo(Dictionary<string, EngineBuildInfo> Platforms);