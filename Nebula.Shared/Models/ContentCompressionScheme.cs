namespace Nebula.Shared.Models;

public enum ContentCompressionScheme
{
    None = 0,
    Deflate = 1,

    /// <summary>
    ///     ZStandard compression. In the future may use SS14 specific dictionary IDs in the frame header.
    /// </summary>
    ZStd = 2
}