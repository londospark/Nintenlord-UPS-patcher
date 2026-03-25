# Create Windows self-extracting installer
param(
    [Parameter(Mandatory=$true)]
    [string]$Runtime
)

$ErrorActionPreference = "Stop"

Write-Host "Creating Windows installer for $Runtime..."

$publishPath = "publish\$Runtime"
$tempDir = "temp-installer-$Runtime"
$installerName = "Nintenlord-UPS-Patcher-Setup-$Runtime.exe"

# Create temp directory
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Copy published files
Write-Host "Copying files from $publishPath..."
Copy-Item -Path "$publishPath\*" -Destination $tempDir -Recurse

# Copy setup script
Copy-Item -Path ".github\installers\setup.bat" -Destination $tempDir

# Download 7zSD.sfx (self-extracting module)
Write-Host "Downloading 7-Zip SFX module..."
$sfxUrl = "https://github.com/chrislake/7zsfx/releases/download/v1.9.1/7zSD.sfx"
$sfxPath = "7zSD.sfx"
if (-not (Test-Path $sfxPath)) {
    Invoke-WebRequest -Uri $sfxUrl -OutFile $sfxPath
}

# Create 7z archive
Write-Host "Creating archive..."
& "C:\Program Files\7-Zip\7z.exe" a -t7z -mx9 "temp-archive.7z" ".\$tempDir\*"

# Combine SFX + config + archive
Write-Host "Building self-extracting installer..."
cmd /c copy /b "$sfxPath" + ".github\installers\installer-config.txt" + "temp-archive.7z" "$installerName"

# Cleanup
Remove-Item -Force "temp-archive.7z"
Remove-Item -Recurse -Force $tempDir

Write-Host "✓ Created installer: $installerName"
Write-Host "  - No admin rights required"
Write-Host "  - Installs to: %LOCALAPPDATA%\Programs\NUPS"
Write-Host "  - Creates Start Menu shortcut"
Write-Host "  - Optional desktop shortcut"
