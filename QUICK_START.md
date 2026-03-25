# EditorConfig Setup - Quick Start

## 🚀 Files You Need to Know About

### 1. **`.editorconfig`** - THE IMPORTANT ONE
- **Location**: Root directory of the project
- **Purpose**: Defines ALL code style rules
- **Auto-detected**: By Rider and Visual Studio
- **Edit**: Only if you want to change style rules

### 2. **`Nintenlord UPS patcher.sln.DotSettings`**
- **Location**: Root directory
- **Purpose**: Rider-specific settings
- **Auto-detected**: By Rider
- **Edit**: Usually not needed

## 📋 What's Configured

### var Preference
```csharp
// These are now PREFERRED
var name = "John";
var items = new List<int>();
var count = GetCount();
```

### Expression Bodies
```csharp
// These are now PREFERRED
public string Name => _name;
public int Age => _age;
public bool IsValid() => _valid;
```

### Naming
```csharp
private string _fieldName;        // Private fields
public string PropertyName { }    // Properties
void MethodName() { }             // Methods
var localVariable = 5;            // Locals
const int CONSTANT = 10;          // Constants
```

## ⚡ Most Used Commands

```bash
# Format everything
dotnet format "Nintenlord UPS patcher.slnx"

# Check without changing
dotnet format "Nintenlord UPS patcher.slnx" --verify-no-changes

# Build
dotnet build
```

## 🎯 In Rider

- **Format File**: Press `Ctrl+Alt+L`
- **Code Cleanup**: Code → Run Code Cleanup
- **Check Settings**: Settings → Editor → Code Style

## 📖 For More Info

- Need examples? → See `STYLE_EXAMPLES.cs`
- Need details? → See `EDITORCONFIG_SETUP.md`
- Need quick answers? → See `CODE_FORMATTING_REFERENCE.md`

## ✅ Current Status

- ✅ All code formatted
- ✅ Compiles successfully (0 errors)
- ✅ Rider configured
- ✅ Ready to develop

---

**That's it! The system is set up and ready to use.**

