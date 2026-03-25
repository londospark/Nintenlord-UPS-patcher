namespace Nintenlord.Hacking.Core.Tests.TestData;

public sealed class ByteArrayPair
{
    public ByteArrayPair(byte[] first, byte[] second)
    {
        First = first;
        Second = second;
    }

    public byte[] First
    {
        get;
    }

    public byte[] Second
    {
        get;
    }
}



