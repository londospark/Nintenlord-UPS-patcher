namespace Nintenlord.Hacking.Core.Tests;

public class UPSfileLoadTests
{
    private static string WritePatchToTempFile(byte[] original, byte[] modified)
    {
        var patch = new UPSfile(original, modified);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");
        patch.WriteToFile(path);
        return path;
    }

    // ── UPSfile(string path) ─────────────────────────────────────────────────

    [Fact]
    public void StringConstructor_ValidFile_IsValidPatch()
    {
        var path = WritePatchToTempFile(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 9, 3, 8, 5 });
        try
        {
            using var loaded = new UPSfile(path);
            Assert.True(loaded.ValidPatch);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void StringConstructor_ValidFile_AppliesCorrectly()
    {
        var original = new byte[] { 10, 20, 30, 40, 50 };
        var modified = new byte[] { 10, 25, 30, 99, 50 };
        var path = WritePatchToTempFile(original, modified);
        try
        {
            using var loaded = new UPSfile(path);
            Assert.Equal(modified, loaded.Apply(original));
            Assert.Equal(original, loaded.Apply(modified));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void StringConstructor_NonExistentFile_InvalidPatch()
    {
        using var patch = new UPSfile(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups"));
        Assert.False(patch.ValidPatch);
    }

    [Fact]
    public void StringConstructor_TooShortFile_InvalidPatch()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");
        try
        {
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            using var patch = new UPSfile(path);
            Assert.False(patch.ValidPatch);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void StringConstructor_InvalidHeader_InvalidPatch()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");
        try
        {
            // 20 bytes with wrong magic ("NOPE" instead of "UPS1")
            var bad = new byte[20];
            bad[0] = (byte)'N'; bad[1] = (byte)'O'; bad[2] = (byte)'P'; bad[3] = (byte)'E';
            File.WriteAllBytes(path, bad);
            using var patch = new UPSfile(path);
            Assert.False(patch.ValidPatch);
        }
        finally { File.Delete(path); }
    }

    // ── UPSfile(byte[]) ───────────────────────────────────────────────────────

    [Fact]
    public void ByteArrayConstructor_ValidBytes_IsValidPatch()
    {
        var path = WritePatchToTempFile(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 9, 3, 8, 5 });
        try
        {
            using var loaded = new UPSfile(File.ReadAllBytes(path));
            Assert.True(loaded.ValidPatch);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ByteArrayConstructor_ValidBytes_AppliesCorrectly()
    {
        var original = new byte[] { 10, 20, 30, 40, 50 };
        var modified = new byte[] { 10, 25, 30, 99, 50 };
        var path = WritePatchToTempFile(original, modified);
        try
        {
            using var loaded = new UPSfile(File.ReadAllBytes(path));
            Assert.Equal(modified, loaded.Apply(original));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ByteArrayConstructor_TooShort_InvalidPatch()
    {
        using var patch = new UPSfile(new byte[] { 1, 2, 3 });
        Assert.False(patch.ValidPatch);
    }

    [Fact]
    public void ByteArrayConstructor_AllZeros_InvalidPatch()
    {
        // Valid length but wrong magic header
        using var patch = new UPSfile(new byte[20]);
        Assert.False(patch.ValidPatch);
    }

    [Fact]
    public void ByteArrayConstructor_MultipleRoundTrips_ConsistentBehavior()
    {
        var original = new byte[] { 5, 10, 15, 20, 25 };
        var modified = new byte[] { 5, 99, 15, 88, 25 };
        var path = WritePatchToTempFile(original, modified);
        try
        {
            var bytes = File.ReadAllBytes(path);
            using var a = new UPSfile(bytes);
            using var b = new UPSfile(bytes);
            Assert.Equal(a.Apply(original), b.Apply(original));
        }
        finally { File.Delete(path); }
    }

    // ── GetData() ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetData_TwoRuns_ReturnsCorrectCount()
    {
        // Two separate single-byte changes
        var original = new byte[] { 0, 0xAA, 0, 0xBB, 0 };
        var modified = new byte[] { 0, 0xFF, 0, 0xCC, 0 };
        var patch = new UPSfile(original, modified);

        var data = patch.GetData();

        Assert.Equal(2, data.GetLength(0));
    }

    [Fact]
    public void GetData_SingleRun_ReturnsCorrectOffsetAndLength()
    {
        // One contiguous run: bytes 1–2 changed
        var original = new byte[] { 0, 10, 20, 0, 0 };
        var modified = new byte[] { 0, 99, 77, 0, 0 };
        var patch = new UPSfile(original, modified);

        var data = patch.GetData();

        Assert.Equal(1, data.GetLength(0));
        Assert.Equal(1, data[0, 0]); // offset = 1
        Assert.Equal(2, data[0, 1]); // length = 2
    }

    [Fact]
    public void GetData_IdenticalFiles_ReturnsEmpty()
    {
        var file = new byte[] { 1, 2, 3, 4, 5 };
        var patch = new UPSfile(file, file);

        Assert.Equal(0, patch.GetData().GetLength(0));
    }

    [Fact]
    public void GetData_LoadedFromFile_MatchesInMemoryPatch()
    {
        var original = new byte[] { 0, 0xAA, 0, 0xBB, 0 };
        var modified = new byte[] { 0, 0xFF, 0, 0xCC, 0 };
        var inMemory = new UPSfile(original, modified);
        var path = WritePatchToTempFile(original, modified);
        try
        {
            using var fromFile = new UPSfile(path);
            var d1 = inMemory.GetData();
            var d2 = fromFile.GetData();
            Assert.Equal(d1.GetLength(0), d2.GetLength(0));
            for (var i = 0; i < d1.GetLength(0); i++)
            {
                Assert.Equal(d1[i, 0], d2[i, 0]);
                Assert.Equal(d1[i, 1], d2[i, 1]);
            }
        }
        finally { File.Delete(path); }
    }
}
