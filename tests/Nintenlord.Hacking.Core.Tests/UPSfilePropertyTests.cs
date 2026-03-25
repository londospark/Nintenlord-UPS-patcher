using FsCheck.Xunit;

namespace Nintenlord.Hacking.Core.Tests;

public class UPSfilePropertyTests
{
    [Property(MaxTest = 120)]
    public bool ApplyThenApplyAgain_ReturnsOriginal_ForEqualLengthFiles(byte[] first, byte[] second)
    {
        if (first == null || second == null || first.Length == 0 || second.Length == 0)
        {
            return true;
        }

        var target = NormalizeToLength(second, first.Length);
        if (first.SequenceEqual(target))
        {
            return true;
        }

        var patch = new UPSfile(first, target);

        var once = patch.Apply(first);
        var twice = patch.Apply(once);

        return once.SequenceEqual(target) && twice.SequenceEqual(first);
    }

    [Property(MaxTest = 120)]
    public bool ValidToApply_TrueForBothSides_AndFalseForMutated(byte[] first, byte[] second)
    {
        if (first == null || second == null || first.Length == 0 || second.Length == 0)
        {
            return true;
        }

        var target = NormalizeToLength(second, first.Length);
        if (first.SequenceEqual(target))
        {
            return true;
        }

        var patch = new UPSfile(first, target);
        var mutated = (byte[])first.Clone();
        mutated[0] ^= 0xFF;

        return patch.ValidToApply(first)
               && patch.ValidToApply(target)
               && !patch.ValidToApply(mutated);
    }

    [Property(MaxTest = 90)]
    public bool PatchAddition_ComposesTransformations(byte[] source, byte[] mid, byte[] end)
    {
        if (source == null || mid == null || end == null || source.Length == 0)
        {
            return true;
        }

        var target1 = NormalizeToLength(mid, source.Length);
        var target2 = NormalizeToLength(end, source.Length);
        var first = new UPSfile(source, target1);
        var second = new UPSfile(target1, target2);
        var combined = first + second;

        var sequential = second.Apply(first.Apply(source));
        var combinedResult = combined.Apply(source);

        return sequential.SequenceEqual(combinedResult);
    }

    private static byte[] NormalizeToLength(byte[] data, int length)
    {
        if (data.Length == length)
        {
            return (byte[])data.Clone();
        }

        var output = new byte[length];
        var copyLength = Math.Min(data.Length, length);
        Array.Copy(data, output, copyLength);
        return output;
    }
}





