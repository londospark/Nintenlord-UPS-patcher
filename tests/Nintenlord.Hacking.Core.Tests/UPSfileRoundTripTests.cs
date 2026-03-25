namespace Nintenlord.Hacking.Core.Tests;

public class UPSfileRoundTripTests
{
    [Fact]
    public void WriteToFile_ThenRead_ProducesValidPatchWithSameBehavior()
    {
        var original = new byte[] { 10, 20, 30, 40, 50 };
        var modified = new byte[] { 10, 25, 30, 99, 50 };
        var generated = new UPSfile(original, modified);

        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");
        try
        {
            generated.WriteToFile(tempPath);
            var loaded = new UPSfile(tempPath);

            Assert.True(loaded.ValidPatch);
            Assert.Equal(modified, loaded.Apply(original));
            Assert.Equal(original, loaded.Apply(modified));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void WriteToFile_MultipleRoundTrips_AreConsistent()
    {
        var a = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var b = new byte[] { 1, 9, 3, 4, 5, 8, 7, 8 };
        var patch1 = new UPSfile(a, b);

        var path1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");
        var path2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");

        try
        {
            patch1.WriteToFile(path1);
            var loaded1 = new UPSfile(path1);
            loaded1.WriteToFile(path2);
            var loaded2 = new UPSfile(path2);

            Assert.Equal(patch1.Apply(a), loaded1.Apply(a));
            Assert.Equal(loaded1.Apply(a), loaded2.Apply(a));
        }
        finally
        {
            if (File.Exists(path1))
                File.Delete(path1);
            if (File.Exists(path2))
                File.Delete(path2);
        }
    }

    [Fact]
    public void ApplyTwice_ReturnsOriginalForEqualLengthFiles()
    {
        var file1 = new byte[] { 100, 101, 102, 103 };
        var file2 = new byte[] { 100, 200, 102, 104 };
        var patch = new UPSfile(file1, file2);

        var applied = patch.Apply(file1);
        var reapplied = patch.Apply(applied);

        Assert.Equal(file2, applied);
        Assert.Equal(file1, reapplied);
    }

    [Fact]
    public void Clone_ProducesIndependentCopyWithIdenticalBehavior()
    {
        var src = new byte[] { 1, 2, 3, 4, 5 };
        var tgt = new byte[] { 1, 9, 3, 8, 5 };
        var original = new UPSfile(src, tgt);
        var cloned = (UPSfile)original.Clone();

        var tempPath1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");
        var tempPath2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ups");

        try
        {
            original.WriteToFile(tempPath1);
            cloned.WriteToFile(tempPath2);

            var data1 = File.ReadAllBytes(tempPath1);
            var data2 = File.ReadAllBytes(tempPath2);

            Assert.Equal(data1, data2);
            Assert.Equal(original.Apply(src), cloned.Apply(src));
        }
        finally
        {
            if (File.Exists(tempPath1))
                File.Delete(tempPath1);
            if (File.Exists(tempPath2))
                File.Delete(tempPath2);
        }
    }

    [Fact]
    public void PatchComposition_ProducesSameResultAsTwoSequentialPatches()
    {
        var base_file = new byte[] { 10, 20, 30, 40, 50 };
        var step1 = new byte[] { 10, 25, 30, 40, 50 };
        var step2 = new byte[] { 10, 25, 35, 40, 50 };

        var patch1 = new UPSfile(base_file, step1);
        var patch2 = new UPSfile(step1, step2);
        var combined = patch1 + patch2;

        var sequential = patch2.Apply(patch1.Apply(base_file));
        var direct = combined.Apply(base_file);

        Assert.Equal(step2, sequential);
        Assert.Equal(step2, direct);
    }
}

