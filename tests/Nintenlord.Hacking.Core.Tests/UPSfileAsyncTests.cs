namespace Nintenlord.Hacking.Core.Tests;

public class UPSfileAsyncTests
{
    private static string TempPath(string ext = "") =>
        Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);

    private static string WriteTemp(byte[] data, string ext = "")
    {
        var path = TempPath(ext);
        File.WriteAllBytes(path, data);
        return path;
    }

    private static string WritePatchFile(byte[] original, byte[] modified)
    {
        var patch = new UPSfile(original, modified);
        var path = TempPath(".ups");
        patch.WriteToFile(path);
        return path;
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    // ── ValidToApplyAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ValidToApplyAsync_OriginalFile_ReturnsTrue()
    {
        var original = new byte[] { 1, 2, 3, 4, 5 };
        var modified = new byte[] { 1, 9, 3, 8, 5 };
        var patchPath = WritePatchFile(original, modified);
        var romPath = WriteTemp(original);
        try
        {
            using var patch = new UPSfile(patchPath);
            Assert.True(await patch.ValidToApplyAsync(romPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(romPath); }
    }

    [Fact]
    public async Task ValidToApplyAsync_ModifiedFile_ReturnsTrue()
    {
        var original = new byte[] { 1, 2, 3, 4, 5 };
        var modified = new byte[] { 1, 9, 3, 8, 5 };
        var patchPath = WritePatchFile(original, modified);
        var romPath = WriteTemp(modified);
        try
        {
            using var patch = new UPSfile(patchPath);
            Assert.True(await patch.ValidToApplyAsync(romPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(romPath); }
    }

    [Fact]
    public async Task ValidToApplyAsync_WrongCrc_ReturnsFalse()
    {
        var original = new byte[] { 1, 2, 3, 4, 5 };
        var modified = new byte[] { 1, 9, 3, 8, 5 };
        var patchPath = WritePatchFile(original, modified);
        // Same size as original but different bytes → wrong CRC
        var romPath = WriteTemp(new byte[] { 9, 9, 9, 9, 9 });
        try
        {
            using var patch = new UPSfile(patchPath);
            Assert.False(await patch.ValidToApplyAsync(romPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(romPath); }
    }

    [Fact]
    public async Task ValidToApplyAsync_WrongSize_ReturnsFalse()
    {
        var original = new byte[] { 1, 2, 3, 4, 5 };
        var modified = new byte[] { 1, 9, 3, 8, 5 };
        var patchPath = WritePatchFile(original, modified);
        var romPath = WriteTemp(new byte[] { 1, 2, 3 }); // wrong length
        try
        {
            using var patch = new UPSfile(patchPath);
            Assert.False(await patch.ValidToApplyAsync(romPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(romPath); }
    }

    [Fact]
    public async Task ValidToApplyAsync_NonExistentFile_ReturnsFalse()
    {
        var patchPath = WritePatchFile(new byte[] { 1, 2, 3 }, new byte[] { 1, 9, 3 });
        try
        {
            using var patch = new UPSfile(patchPath);
            Assert.False(await patch.ValidToApplyAsync(TempPath()));
        }
        finally { DeleteIfExists(patchPath); }
    }

    [Fact]
    public async Task ValidToApplyAsync_ReportsProgress()
    {
        var original = new byte[4096];
        var modified = new byte[4096];
        modified[100] = 0xFF;
        var patchPath = WritePatchFile(original, modified);
        var romPath = WriteTemp(original);
        var reports = new List<double>();
        try
        {
            using var patch = new UPSfile(patchPath);
            await patch.ValidToApplyAsync(romPath, CancellationToken.None,
                new Progress<double>(p => reports.Add(p)));
            Assert.NotEmpty(reports);
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(romPath); }
    }

    // ── ApplyAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyAsync_InPlace_ProducesCorrectOutput()
    {
        var original = new byte[] { 10, 20, 30, 40, 50 };
        var modified = new byte[] { 10, 25, 30, 99, 50 };
        var patchPath = WritePatchFile(original, modified);
        var romPath = WriteTemp(original);
        try
        {
            using var patch = new UPSfile(patchPath);
            await patch.ApplyAsync(romPath, romPath);
            Assert.Equal(modified, File.ReadAllBytes(romPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(romPath); }
    }

    [Fact]
    public async Task ApplyAsync_ToSeparateFile_OriginalUntouched()
    {
        var original = new byte[] { 10, 20, 30, 40, 50 };
        var modified = new byte[] { 10, 25, 30, 99, 50 };
        var patchPath = WritePatchFile(original, modified);
        var inputPath = WriteTemp(original);
        var outputPath = TempPath();
        try
        {
            using var patch = new UPSfile(patchPath);
            await patch.ApplyAsync(inputPath, outputPath);
            Assert.Equal(original, File.ReadAllBytes(inputPath));
            Assert.Equal(modified, File.ReadAllBytes(outputPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(inputPath); DeleteIfExists(outputPath); }
    }

    [Fact]
    public async Task ApplyAsync_ReportsProgressAndFinishesWith100()
    {
        var original = new byte[4096];
        var modified = new byte[4096];
        modified[100] = 0xFF;
        var patchPath = WritePatchFile(original, modified);
        var inputPath = WriteTemp(original);
        var outputPath = TempPath();
        var reports = new List<double>();
        try
        {
            using var patch = new UPSfile(patchPath);
            await patch.ApplyAsync(inputPath, outputPath,
                new Progress<double>(p => reports.Add(p)));
            Assert.NotEmpty(reports);
            Assert.Equal(100.0, reports.Last());
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(inputPath); DeleteIfExists(outputPath); }
    }

    [Fact]
    public async Task ApplyAsync_Cancelled_ThrowsAndLeavesNoTempFile()
    {
        var original = new byte[4096];
        var modified = new byte[4096];
        modified[100] = 0xFF;
        var patchPath = WritePatchFile(original, modified);
        var inputPath = WriteTemp(original);
        var outputPath = TempPath();
        try
        {
            using var patch = new UPSfile(patchPath);
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => patch.ApplyAsync(inputPath, outputPath, null, cts.Token));
            Assert.False(File.Exists(outputPath + ".patching.tmp"));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(inputPath); DeleteIfExists(outputPath); }
    }

    [Fact]
    public async Task ApplyAsync_ByteArrayLoadedPatch_ProducesCorrectOutput()
    {
        var original = new byte[] { 0, 10, 20, 30, 40 };
        var modified = new byte[] { 0, 99, 20, 77, 40 };
        var patchPath = WritePatchFile(original, modified);
        var inputPath = WriteTemp(original);
        var outputPath = TempPath();
        try
        {
            using var patch = new UPSfile(File.ReadAllBytes(patchPath));
            await patch.ApplyAsync(inputPath, outputPath);
            Assert.Equal(modified, File.ReadAllBytes(outputPath));
        }
        finally { DeleteIfExists(patchPath); DeleteIfExists(inputPath); DeleteIfExists(outputPath); }
    }

    // ── WriteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task WriteAsync_ProducesValidAndCorrectPatch()
    {
        var original = new byte[] { 10, 20, 30, 40, 50 };
        var modified = new byte[] { 10, 25, 30, 99, 50 };
        var originalPath = WriteTemp(original);
        var modifiedPath = WriteTemp(modified);
        var patchPath = TempPath(".ups");
        try
        {
            await UPSfile.WriteAsync(originalPath, modifiedPath, patchPath);
            using var loaded = new UPSfile(patchPath);
            Assert.True(loaded.ValidPatch);
            Assert.Equal(modified, loaded.Apply(original));
        }
        finally { DeleteIfExists(originalPath); DeleteIfExists(modifiedPath); DeleteIfExists(patchPath); }
    }

    [Fact]
    public async Task WriteAsync_PatchIsRoundTrippable()
    {
        var original = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        var modified = new byte[] { 0, 9, 2, 3, 4, 5, 8, 7 };
        var originalPath = WriteTemp(original);
        var modifiedPath = WriteTemp(modified);
        var patchPath = TempPath(".ups");
        var outputPath = TempPath();
        try
        {
            await UPSfile.WriteAsync(originalPath, modifiedPath, patchPath);
            using var patch = new UPSfile(patchPath);
            await patch.ApplyAsync(originalPath, outputPath);
            Assert.Equal(modified, File.ReadAllBytes(outputPath));
        }
        finally
        {
            DeleteIfExists(originalPath); DeleteIfExists(modifiedPath);
            DeleteIfExists(patchPath); DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public async Task WriteAsync_ReportsProgress()
    {
        var original = new byte[4096];
        var modified = new byte[4096];
        modified[500] = 0xAB;
        var originalPath = WriteTemp(original);
        var modifiedPath = WriteTemp(modified);
        var patchPath = TempPath(".ups");
        var reports = new List<double>();
        try
        {
            await UPSfile.WriteAsync(originalPath, modifiedPath, patchPath,
                new Progress<double>(p => reports.Add(p)));
            Assert.NotEmpty(reports);
        }
        finally { DeleteIfExists(originalPath); DeleteIfExists(modifiedPath); DeleteIfExists(patchPath); }
    }

    [Fact]
    public async Task WriteAsync_Cancelled_LeavesNoOutputFile()
    {
        var originalPath = WriteTemp(new byte[4096]);
        var modifiedPath = WriteTemp(new byte[4096]);
        var patchPath = TempPath(".ups");
        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => UPSfile.WriteAsync(originalPath, modifiedPath, patchPath, null, cts.Token));
            Assert.False(File.Exists(patchPath));
        }
        finally { DeleteIfExists(originalPath); DeleteIfExists(modifiedPath); DeleteIfExists(patchPath); }
    }

    [Fact]
    public async Task WriteAsync_MatchesInMemoryPatch()
    {
        var original = new byte[] { 0, 0xAA, 0, 0xBB, 0, 0xCC };
        var modified = new byte[] { 0, 0xFF, 0, 0xDD, 0, 0xCC };
        var originalPath = WriteTemp(original);
        var modifiedPath = WriteTemp(modified);
        var patchPath = TempPath(".ups");
        try
        {
            await UPSfile.WriteAsync(originalPath, modifiedPath, patchPath);
            var inMemory = new UPSfile(original, modified);
            using var fromFile = new UPSfile(patchPath);
            Assert.Equal(inMemory.Apply(original), fromFile.Apply(original));
        }
        finally { DeleteIfExists(originalPath); DeleteIfExists(modifiedPath); DeleteIfExists(patchPath); }
    }
}
