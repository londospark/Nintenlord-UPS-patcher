# Create MSI installer using WiX Toolset v4
param(
    [Parameter(Mandatory=$true)]
    [string]$Runtime,

    [Parameter(Mandatory=$true)]
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "Creating MSI installer for $Runtime..."

# Ensure WiX v4 is installed
Write-Host "Checking for WiX Toolset..."
$wixPath = "wix.exe"

try {
    & $wixPath --version | Out-Null
} catch {
    Write-Host "Installing WiX Toolset v4..."
    dotnet tool install --global wix --version 4.0.5
    $env:PATH = "$env:PATH;$env:USERPROFILE\.dotnet\tools"
}

# Set variables
$publishDir = Resolve-Path "publish\$Runtime"
$sourceDir = Resolve-Path "."
$outputFile = "Nintenlord-UPS-Patcher-Setup-$Runtime.msi"

Write-Host "Build directory: $publishDir"
Write-Host "Source directory: $sourceDir"

# Build MSI
Write-Host "Building MSI with WiX..."
& wix build `
    ".github\installers\Product.wxs" `
    -out $outputFile `
    -d "PublishDir=$publishDir" `
    -d "SourceDir=$sourceDir" `
    -d "ProductVersion=$Version" `
    -ext WixToolset.UI.wixext

if ($LASTEXITCODE -ne 0) {
    Write-Error "WiX build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "✓ Created MSI: $outputFile"
Write-Host "  - Per-user installation (no admin required)"
Write-Host "  - Installs to: %LOCALAPPDATA%\NUPS"
Write-Host "  - Creates Start Menu and Desktop shortcuts"
