using DSDecmp.Formats.Nitro;
using FsCheck.Xunit;

namespace Nintenlord.Hacking.Core.Tests;

public class LZ11PropertyTests
{
    /// Compressing any non-empty byte sequence and then decompressing it returns
    /// the original data unchanged.
    [Property(MaxTest = 100)]
    public bool CompressDecompress_RoundTrip(byte[] data)
    {
        if (data.Length == 0) return true;

        using var inStream = new MemoryStream(data);
        using var compressed = new MemoryStream();
        LZ11.CompressWithLA(inStream, data.Length, compressed);

        compressed.Seek(0, SeekOrigin.Begin);
        using var decompressed = new MemoryStream();
        var decompSize = LZ11.Decompress(compressed, decompressed);

        return (long)data.Length == decompSize
               && data.SequenceEqual(decompressed.ToArray());
    }

    /// The decompressed size reported by Decompress always matches the actual
    /// number of bytes written to the output stream.
    [Property(MaxTest = 100)]
    public bool Decompress_ReportedSize_MatchesOutputLength(byte[] data)
    {
        if (data.Length == 0) return true;

        using var inStream = new MemoryStream(data);
        using var compressed = new MemoryStream();
        LZ11.CompressWithLA(inStream, data.Length, compressed);

        compressed.Seek(0, SeekOrigin.Begin);
        using var decompressed = new MemoryStream();
        var reported = LZ11.Decompress(compressed, decompressed);

        return reported == decompressed.Length;
    }

    /// Compressing always produces a non-empty output (the format header alone
    /// takes at least 4 bytes).
    [Property(MaxTest = 100)]
    public bool Compress_OutputIsNonEmpty(byte[] data)
    {
        if (data.Length == 0) return true;

        using var inStream = new MemoryStream(data);
        using var compressed = new MemoryStream();
        LZ11.CompressWithLA(inStream, data.Length, compressed);

        return compressed.Length > 0;
    }
}
