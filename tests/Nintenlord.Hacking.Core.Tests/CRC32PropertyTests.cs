using FsCheck.Xunit;

namespace Nintenlord.Hacking.Core.Tests;

public class CRC32PropertyTests
{
    [Property(MaxTest = 140)]
    public bool CalculateCRC32_RangeOverloadMatchesWholeArray(byte[] data)
    {
        var whole = CRC32.CalculateCRC32(data);
        var ranged = CRC32.CalculateCRC32(data, 0, data.Length);

        return whole == ranged;
    }

    [Property(MaxTest = 120)]
    public bool CalculateCRC32_BinaryReaderOverloadMatchesArray(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var fromReader = CRC32.CalculateCRC32(reader);
        var fromArray = CRC32.CalculateCRC32(data);

        return fromReader == fromArray;
    }

    [Property(MaxTest = 120)]
    public bool crc32_adjust_IncrementalAndBulkProduceSameChecksum(byte[] data)
    {
        var incremental = 0xFFFFFFFF;
        var bulk = 0xFFFFFFFF;

        foreach (var b in data)
        {
            CRC32.crc32_adjust(ref incremental, b);
        }

        CRC32.crc32_adjust(ref bulk, data);
        return ~incremental == ~bulk;
    }
}



