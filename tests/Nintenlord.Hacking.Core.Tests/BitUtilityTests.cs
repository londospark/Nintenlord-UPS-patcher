namespace Nintenlord.Hacking.Core.Tests;

public class BitUtilityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(1024)]
    public void IsValidIntOffset_ReturnsTrueForFourByteAlignedValues(int value)
    {
        Assert.True(BitUtility.IsValidIntOffset(value));
        Assert.False(BitUtility.IsInvalidIntOffset(value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(6)]
    [InlineData(1023)]
    public void IsValidIntOffset_ReturnsFalseForNonAlignedValues(int value)
    {
        Assert.False(BitUtility.IsValidIntOffset(value));
        Assert.True(BitUtility.IsInvalidIntOffset(value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(100)]
    public void IsValidShortOffset_ReturnsTrueForTwoByteAlignedValues(int value)
    {
        Assert.True(BitUtility.IsValidShortOffset(value));
        Assert.False(BitUtility.IsInvalidShortOffset(value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(101)]
    public void IsValidShortOffset_ReturnsFalseForOddValues(int value)
    {
        Assert.False(BitUtility.IsValidShortOffset(value));
        Assert.True(BitUtility.IsInvalidShortOffset(value));
    }
}

