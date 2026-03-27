namespace Nintenlord.Hacking.Core.Tests.TestData;

public sealed class ByteArrayPair(byte[] first, byte[] second)
{
    public byte[] First
    {
        get;
    } = first;

    public byte[] Second
    {
        get;
    } = second;
}



