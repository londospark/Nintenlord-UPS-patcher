// EditorConfig Style Examples
// This file demonstrates the preferred code style for this project

namespace NintenlordUPSPatcher.Examples;

/// <summary>
/// Example class demonstrating preferred var usage and expression bodies.
/// </summary>
public class StyleExamples
{
    // Constants - PascalCase, no underscore
    public const int MaxRetries = 3;
    public const string DefaultFormat = "N2";

    // Private fields - _camelCase with underscore prefix
    private string _filePath = string.Empty;
    private int _fileSize;
    private List<byte> _buffer = new();

    // Properties - PascalCase
    // Preferred: Expression body for simple getters
    public string FilePath => _filePath;

    // Preferred: Auto-property with init
    public string FileName { get; init; } = string.Empty;

    // Preferred: Expression body for computed property
    public int BufferCount => _buffer.Count;

    /// <summary>
    /// Example method showing var preference and expression body.
    /// </summary>
    /// <param name="items">Items to process.</param>
    /// <returns>Processed items.</returns>
    public IEnumerable<string> ProcessItems(IEnumerable<int> items)
    {
        // ✅ Preferred: var with obvious type from method call
        var results = items
            .Where(x => x > 0)
            .Select(x => x.ToString())
            .ToList();

        return results;
    }

    /// <summary>
    /// Example showing var with new expressions.
    /// </summary>
    public void WorkWithCollections()
    {
        // ✅ Preferred: var with collection initializer
        var names = new List<string> { "Alice", "Bob", "Charlie" };

        // ✅ Preferred: var with dictionary
        var lookupTable = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        // ✅ Preferred: var with LINQ query
        var filtered = names
            .Where(n => n.Length > 3)
            .OrderBy(n => n)
            .ToList();

        // ✅ Preferred: var with method call where type is clear
        var firstItem = names.FirstOrDefault();
        var itemCount = names.Count;
    }

    /// <summary>
    /// Simple method - good candidate for expression body.
    /// </summary>
    public bool IsValidFile(string path) => !string.IsNullOrEmpty(path) && File.Exists(path);

    /// <summary>
    /// Simple property getter - preferred expression body style.
    /// </summary>
    public int GetFileSize() => _fileSize;

    /// <summary>
    /// Method with logic - can use expression body if single expression.
    /// </summary>
    public string FormatFileSize() =>
        _fileSize > 0
            ? $"{_fileSize / 1024.0:F2} KB"
            : "Unknown";

    /// <summary>
    /// Multi-line expression body for complex expressions.
    /// </summary>
    public ValidationResult ValidateFile(string filePath) =>
        string.IsNullOrEmpty(filePath) ? ValidationResult.InvalidPath :
        !File.Exists(filePath) ? ValidationResult.FileNotFound :
        ValidateFileContent(filePath);

    /// <summary>
    /// Method showing pattern matching preference.
    /// </summary>
    public string GetTypeDescription(object obj) =>
        obj switch
        {
            string s => $"String: {s}",
            int i => $"Integer: {i}",
            null => "Null value",
            _ => "Unknown type"
        };

    /// <summary>
    /// Method showing null-coalescing preference.
    /// </summary>
    public string GetDisplayName(string? firstName, string? lastName, string? nickname)
    {
        // ✅ Preferred: null-coalescing and null-coalescing assignment
        var display = nickname ?? firstName ?? lastName ?? "Unknown";
        return display;
    }

    /// <summary>
    /// Example showing inlined variable declarations.
    /// </summary>
    public void ProcessFileContent(string content)
    {
        // ✅ Preferred: inlined declaration with pattern matching
        if (int.TryParse(content.Trim(), out var number))
        {
            Console.WriteLine($"Parsed number: {number}");
        }

        // ✅ Preferred: using statement with inlined declaration
        using var reader = new StringReader(content);
        var line = reader.ReadLine();
    }

    /// <summary>
    /// Example showing throw expression.
    /// </summary>
    public void SetFilePath(string path) =>
        _filePath = string.IsNullOrEmpty(path) ? throw new ArgumentException(nameof(path)) : path;

    /// <summary>
    /// Example with object and collection initializers.
    /// </summary>
    public FileInfo CreateFileInfo()
    {
        // ✅ Preferred: var with object initializer
        var info = new FileInfo("example.txt")
        {
            // Note: FileInfo doesn't have settable properties in this example,
            // but demonstrates the pattern
        };

        return info;
    }

    /// <summary>
    /// Example showing conditional expression preference.
    /// </summary>
    /// <param name="age">Person's age.</param>
    /// <returns>Age category.</returns>
    public string CategorizeAge(int age)
    {
        // ✅ Preferred: conditional expression
        var category = age < 13 ? "Child" :
                      age < 18 ? "Teenager" :
                      age < 65 ? "Adult" :
                      "Senior";

        return category;
    }

    /// <summary>
    /// Local method example - preferred for helper functions.
    /// </summary>
    public void ProcessList(List<int> items)
    {
        // ✅ Preferred: static local function when no closure needed
        static bool IsPositive(int x) => x > 0;

        // ✅ Preferred: using LINQ with local function
        var positive = items.Where(IsPositive).ToList();
    }

    /// <summary>
    /// Example with compound assignment.
    /// </summary>
    public void UpdateValue(int delta) =>
        // ✅ Preferred: compound assignment operators
        _fileSize += delta;

    /// <summary>
    /// Indexer - preferred expression body style.
    /// </summary>
    public byte this[int index] => _buffer[index];

    /// <summary>
    /// Accessor with expression body.
    /// </summary>
    private string CachedPath { get => _filePath; set => _filePath = value; }
}

/// <summary>
/// Example enum - PascalCase for enum and members.
/// </summary>
public enum ValidationResult
{
    Valid = 0,
    InvalidPath = 1,
    FileNotFound = 2,
    InvalidContent = 3
}

/// <summary>
/// Record type - modern C# feature compatible with expression bodies.
/// </summary>
public record FileRecord(
    string Path,
    long Size,
    DateTime CreatedDate)
{
    // ✅ Preferred: expression body in record
    public bool IsLargeFile => Size > 1_000_000;

    // ✅ Preferred: expression body for method
    public string GetSummary() => $"{Path} ({Size} bytes)";
}

/// <summary>
/// Struct with expression bodies.
/// </summary>
public struct Point
{
    private readonly double _x;
    private readonly double _y;

    // ✅ Preferred: expression bodies in struct
    public double X => _x;
    public double Y => _y;
    public double Distance => Math.Sqrt(_x * _x + _y * _y);

    public Point(double x, double y)
    {
        _x = x;
        _y = y;
    }
}

