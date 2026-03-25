using FsCheck.Xunit;

namespace Nintenlord.Hacking.Core.Tests;

public class BitUtilityPropertyTests
{
    [Property(MaxTest = 300)]
    public bool IntOffsetValidityAndInvalidityAreComplements(int value) => BitUtility.IsValidIntOffset(value) != BitUtility.IsInvalidIntOffset(value);

    [Property(MaxTest = 300)]
    public bool ShortOffsetValidityAndInvalidityAreComplements(int value) => BitUtility.IsValidShortOffset(value) != BitUtility.IsInvalidShortOffset(value);

    [Property(MaxTest = 300)]
    public bool IntAlignmentImpliesShortAlignment(int value) => !BitUtility.IsValidIntOffset(value) || BitUtility.IsValidShortOffset(value);
}


