using DSDecmp.Formats.Nitro;

namespace Nintenlord.Hacking.Core.Tests;

public class LZ11Tests
{
    [Fact]
    public void RoundTrip_SmallData_DecompressesIdentically()
    {
        var original = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE };

        using var instream = new MemoryStream(original);
        using var compressed = new MemoryStream();
        LZ11.CompressWithLA(instream, original.Length, compressed);

        compressed.Seek(0, SeekOrigin.Begin);
        using var decompressed = new MemoryStream();
        long decompSize = LZ11.Decompress(compressed, decompressed);

        Assert.Equal(original.Length, decompSize);
        Assert.Equal(original, decompressed.ToArray());
    }

    [Fact]
    public void RoundTrip_RepetitiveData_DecompressesIdentically()
    {
        var original = new byte[64];
        for (int i = 0; i < original.Length; i++)
            original[i] = (byte)(i % 8);

        using var instream = new MemoryStream(original);
        using var compressed = new MemoryStream();
        LZ11.CompressWithLA(instream, original.Length, compressed);

        compressed.Seek(0, SeekOrigin.Begin);
        using var decompressed = new MemoryStream();
        long decompSize = LZ11.Decompress(compressed, decompressed);

        Assert.Equal(original.Length, decompSize);
        Assert.Equal(original, decompressed.ToArray());
    }

    [Fact]
    public void RoundTrip_LargerData_DecompressesIdentically()
    {
        var original = new byte[256];
        for (int i = 0; i < original.Length; i++)
            original[i] = (byte)i;

        using var instream = new MemoryStream(original);
        using var compressed = new MemoryStream();
        LZ11.CompressWithLA(instream, original.Length, compressed);

        compressed.Seek(0, SeekOrigin.Begin);
        using var decompressed = new MemoryStream();
        long decompSize = LZ11.Decompress(compressed, decompressed);

        Assert.Equal(original.Length, decompSize);
        Assert.Equal(original, decompressed.ToArray());
    }

    [Fact]
    public void CompressWithLA_ReturnsPositiveCompressedSize()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        using var instream = new MemoryStream(data);
        using var outstream = new MemoryStream();

        int size = LZ11.CompressWithLA(instream, data.Length, outstream);

        Assert.True(size > 0);
    }

    [Fact]
    public void Decompress_InvalidTypeByte_ThrowsInvalidDataException()
    {
        // First byte is 0xFF, not 0x11
        var badData = new byte[] { 0xFF, 0x05, 0x00, 0x00 };
        using var instream = new MemoryStream(badData);
        using var outstream = new MemoryStream();

        Assert.Throws<InvalidDataException>(() => LZ11.Decompress(instream, outstream));
    }

    [Fact]
    public void Decompress_TypeByteIsZero_ThrowsInvalidDataException()
    {
        var badData = new byte[] { 0x00, 0x05, 0x00, 0x00 };
        using var instream = new MemoryStream(badData);
        using var outstream = new MemoryStream();

        Assert.Throws<InvalidDataException>(() => LZ11.Decompress(instream, outstream));
    }
}
