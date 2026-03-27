namespace Nintenlord.Hacking.Core.Tests;

public class CompressionTests
{
    [Fact]
    public void Constructor_NoneMode_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(
            () => new TestCompression(CompressionOperation.None, Array.Empty<string>()));
    }

    [Fact]
    public void Constructor_ValidMode_SetsSupportedModes()
    {
        var c = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());

        Assert.Equal(CompressionOperation.Compress, c.supportedModes);
    }

    [Fact]
    public void Constructor_ValidMode_SetsFileExtensions()
    {
        var extensions = new[] { ".bin", ".lz" };
        var c = new TestCompression(CompressionOperation.Decompress, extensions);

        Assert.Equal(extensions, c.fileExtensions);
    }

    [Fact]
    public void SupportsOperation_IncludedFlag_ReturnsTrue()
    {
        var c = new TestCompression(
            CompressionOperation.Compress | CompressionOperation.Decompress,
            Array.Empty<string>());

        Assert.True(c.SupportsOperation(CompressionOperation.Compress));
        Assert.True(c.SupportsOperation(CompressionOperation.Decompress));
    }

    [Fact]
    public void SupportsOperation_ExcludedFlag_ReturnsFalse()
    {
        var c = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());

        Assert.False(c.SupportsOperation(CompressionOperation.Decompress));
    }

    [Fact]
    public void SupportsOperation_CombinedFlags_AllMustBePresent()
    {
        var c = new TestCompression(
            CompressionOperation.Compress | CompressionOperation.Decompress,
            Array.Empty<string>());

        Assert.True(c.SupportsOperation(CompressionOperation.Compress | CompressionOperation.Decompress));
        Assert.False(c.SupportsOperation(CompressionOperation.Compress | CompressionOperation.Scan));
    }

    [Fact]
    public void Equals_SameType_ReturnsTrue()
    {
        var a = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());
        var b = new TestCompression(CompressionOperation.Decompress, new[] { ".bin" });

        Assert.True(a.Equals(b));
        Assert.True(b.Equals(a));
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var a = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());
        var b = new AnotherCompression(CompressionOperation.Compress, Array.Empty<string>());

        Assert.False(a.Equals(b));
        Assert.False(b.Equals(a));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var c = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());

        Assert.False(c.Equals(null));
    }

    [Fact]
    public void GetHashCode_SameType_ReturnsSameValue()
    {
        var a = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());
        var b = new TestCompression(CompressionOperation.Decompress, Array.Empty<string>());

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentType_ReturnsDifferentValue()
    {
        var a = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());
        var b = new AnotherCompression(CompressionOperation.Compress, Array.Empty<string>());

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsTypeName()
    {
        var c = new TestCompression(CompressionOperation.Compress, Array.Empty<string>());

        Assert.Equal("TestCompression", c.ToString());
    }

    private class TestCompression(CompressionOperation modes, string[] extensions)
        : Compression(modes, extensions)
    {
        public override byte[] Decompress(byte[] data) => throw new NotImplementedException();
        public override byte[] Decompress(byte[] data, int offset) => throw new NotImplementedException();
        public override byte[] Compress(byte[] data) => throw new NotImplementedException();
        public override byte[] Compress(byte[] data, int offset) => throw new NotImplementedException();
        public override byte[] Compress(byte[] data, int offset, int length) => throw new NotImplementedException();
        public override int[] Scan(byte[] data, int sizeMax, int sizeMin, int sizeModulus) => throw new NotImplementedException();
        public override int[] Scan(byte[] data, int offset, int sizeMax, int sizeMin, int sizeModulus) => throw new NotImplementedException();
        public override int[] Scan(byte[] data, int offset, int length, int sizeMax, int sizeMin, int sizeModulus) => throw new NotImplementedException();
        public override bool CanBeDecompressed(byte[] data) => throw new NotImplementedException();
        public override bool CanBeDecompressed(byte[] data, int offset) => throw new NotImplementedException();
        public override bool CanBeDecompressed(byte[] data, int offset, int maxLength, int minLength) => throw new NotImplementedException();
        public override int CompressedLength(byte[] data) => throw new NotImplementedException();
        public override int CompressedLength(byte[] data, int offset) => throw new NotImplementedException();
        public override int DecompressedDataLenght(byte[] data) => throw new NotImplementedException();
        public override int DecompressedDataLenght(byte[] data, int offset) => throw new NotImplementedException();
    }

    private class AnotherCompression(CompressionOperation modes, string[] extensions)
        : Compression(modes, extensions)
    {
        public override byte[] Decompress(byte[] data) => throw new NotImplementedException();
        public override byte[] Decompress(byte[] data, int offset) => throw new NotImplementedException();
        public override byte[] Compress(byte[] data) => throw new NotImplementedException();
        public override byte[] Compress(byte[] data, int offset) => throw new NotImplementedException();
        public override byte[] Compress(byte[] data, int offset, int length) => throw new NotImplementedException();
        public override int[] Scan(byte[] data, int sizeMax, int sizeMin, int sizeModulus) => throw new NotImplementedException();
        public override int[] Scan(byte[] data, int offset, int sizeMax, int sizeMin, int sizeModulus) => throw new NotImplementedException();
        public override int[] Scan(byte[] data, int offset, int length, int sizeMax, int sizeMin, int sizeModulus) => throw new NotImplementedException();
        public override bool CanBeDecompressed(byte[] data) => throw new NotImplementedException();
        public override bool CanBeDecompressed(byte[] data, int offset) => throw new NotImplementedException();
        public override bool CanBeDecompressed(byte[] data, int offset, int maxLength, int minLength) => throw new NotImplementedException();
        public override int CompressedLength(byte[] data) => throw new NotImplementedException();
        public override int CompressedLength(byte[] data, int offset) => throw new NotImplementedException();
        public override int DecompressedDataLenght(byte[] data) => throw new NotImplementedException();
        public override int DecompressedDataLenght(byte[] data, int offset) => throw new NotImplementedException();
    }
}
