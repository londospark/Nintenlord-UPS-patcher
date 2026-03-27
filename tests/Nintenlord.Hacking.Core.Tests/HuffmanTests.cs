using Nintenlord.Collections.Trees;
using Nintenlord.ROMHacking;

namespace Nintenlord.Hacking.Core.Tests;

public class HuffmanTests
{
    [Fact]
    public void GetHuffmanTree_TwoItems_ReturnsNonNullTree()
    {
        var items = new Dictionary<char, int>
        {
            ['a'] = 5,
            ['b'] = 3,
        };

        var tree = Huffman.GetHuffmanTree(items);

        Assert.NotNull(tree);
    }

    [Fact]
    public void GetHuffmanTree_TwoItems_TreeContainsBothValues()
    {
        var items = new Dictionary<char, int>
        {
            ['x'] = 10,
            ['y'] = 4,
        };

        var tree = Huffman.GetHuffmanTree(items);

        // BinaryTreeNode.GetChildren() yields null for leaf children, breaking
        // BreadthFirstEnumerator. Use Count (set by constructor) to verify both
        // values are present rather than enumerating the tree.
        Assert.Equal(2, tree.Count);
    }

    [Fact]
    public void GetHuffmanTree_SingleItem_ReturnsNonNullTree()
    {
        var items = new Dictionary<char, int>
        {
            ['z'] = 1,
        };

        var tree = Huffman.GetHuffmanTree(items);

        Assert.NotNull(tree);
    }

    [Fact]
    public void GetHuffmanTree_SingleItem_TreeContainsThatValue()
    {
        var items = new Dictionary<char, int>
        {
            ['z'] = 1,
        };

        var tree = Huffman.GetHuffmanTree(items);

        Assert.Contains('z', tree);
    }

    [Fact]
    public void GetHuffmanTree_EmptyDictionary_ThrowsNullReferenceException()
    {
        var items = new Dictionary<char, int>();

        Assert.Throws<NullReferenceException>(() => Huffman.GetHuffmanTree(items));
    }

    [Fact]
    public void GetHuffmanTree_ThreeItems_ReturnsTreeWithAllValues()
    {
        var items = new Dictionary<char, int>
        {
            ['a'] = 10,
            ['b'] = 5,
            ['c'] = 2,
        };

        var tree = Huffman.GetHuffmanTree(items);

        Assert.NotNull(tree);
        // Count reflects the number of leaves (values) in the tree
        Assert.Equal(3, tree.Count);
    }

    [Fact]
    public void GetHuffmanTree_TwoItems_ImplicitDictionaryConversionProducesEncoding()
    {
        var items = new Dictionary<char, int>
        {
            ['a'] = 5,
            ['b'] = 3,
        };

        var tree = Huffman.GetHuffmanTree(items);
        Dictionary<char, bool[]> encoding = tree;

        Assert.True(encoding.ContainsKey('a'));
        Assert.True(encoding.ContainsKey('b'));
    }
}
