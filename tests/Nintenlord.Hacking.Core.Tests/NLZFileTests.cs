using System.Xml;

namespace Nintenlord.Hacking.Core.Tests;

public class NLZFileTests
{
    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        var file = new NLZFile();

        Assert.Equal(0, file.CRC32);
        Assert.Equal(0, file.FileSize);
    }

    [Fact]
    public void CRC32_CanBeSetAndRetrieved()
    {
        var file = new NLZFile();
        file.CRC32 = 0x1A2B3C4D;

        Assert.Equal(0x1A2B3C4D, file.CRC32);
    }

    [Fact]
    public void FileSize_CanBeSetAndRetrieved()
    {
        var file = new NLZFile();
        file.FileSize = 1024;

        Assert.Equal(1024, file.FileSize);
    }

    [Fact]
    public void AddAppData_SerializableType_StoresData()
    {
        var file = new NLZFile();

        file.AddAppData("myInt", 42);

        Assert.True(file.ContainsAppData("myInt"));
    }

    [Fact]
    public void AddAppData_NonSerializableType_ThrowsArgumentException()
    {
        var file = new NLZFile();

        Assert.Throws<ArgumentException>(() => file.AddAppData("key", new NonSerializableClass()));
    }

    [Fact]
    public void AddAppData_OverwritesExistingKey()
    {
        var file = new NLZFile();
        file.AddAppData("key", 1);
        file.AddAppData("key", 99);

        Assert.Equal(99, file.GetAppData<int>("key"));
    }

    [Fact]
    public void ContainsAppData_AbsentKey_ReturnsFalse()
    {
        var file = new NLZFile();

        Assert.False(file.ContainsAppData("missing"));
    }

    [Fact]
    public void ContainsAppData_PresentKey_ReturnsTrue()
    {
        var file = new NLZFile();
        file.AddAppData("present", "value");

        Assert.True(file.ContainsAppData("present"));
    }

    [Fact]
    public void RemoveAppData_PresentKey_ReturnsTrueAndRemoves()
    {
        var file = new NLZFile();
        file.AddAppData("key", 1);

        var result = file.RemoveAppData("key");

        Assert.True(result);
        Assert.False(file.ContainsAppData("key"));
    }

    [Fact]
    public void RemoveAppData_AbsentKey_ReturnsFalse()
    {
        var file = new NLZFile();

        Assert.False(file.RemoveAppData("missing"));
    }

    [Fact]
    public void GetAppData_CorrectType_ReturnsStoredValue()
    {
        var file = new NLZFile();
        file.AddAppData("count", 7);

        Assert.Equal(7, file.GetAppData<int>("count"));
    }

    [Fact]
    public void GetAppData_WrongType_ThrowsInvalidCastException()
    {
        var file = new NLZFile();
        file.AddAppData("count", 7);

        Assert.Throws<InvalidCastException>(() => file.GetAppData<string>("count"));
    }

    [Fact]
    public void IsSpaceFree_NoFreeSpaceAdded_ReturnsFalse()
    {
        var file = new NLZFile();

        Assert.False(file.IsSpaceFree(0, 1));
    }

    [Fact]
    public void AddFreeSpace_ThenIsSpaceFree_ReturnsTrue()
    {
        var file = new NLZFile();
        file.AddFreeSpace(100, 50);

        Assert.True(file.IsSpaceFree(100, 50));
    }

    [Fact]
    public void RemoveFreeSpace_RemovesPreviouslyAddedRange()
    {
        var file = new NLZFile();
        file.AddFreeSpace(100, 50);
        file.RemoveFreeSpace(100, 50);

        Assert.False(file.IsSpaceFree(100, 50));
    }

    [Fact]
    public void GetFreeData_NoFreeSpace_ReturnsMinusOne()
    {
        var file = new NLZFile();

        Assert.Equal(-1, file.GetFreeData(10));
    }

    [Fact]
    public void GetFreeData_FittingBlock_ReturnsBlockOffset()
    {
        var file = new NLZFile();
        file.AddFreeSpace(200, 100);

        Assert.Equal(200, file.GetFreeData(50));
    }

    [Fact]
    public void GetFreeData_RequestExceedsBlock_ReturnsMinusOne()
    {
        var file = new NLZFile();
        file.AddFreeSpace(200, 10);

        Assert.Equal(-1, file.GetFreeData(50));
    }

    [Fact]
    public void GetFreeData_BestFit_ReturnsSmallestFittingBlockOffset()
    {
        var file = new NLZFile();
        file.AddFreeSpace(0, 100);
        file.AddFreeSpace(300, 20);

        // Block at 300 (size 20) fits and is smaller than block at 0 (size 100)
        Assert.Equal(300, file.GetFreeData(10));
    }

    [Fact]
    public void GetSchema_ThrowsNotImplementedException()
    {
        var file = new NLZFile();

        Assert.Throws<NotImplementedException>(() => file.GetSchema());
    }

    [Fact]
    public void ReadXml_ThrowsNotImplementedException()
    {
        var file = new NLZFile();
        using var ms = new MemoryStream("<root/>"u8.ToArray());
        using var reader = XmlReader.Create(ms);

        Assert.Throws<NotImplementedException>(() => file.ReadXml(reader));
    }

    [Fact]
    public void WriteXml_ThrowsNotImplementedException()
    {
        var file = new NLZFile();
        using var ms = new MemoryStream();
        using var writer = XmlWriter.Create(ms);

        Assert.Throws<NotImplementedException>(() => file.WriteXml(writer));
    }

    private class NonSerializableClass { }
}
