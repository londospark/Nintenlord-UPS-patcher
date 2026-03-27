using FsCheck.Xunit;

namespace Nintenlord.Hacking.Core.Tests;

public class NLZFilePropertyTests
{
    // ── Free space ────────────────────────────────────────────────────────────

    /// After adding a free space range, IsSpaceFree returns true for that exact range.
    [Property(MaxTest = 200)]
    public bool AddFreeSpace_ThenIsSpaceFree_ReturnsTrue(int offset, int amount)
    {
        if (offset < 0 || amount <= 0) return true;

        var file = new NLZFile();
        file.AddFreeSpace(offset, amount);
        return file.IsSpaceFree(offset, amount);
    }

    /// Adding then removing the same range leaves no free space at that location.
    [Property(MaxTest = 200)]
    public bool AddThenRemoveFreeSpace_IsNoLongerFree(int offset, int amount)
    {
        if (offset < 0 || amount <= 0) return true;

        var file = new NLZFile();
        file.AddFreeSpace(offset, amount);
        file.RemoveFreeSpace(offset, amount);
        return !file.IsSpaceFree(offset, amount);
    }

    /// When free space exists, GetFreeData(1) returns a non-negative offset, and
    /// that offset itself is free.
    [Property(MaxTest = 200)]
    public bool GetFreeData_WhenSpaceExists_ReturnsAFreeOffset(int offset, int amount)
    {
        if (offset < 0 || amount <= 0) return true;

        var file = new NLZFile();
        file.AddFreeSpace(offset, amount);

        var result = file.GetFreeData(1);
        return result >= 0 && file.IsSpaceFree(result, 1);
    }

    /// GetFreeData returns -1 when the requested size exceeds all available blocks.
    [Property(MaxTest = 200)]
    public bool GetFreeData_WhenRequestTooLarge_ReturnsMinusOne(int offset, int amount)
    {
        if (offset < 0 || amount <= 0) return true;
        // Request one byte more than the available block.
        if (amount == int.MaxValue) return true;

        var file = new NLZFile();
        file.AddFreeSpace(offset, amount);
        return file.GetFreeData(amount + 1) == -1;
    }

    /// Two non-overlapping free ranges are both independently reported as free.
    [Property(MaxTest = 150)]
    public bool AddTwoNonOverlappingRanges_BothAreFree(int offset1, int offset2, int amount)
    {
        if (offset1 < 0 || offset2 < 0 || amount <= 0) return true;
        // Ensure the second range starts after the first ends.
        var start2 = offset1 + amount + 1 + Math.Abs(offset2);

        var file = new NLZFile();
        file.AddFreeSpace(offset1, amount);
        file.AddFreeSpace(start2, amount);
        return file.IsSpaceFree(offset1, amount) && file.IsSpaceFree(start2, amount);
    }

    // ── App data ──────────────────────────────────────────────────────────────

    /// Any int stored under a key can be retrieved unchanged.
    [Property(MaxTest = 200)]
    public bool AppData_Int_RoundTrip(int value)
    {
        var file = new NLZFile();
        file.AddAppData("k", value);
        return file.GetAppData<int>("k") == value;
    }

    /// Any string stored under a key can be retrieved unchanged.
    [Property(MaxTest = 200)]
    public bool AppData_String_RoundTrip(string value)
    {
        if (value is null) return true;

        var file = new NLZFile();
        file.AddAppData("k", value);
        return file.GetAppData<string>("k") == value;
    }

    /// Overwriting a key with a new value is reflected on the next read.
    [Property(MaxTest = 150)]
    public bool AppData_Overwrite_ReturnsLatestValue(int first, int second)
    {
        var file = new NLZFile();
        file.AddAppData("k", first);
        file.AddAppData("k", second);
        return file.GetAppData<int>("k") == second;
    }

    // ── CRC32 / FileSize ──────────────────────────────────────────────────────

    /// CRC32 and FileSize are simple pass-through properties.
    [Property(MaxTest = 200)]
    public bool CRC32_RoundTrip(int value)
    {
        var file = new NLZFile();
        file.CRC32 = value;
        return file.CRC32 == value;
    }

    [Property(MaxTest = 200)]
    public bool FileSize_RoundTrip(int value)
    {
        var file = new NLZFile();
        file.FileSize = value;
        return file.FileSize == value;
    }
}
