using System.Text;

namespace Nintenlord.Hacking.Core.Tests;

public class CRC32Tests
{
    [Fact]
    public void CalculateCRC32_KnownVector_MatchesStandardValue()
    {
        var data = Encoding.ASCII.GetBytes("123456789");

        var crc = CRC32.CalculateCRC32(data);

        Assert.Equal(0xCBF43926u, crc);
    }

    [Fact]
    public void CalculateCRC32_WithIndexAndLength_MatchesSlice()
    {
        var data = Encoding.ASCII.GetBytes("xxHELLOyy");

        var ranged = CRC32.CalculateCRC32(data, 2, 5);
        var sliced = CRC32.CalculateCRC32(Encoding.ASCII.GetBytes("HELLO"));

        Assert.Equal(sliced, ranged);
    }

    [Fact]
    public void CalculateCRC32_FromBinaryReader_MatchesByteArrayOverload()
    {
        var data = Encoding.ASCII.GetBytes("Nintenlord UPS");
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        var fromReader = CRC32.CalculateCRC32(reader);
        var fromArray = CRC32.CalculateCRC32(data);

        Assert.Equal(fromArray, fromReader);
    }

    [Fact]
    public void crc32_adjust_ByteByByte_EqualsBulkAdjustment()
    {
        var data = Encoding.ASCII.GetBytes("abcxyz");
        var oneByOne = 0xFFFFFFFF;
        var bulk = 0xFFFFFFFF;

        foreach (var b in data)
        {
            CRC32.crc32_adjust(ref oneByOne, b);
        }

        CRC32.crc32_adjust(ref bulk, data);

        Assert.Equal(~bulk, ~oneByOne);
    }
}

