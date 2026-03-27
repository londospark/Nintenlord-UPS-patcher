namespace Nintenlord.Hacking.Core.Tests;

public class UPSfileOffsetTests
{
    // One run: offset 2, length 1
    private static UPSfile MakeSingleRunPatch() =>
        new(new byte[] { 0, 0, 0xAA, 0, 0 },
            new byte[] { 0, 0, 0xFF, 0, 0 });

    // One run: offset 1, length 3 (bytes 1–3 all changed)
    private static UPSfile MakeLongRunPatch() =>
        new(new byte[] { 0, 0x11, 0x22, 0x33, 0 },
            new byte[] { 0, 0xAA, 0xBB, 0xCC, 0 });

    // Two separate single-byte runs: offset 1, offset 3
    private static UPSfile MakeTwoRunPatch() =>
        new(new byte[] { 0, 0xAA, 0, 0xBB, 0 },
            new byte[] { 0, 0xFF, 0, 0xCC, 0 });

    // ── ChangesOffset ─────────────────────────────────────────────────────────

    [Fact]
    public void ChangesOffset_InsideRun_ReturnsTrue() => Assert.True(MakeSingleRunPatch().ChangesOffset(2));

    [Fact]
    public void ChangesOffset_BeforeRun_ReturnsFalse() => Assert.False(MakeSingleRunPatch().ChangesOffset(0));

    [Fact]
    public void ChangesOffset_AfterRun_ReturnsFalse() => Assert.False(MakeSingleRunPatch().ChangesOffset(3));

    [Fact]
    public void ChangesOffset_FirstByteOfLongRun_ReturnsTrue() => Assert.True(MakeLongRunPatch().ChangesOffset(1));

    [Fact]
    public void ChangesOffset_LastByteOfLongRun_ReturnsTrue() => Assert.True(MakeLongRunPatch().ChangesOffset(3));

    [Fact]
    public void ChangesOffset_FirstRunInTwoRunPatch_ReturnsTrue() => Assert.True(MakeTwoRunPatch().ChangesOffset(1));

    [Fact]
    public void ChangesOffset_SecondRunInTwoRunPatch_ReturnsTrue() => Assert.True(MakeTwoRunPatch().ChangesOffset(3));

    [Fact]
    public void ChangesOffset_GapBetweenTwoRuns_ReturnsFalse() => Assert.False(MakeTwoRunPatch().ChangesOffset(2));

    // ── ChangeOffsets ─────────────────────────────────────────────────────────

    [Fact]
    public void ChangeOffsets_RangeContainsEntireRun_ReturnsTrue() => Assert.True(MakeSingleRunPatch().ChangeOffsets(1, 3));

    [Fact]
    public void ChangeOffsets_RangeBeforeRun_ReturnsFalse() => Assert.False(MakeSingleRunPatch().ChangeOffsets(0, 1));

    [Fact]
    public void ChangeOffsets_RangeAfterRun_ReturnsFalse() => Assert.False(MakeSingleRunPatch().ChangeOffsets(3, 2));

    [Fact]
    public void ChangeOffsets_RangeOverlapsRunFromLeft_ReturnsTrue() =>
        // run at [2,3), range [1,3) overlaps
        Assert.True(MakeSingleRunPatch().ChangeOffsets(1, 2));

    [Fact]
    public void ChangeOffsets_RangeOverlapsFirstOfTwoRuns_ReturnsTrue() => Assert.True(MakeTwoRunPatch().ChangeOffsets(0, 2));
}
