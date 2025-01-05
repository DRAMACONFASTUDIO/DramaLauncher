namespace Nebula.Shared.Models;

public record struct RobustManifestInfo(Uri ManifestUri, Uri DownloadUri, string Hash);