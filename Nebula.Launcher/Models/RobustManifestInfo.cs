using System;

namespace Nebula.Launcher.Models;

public record struct RobustManifestInfo(Uri ManifestUri, Uri DownloadUri, string Hash);