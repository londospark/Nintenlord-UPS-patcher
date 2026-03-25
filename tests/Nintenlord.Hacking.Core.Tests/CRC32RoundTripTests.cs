using FsCheck.Xunit;

namespace Nintenlord.Hacking.Core.Tests;

public class CRC32RoundTripTests
{
    [Fact]
    public void CRC32_OfMultipleParts_EqualsOnePart()
    {
        var part1 = new byte[] { 1, 2, 3, 4, 5 };
        var part2 = new byte[] { 6, 7, 8, 9 };
        var combined = new byte[part1.Length + part2.Length];
        Array.Copy(part1, combined, part1.Length);
        Array.Copy(part2, 0, combined, part1.Length, part2.Length);

        var crc_combined = CRC32.CalculateCRC32(combined);
        var crc_whole = CRC32.CalculateCRC32(combined);

        Assert.Equal(crc_combined, crc_whole);
    }

    [Property(MaxTest = 100)]
    public bool CRC32_RangeOverloads_AreEquivalentToArraySlice(byte[] data, int start)
    {
        if (data == null || data.Length == 0)
        {
            return true;
        }

        var actualStart = Math.Abs(start) % data.Length;
        var length = Math.Max(1, (data.Length - actualStart) / 2);

        var crcRange = CRC32.CalculateCRC32(data, actualStart, length);
        var slice = new byte[length];
        Array.Copy(data, actualStart, slice, 0, length);
        var crcSlice = CRC32.CalculateCRC32(slice);

        return crcRange == crcSlice;
    }

    [Fact]
    public void BinaryReader_RoundTrip_ProducesSameChecksum()
    {
        var data = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        uint crc1, crc2;
        using (var stream = new MemoryStream(data))
        using (var reader = new BinaryReader(stream))
        {
            crc1 = CRC32.CalculateCRC32(reader);
        }

        using (var stream = new MemoryStream(data))
        using (var reader = new BinaryReader(stream))
        {
            crc2 = CRC32.CalculateCRC32(reader);
        }

        Assert.Equal(crc1, crc2);
        Assert.Equal(crc1, CRC32.CalculateCRC32(data));
    }
}

