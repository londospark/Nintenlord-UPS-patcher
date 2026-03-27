namespace Nintenlord.Hacking.Core.Tests;

public class Crc32AsyncTests
{
    // Standard CRC32 test vector: "123456789" → 0xCBF43926
    private static readonly byte[] KnownInput = "123456789"u8.ToArray();
    private const uint KnownCrc = 0xCBF43926;

    // ── CalculateCRC32Async ───────────────────────────────────────────────────

    [Fact]
    public async Task CalculateCRC32Async_KnownVector_ReturnsExpected()
    {
        using var stream = new MemoryStream(KnownInput);
        Assert.Equal(KnownCrc, await CRC32.CalculateCRC32Async(stream));
    }

    [Fact]
    public async Task CalculateCRC32Async_SeekableStream_MatchesSyncOverload()
    {
        var data = new byte[8192];
        new Random(42).NextBytes(data);
        using var stream = new MemoryStream(data);
        Assert.Equal(CRC32.CalculateCRC32(data), await CRC32.CalculateCRC32Async(stream));
    }

    [Fact]
    public async Task CalculateCRC32Async_NonSeekableStream_ReturnsCorrectResult()
    {
        using var ms = new MemoryStream(KnownInput);
        await using var ns = new NonSeekableStream(ms);
        Assert.Equal(KnownCrc, await CRC32.CalculateCRC32Async(ns));
    }

    [Fact]
    public async Task CalculateCRC32Async_ReportsProgressOnSeekableStream()
    {
        // Use a buffer larger than the chunk size (81920) to generate multiple progress reports.
        // SyncProgress avoids the thread-pool race inherent in Progress<T>.
        var data = new byte[200_000];
        new Random(1).NextBytes(data);
        var reports = new List<double>();
        using var stream = new MemoryStream(data);
        await CRC32.CalculateCRC32Async(stream, CancellationToken.None,
            new SyncProgress<double>(reports.Add));
        Assert.NotEmpty(reports);
        Assert.All(reports, p => Assert.InRange(p, 0.0, 100.0));
    }

    [Fact]
    public async Task CalculateCRC32Async_NonSeekableStream_DoesNotReportProgress()
    {
        // totalBytes = 0 for non-seekable streams — no progress callbacks expected
        using var ms = new MemoryStream(KnownInput);
        await using var ns = new NonSeekableStream(ms);
        var reports = new List<double>();
        await CRC32.CalculateCRC32Async(ns, CancellationToken.None,
            new Progress<double>(reports.Add));
        Assert.Empty(reports);
    }

    [Fact]
    public async Task CalculateCRC32Async_Cancelled_ThrowsOperationCanceledException()
    {
        using var stream = new MemoryStream(new byte[8192]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CRC32.CalculateCRC32Async(stream, cts.Token));
    }

    [Fact]
    public async Task CalculateCRC32Async_EmptyStream_ReturnsZero()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());
        // CRC32 of empty input is 0x00000000
        Assert.Equal(0u, await CRC32.CalculateCRC32Async(stream));
    }

    // ── CalculateCRC32(ReadOnlySpan<byte>) ────────────────────────────────────

    [Fact]
    public void CalculateCRC32_ReadOnlySpan_KnownVector() => Assert.Equal(KnownCrc, CRC32.CalculateCRC32((ReadOnlySpan<byte>)KnownInput));

    [Fact]
    public void CalculateCRC32_ReadOnlySpan_MatchesArrayOverload()
    {
        var data = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A };
        Assert.Equal(CRC32.CalculateCRC32(data), CRC32.CalculateCRC32((ReadOnlySpan<byte>)data));
    }

    [Fact]
    public void CalculateCRC32_ReadOnlySpan_SliceMatchesRangeOverload()
    {
        var data = new byte[] { 0, 0x12, 0x34, 0x56, 0 };
        Assert.Equal(
            CRC32.CalculateCRC32(data, 1, 3),
            CRC32.CalculateCRC32(data.AsSpan(1, 3)));
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    /// <summary>Calls the action synchronously, avoiding Progress&lt;T&gt;'s thread-pool posting.</summary>
    private sealed class SyncProgress<T>(Action<T> action) : IProgress<T>
    {
        public void Report(T value) => action(value);
    }

    /// <summary>Wraps a MemoryStream but hides CanSeek to exercise the non-seekable code path.</summary>
    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) =>
            inner.ReadAsync(buffer, offset, count, ct);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    }
}
