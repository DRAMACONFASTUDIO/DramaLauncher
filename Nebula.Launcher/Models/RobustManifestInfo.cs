using System;

namespace Nebula.Launcher.Utils;

public record struct RobustManifestInfo(Uri ManifestUri, Uri DownloadUri, string Hash);