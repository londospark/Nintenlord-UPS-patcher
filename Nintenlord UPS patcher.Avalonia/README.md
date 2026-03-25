# Nintenlord UPS patcher (Avalonia)

This is a Linux-compatible GUI port of the original WinForms app.

Use `Nintenlord UPS patcher.slnx` as the primary solution file. The Avalonia app is configured as startup.

## Features

- Apply a `.ups` patch to a target file
- Create a `.ups` patch from original and modified files
- Inspect patch offset/length data

## Build

```bash
dotnet build "Nintenlord UPS patcher.Avalonia/Nintenlord UPS patcher.Avalonia.csproj"
```

## Run

```bash
dotnet run --project "Nintenlord UPS patcher.Avalonia/Nintenlord UPS patcher.Avalonia.csproj"
```

## Tests

```bash
dotnet test "tests/Nintenlord.Hacking.Core.Tests/Nintenlord.Hacking.Core.Tests.csproj"
```


