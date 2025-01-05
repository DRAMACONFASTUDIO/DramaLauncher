namespace Nebula.Shared.Models;

[Flags]
public enum DownloadStreamHeaderFlags
{
    None = 0,

    /// <summary>
    ///     If this flag is set on the download stream, individual files have been pre-compressed by the server.
    ///     This means each file has a compression header, and the launcher should not attempt to compress files itself.
    /// </summary>
    PreCompressed = 1 << 0
}