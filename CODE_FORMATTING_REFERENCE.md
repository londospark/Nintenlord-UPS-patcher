# Code Formatting & EditorConfig Quick Reference

## What Was Done

✅ **Created `.editorconfig`** - Root-level configuration file that enforces:
  - Preference for `var` keyword (all scenarios)
  - Expression body members for simple methods/properties
  - UTF-8 encoding and consistent indentation
  - Standard C# naming conventions

✅ **Created Rider settings** - `Nintenlord UPS patcher.sln.DotSettings` file that:
  - Enables EditorConfig support
  - Configures code cleanup options
  - Enables auto-formatting on save

✅ **Applied full code formatting** - Entire solution has been reformatted with:
  - Whitespace fixes
  - Proper indentation
  - Consistent line endings (CRLF)
  - Charset standardization (UTF-8)

## Usage Commands

### Format the Entire Solution
```bash
cd X:\Code\Nintenlord-UPS-patcher
dotnet format "Nintenlord UPS patcher.slnx"
```

### Verify Formatting Without Changes
```bash
dotnet format "Nintenlord UPS patcher.slnx" --verify-no-changes
```

### Format Specific File
```bash
dotnet format --include <filepath>
```

### Build Solution
```bash
dotnet build
```

## Rider IDE Usage

### Auto-Format Current File
- **Windows/Linux**: `Ctrl+Alt+L`
- **Mac**: `Cmd+Option+L`

### Code Cleanup
1. Go to **Code → Run Code Cleanup**
2. Select the cleanup scope (file/folder/solution)
3. Profile should be "Custom: Reformat Code"

### Check EditorConfig Status
1. **Settings → Editor → Code Style**
2. Look for "EditorConfig support: Enabled"
3. Verify rules are loaded from `.editorconfig`

## Key Style Rules

### var Usage (Preferred)
```csharp
// ✅ Good
var message = "Hello";
var count = 42;
var items = new List<string>();
var result = SomeMethod();

// ❌ Avoid
string message = "Hello";
int count = 42;
List<string> items = new List<string>();
SomeMethod() result = SomeMethod();
```

### Expression Bodies (Preferred)
```csharp
// ✅ Good - Properties
public string Name => _name;
public int Age => _age;
public bool IsActive => _isActive;

// ✅ Good - Simple Methods
public int GetAge() => DateTime.Now.Year - _birthYear;
public string ToString() => _name;

// ❌ Avoid
public string Name { get { return _name; } }
public int GetAge() { return DateTime.Now.Year - _birthYear; }
```

### Naming Conventions
```csharp
// Constants and Static Readonly - PascalCase
public const int MaxSize = 100;
public static readonly string ApplicationName = "NUPS";

// Private Fields - _camelCase
private string _fieldName;
private int _count;

// Properties and Public Members - PascalCase
public string PropertyName { get; set; }
public int CalculateValue() { }

// Local Variables and Parameters - camelCase
void MyMethod(int parameterName)
{
    var localVariable = 0;
    const string localConstant = "value";
}
```

## File Structure

```
Nintenlord-UPS-patcher/
├── .editorconfig                          # Main config file
├── Nintenlord UPS patcher.sln.DotSettings # Rider settings
├── EDITORCONFIG_SETUP.md                  # Detailed documentation
├── CODE_FORMATTING_REFERENCE.md           # This file
└── [source files]
```

## Build Status

After formatting:
- ✅ **Whitespace Issues**: All fixed
- ✅ **Code Compiles**: Successfully (0 errors)
- ⚠️ **Warnings**: 39 (mostly nullability-related, not formatting)
- ⚠️ **CVE**: System.Drawing.Common 5.0.2 has known vulnerability

## For New Developers

When starting work on this project:

1. Verify Rider has EditorConfig support enabled
2. Open a C# file and use `Ctrl+Alt+L` to format
3. Write code following the style rules above
4. Before committing: `dotnet format "Nintenlord UPS patcher.slnx"`
5. Commit the formatted code

## EditorConfig File Location
- **Path**: `X:\Code\Nintenlord-UPS-patcher\.editorconfig`
- **Applies to**: All files in the repository and subdirectories
- **Format**: INI-style configuration
- **Editor Support**: VS Code, Visual Studio, JetBrains IDEs, and more

## Troubleshooting

### Formatting Not Applied in Rider
1. Check **Settings → Editor → Code Style**
2. Ensure "Enable EditorConfig support" is checked
3. Restart Rider if needed
4. Re-open the `.editorconfig` file in editor

### dotnet format Command Not Found
```bash
# Install latest version
dotnet tool install -g dotnet-format

# Or update if already installed
dotnet tool update -g dotnet-format
```

### Need to Adjust Rules
1. Edit the `.editorconfig` file
2. Change the rule values (see EditorConfig documentation)
3. Save the file
4. Rider will automatically detect changes
5. Re-format your code

## References
- **EditorConfig**: https://editorconfig.org/
- **.NET Code Style Rules**: https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/
- **C# Coding Conventions**: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- **JetBrains EditorConfig**: https://www.jetbrains.com/help/rider/settings_editorconfig.html

