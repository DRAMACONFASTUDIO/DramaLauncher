using System.Collections.Generic;

namespace Nebula.UpdateResolver;

public record struct ManifestEnsureInfo(HashSet<LauncherManifestEntry> ToDownload, HashSet<LauncherManifestEntry> ToDelete, HashSet<LauncherManifestEntry> FilesExist);