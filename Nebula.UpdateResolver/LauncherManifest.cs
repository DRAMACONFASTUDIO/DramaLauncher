using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.UpdateResolver;

public record struct LauncherManifest(
    [property: JsonPropertyName("entries")] HashSet<LauncherManifestEntry> Entries
);

public record struct LauncherManifestEntry(
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("path")] string Path
    );