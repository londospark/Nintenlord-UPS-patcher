using Nintenlord.Hacking.Core.MemoryManagement;

namespace Nintenlord.Hacking.Core.Tests;

public class ManagedPointerTests
{
    [Fact]
    public void NullPointer_IsNull_IsTrue()
    {
        Assert.True(ManagedPointer.NullPointer.IsNull);
    }

    [Fact]
    public void NullPointer_Offset_IsMinusOne()
    {
        Assert.Equal(-1, ManagedPointer.NullPointer.Offset);
    }

    [Fact]
    public void NullPointer_Size_IsZero()
    {
        Assert.Equal(0, ManagedPointer.NullPointer.Size);
    }

    [Fact]
    public void NullPointer_Pinned_IsFalse()
    {
        Assert.False(ManagedPointer.NullPointer.Pinned);
    }

    [Fact]
    public void NullPointer_OffsetAfter_IsMinusOne()
    {
        // OffsetAfter = Offset + Size = -1 + 0 = -1
        Assert.Equal(-1, ManagedPointer.NullPointer.OffsetAfter);
    }

    [Fact]
    public void NullPointer_CompareToNull_ReturnsPositive()
    {
        Assert.Equal(1, ManagedPointer.NullPointer.CompareTo(null));
    }

    [Fact]
    public void NullPointer_EqualsItself_ReturnsTrue()
    {
        Assert.True(ManagedPointer.NullPointer.Equals(ManagedPointer.NullPointer));
    }

    [Fact]
    public void NullPointer_EqualsNull_ReturnsFalse()
    {
        Assert.False(ManagedPointer.NullPointer.Equals(null));
    }

    [Fact]
    public void NullPointer_GetHashCode_ReturnsMinusOne()
    {
        Assert.Equal(-1, ManagedPointer.NullPointer.GetHashCode());
    }

    [Fact]
    public void NullPointer_ToString_ContainsHexFFFFF()
    {
        var str = ManagedPointer.NullPointer.ToString();

        Assert.Contains("FFFFF", str);
    }

    [Fact]
    public void NullPointer_IsSameReferenceAcrossAccesses()
    {
        Assert.Same(ManagedPointer.NullPointer, ManagedPointer.NullPointer);
    }
}
