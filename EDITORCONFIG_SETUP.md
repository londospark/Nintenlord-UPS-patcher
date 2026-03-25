# EditorConfig and Code Formatting Setup

## Overview

This project has been configured with a comprehensive `.editorconfig` file that enforces consistent code style across all C# files. The configuration prioritizes:

1. **`var` usage** - Prefer implicit typing where type is apparent
2. **Expression body members** - Use expression syntax for simple members
3. **Modern C# conventions** - Follow contemporary best practices

## Files Created

### 1. `.editorconfig` (Root Directory)
The main EditorConfig file that defines:

#### Global Settings
- **Charset**: UTF-8 for all files
- **Line Endings**: CRLF (Windows)
- **Indentation**: 4 spaces for C# files
- **Trailing Whitespace**: Removed automatically

#### C# Style Preferences

##### var Preferences (PREFER var)
```
csharp_style_var_for_built_in_types = true:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = true:suggestion
```

These settings encourage developers to use `var` for:
- Built-in types (e.g., `var x = 5;` instead of `int x = 5;`)
- Obvious types from the RHS (e.g., `var dict = new Dictionary<string, int>();`)
- All other scenarios where the type is clear from context

##### Expression-Bodied Members (PREFER expression bodies)
```
csharp_style_expression_bodied_methods = true:suggestion
csharp_style_expression_bodied_properties = true:suggestion
csharp_style_expression_bodied_indexers = true:suggestion
csharp_style_expression_bodied_accessors = true:suggestion
csharp_style_expression_bodied_operators = true:suggestion
```

Examples:
```csharp
// Preferred
public string Name => _name;
public int Calculate() => _value * 2;
public string Value { get => _value; set => _value = value; }

// Discouraged
public string Name { get { return _name; } }
public int Calculate() { return _value * 2; }
```

##### Other Style Preferences
- **Pattern matching**: Preferred over `is`/`as` checks
- **Null coalescing**: Encouraged for null-safe operations
- **Auto-properties**: Used when appropriate
- **Inlined variable declarations**: Encouraged with `is` and `using`

#### Formatting Rules
- **Braces**: Always on new lines
- **Indentation**: Consistent 4-space indentation
- **Spacing**: Standard C# conventions around operators and keywords

#### Naming Conventions
- **Constants & Static Readonly**: `PascalCase`
- **Private Fields**: `_camelCase` (leading underscore)
- **Properties & Public Members**: `PascalCase`
- **Local Variables & Parameters**: `camelCase`
- **Types & Namespaces**: `PascalCase`

### 2. `Nintenlord UPS patcher.sln.DotSettings`
JetBrains Rider settings that:
- Enables EditorConfig support
- Configures code cleanup on save
- Enables automatic code formatting

## Using the Configuration

### In Rider
The configuration is automatically detected and applied. You can:

1. **Format Code**: Use `Ctrl+Alt+L` (Windows) or `Cmd+Option+L` (Mac)
2. **Code Cleanup**: Use the code cleanup inspection to apply all rules
3. **Enable Auto-Format**: Files can be formatted automatically on save

### Manual Formatting
Run the following command to format the entire solution:

```bash
dotnet format "Nintenlord UPS patcher.slnx"
```

Or verify without applying changes:

```bash
dotnet format "Nintenlord UPS patcher.slnx" --verify-no-changes
```

## Build Warnings Status

After applying the full code formatting:
- **Whitespace formatting**: ✅ All fixed
- **Nullability issues**: ⚠️ Remaining (architectural - not formatting related)
- **System.Drawing.Common**: ⚠️ Known CVE (separate issue to address)

## IDE Setup

### Rider Integration
The `.editorconfig` file is automatically picked up by Rider. To verify:

1. Open **Settings → Editor → Code Style**
2. Verify "EditorConfig support" is enabled
3. Check that rules are loaded from `.editorconfig`

### Visual Studio Integration
Visual Studio 2019+ automatically respects `.editorconfig` files:

1. Install the latest Visual Studio version
2. No additional configuration needed
3. Formatting will respect the `.editorconfig` rules

## Future Development

When writing new code or making changes:

1. Code will be auto-formatted according to `.editorconfig` settings
2. Prefer `var` for type declarations
3. Use expression bodies for simple properties and methods
4. Run `dotnet format` before committing changes

## References

- [EditorConfig Documentation](https://editorconfig.org/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Code Style Rules](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/)

