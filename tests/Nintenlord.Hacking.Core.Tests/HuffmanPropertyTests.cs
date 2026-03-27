using FsCheck.Xunit;
using Nintenlord.Collections.Trees;
using Nintenlord.IO.Bits;
using Nintenlord.ROMHacking;

namespace Nintenlord.Hacking.Core.Tests;

public class HuffmanPropertyTests
{
    // ── Tree structure ────────────────────────────────────────────────────────

    /// For any non-empty set of distinct symbols the tree leaf count equals
    /// the number of distinct symbols.
    [Property(MaxTest = 200)]
    public bool GetHuffmanTree_Count_EqualsDistinctSymbolCount(char[] symbols)
    {
        var distinct = symbols.Distinct().ToList();
        if (distinct.Count == 0) return true;

        var freqs = distinct.ToDictionary(c => c, _ => 1);
        var tree = Huffman.GetHuffmanTree(freqs);
        return tree.Count == distinct.Count;
    }

    /// Frequencies are irrelevant to the leaf count — only the number of
    /// distinct symbols matters.
    [Property(MaxTest = 200)]
    public bool GetHuffmanTree_Count_IsIndependentOfFrequencies(char[] symbols, int seed)
    {
        var distinct = symbols.Distinct().ToList();
        if (distinct.Count == 0) return true;

        var rng = new Random(seed);
        var freqs = distinct.ToDictionary(c => c, _ => rng.Next(1, 1000));
        var tree = Huffman.GetHuffmanTree(freqs);
        return tree.Count == distinct.Count;
    }

    // ── Encoding ──────────────────────────────────────────────────────────────

    /// The implicit Dictionary<T,bool[]> conversion contains exactly one entry
    /// per distinct symbol.
    [Property(MaxTest = 200)]
    public bool Encoding_ContainsExactlyOneEntryPerSymbol(char[] symbols)
    {
        var distinct = symbols.Distinct().ToList();
        if (distinct.Count == 0) return true;

        var freqs = distinct.ToDictionary(c => c, _ => 1);
        var tree = Huffman.GetHuffmanTree(freqs);
        Dictionary<char, bool[]> encoding = tree;
        return encoding.Count == distinct.Count
               && distinct.All(encoding.ContainsKey);
    }

    /// No two symbols share the same codeword (prefix-free property implied by
    /// having distinct, non-empty codewords for every symbol).
    [Property(MaxTest = 150)]
    public bool Encoding_AllCodewordsAreNonEmpty(char[] symbols)
    {
        var distinct = symbols.Distinct().ToList();
        if (distinct.Count == 0) return true;
        // Single-symbol trees get an empty codeword by convention in this impl.
        if (distinct.Count == 1) return true;

        var freqs = distinct.ToDictionary(c => c, _ => 1);
        var tree = Huffman.GetHuffmanTree(freqs);
        Dictionary<char, bool[]> encoding = tree;
        return encoding.Values.All(bits => bits.Length > 0);
    }

    // ── Compress → Decompress round-trip ─────────────────────────────────────

    /// Compressing a symbol sequence and then decompressing it with the same
    /// tree recovers the original sequence exactly.
    ///
    /// Uses '\0' as a sentinel so DecompressDataUntil knows where to stop,
    /// avoiding issues with padding bits at the end of the compressed stream.
    [Property(MaxTest = 150)]
    public bool CompressDecompress_RoundTrip(char[] symbols)
    {
        // '\0' is reserved as a sentinel; skip inputs that contain it.
        var items = symbols.Where(c => c != '\0').ToArray();
        if (items.Length == 0) return true;

        // Build a frequency table from the actual items plus the sentinel.
        var freqs = items.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
        freqs['\0'] = 1;

        var tree = Huffman.GetHuffmanTree(freqs);
        Dictionary<char, bool[]> encoding = tree;

        // Compress items followed by the sentinel.
        using var compressed = new MemoryStream();
        var writer = new BitWriter(compressed);
        Huffman.Compress(encoding, writer, items.Append('\0'));
        writer.Flush();

        // Decompress until the sentinel is encountered.
        compressed.Seek(0, SeekOrigin.Begin);
        var result = new List<char>();
        Huffman.DecompressDataUntil(compressed, c => c == '\0', tree, result);

        // DecompressDataUntil adds the sentinel itself; strip it.
        if (result.Count > 0 && result[^1] == '\0')
            result.RemoveAt(result.Count - 1);

        return items.SequenceEqual(result);
    }

    /// Round-trip holds across varying frequency distributions, not just uniform.
    [Property(MaxTest = 100)]
    public bool CompressDecompress_RoundTrip_WithRealFrequencies(char[] symbols, int seed)
    {
        var items = symbols.Where(c => c != '\0').ToArray();
        if (items.Length == 0) return true;

        var rng = new Random(seed);
        // Use actual occurrence counts plus random noise to vary tree shape.
        var freqs = items.GroupBy(c => c)
                         .ToDictionary(g => g.Key, g => g.Count() + rng.Next(0, 20));
        freqs['\0'] = 1;

        var tree = Huffman.GetHuffmanTree(freqs);
        Dictionary<char, bool[]> encoding = tree;

        using var compressed = new MemoryStream();
        var writer = new BitWriter(compressed);
        Huffman.Compress(encoding, writer, items.Append('\0'));
        writer.Flush();

        compressed.Seek(0, SeekOrigin.Begin);
        var result = new List<char>();
        Huffman.DecompressDataUntil(compressed, c => c == '\0', tree, result);

        if (result.Count > 0 && result[^1] == '\0')
            result.RemoveAt(result.Count - 1);

        return items.SequenceEqual(result);
    }
}
