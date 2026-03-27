using Nintenlord.Hacking.Core.GameData;
using Nintenlord.Hacking.Core.MemoryManagement;

namespace Nintenlord.Hacking.Core.Tests;

public class ROMReaderStreamTests
{
    private static readonly byte[] SampleData = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];

    [Fact]
    public void CanRead_IsTrue()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        Assert.True(stream.CanRead);
    }

    [Fact]
    public void CanSeek_IsTrue()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        Assert.True(stream.CanSeek);
    }

    [Fact]
    public void CanWrite_IsFalse()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        Assert.False(stream.CanWrite);
    }

    [Fact]
    public void Length_DelegatesToRomLength()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        Assert.Equal(SampleData.Length, stream.Length);
    }

    [Fact]
    public void Read_ReturnsCorrectData()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));
        var buffer = new byte[4];

        stream.Read(buffer, 0, 4);

        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, buffer);
    }

    [Fact]
    public void Read_WithBufferOffset_CopiesIntoCorrectPosition()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));
        var buffer = new byte[8];

        int read = stream.Read(buffer, 2, 4);

        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, buffer[2..6]);
        Assert.Equal(4, read);
    }

    [Fact]
    public void Read_ReturnsDataLength()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));
        var buffer = new byte[4];

        int result = stream.Read(buffer, 0, 4);

        Assert.Equal(4, result);
    }

    [Fact]
    public void Seek_FromBegin_SetsPosition()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        long pos = stream.Seek(3, SeekOrigin.Begin);

        Assert.Equal(3, pos);
        Assert.Equal(3, stream.Position);
    }

    [Fact]
    public void Seek_FromCurrent_AdvancesPosition()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));
        stream.Seek(2, SeekOrigin.Begin);

        long pos = stream.Seek(3, SeekOrigin.Current);

        Assert.Equal(5, pos);
        Assert.Equal(5, stream.Position);
    }

    [Fact]
    public void Seek_FromEnd_SetsPositionRelativeToEnd()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        long pos = stream.Seek(-2, SeekOrigin.End);

        Assert.Equal(SampleData.Length - 2, pos);
        Assert.Equal(SampleData.Length - 2, stream.Position);
    }

    [Fact]
    public void SetLength_ThrowsNotSupportedException()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        Assert.Throws<NotSupportedException>(() => stream.SetLength(100));
    }

    [Fact]
    public void Write_ThrowsNotSupportedException()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        Assert.Throws<NotSupportedException>(() => stream.Write([0x00], 0, 1));
    }

    [Fact]
    public void Position_SetDirectly_UpdatesPosition()
    {
        using var stream = new ROMReaderStream(new FakeROM(SampleData));

        stream.Position = 5;

        Assert.Equal(5, stream.Position);
    }

    private sealed class FakeROM(byte[] data) : IROM
    {
        public string GameTitle => "Test";
        public string GameCode => "TEST";
        public string MakerCode => "TT";
        public int Length => data.Length;

        public byte[] ReadData(int offset, int length)
        {
            int available = Math.Max(0, Math.Min(length, data.Length - offset));
            var result = new byte[available];
            Array.Copy(data, offset, result, 0, available);
            return result;
        }

        public byte[] ReadData(ManagedPointer ptr) => throw new NotImplementedException();
        public void WriteData(ManagedPointer ptr, byte[] d, int index, int length) => throw new NotImplementedException();
        public void AddCustomData(string name, string value) => throw new NotImplementedException();
        public string GetCustomData(string name) => throw new NotImplementedException();
    }
}
