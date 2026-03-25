#!/bin/bash
# Create macOS universal .app bundle
set -e

ARM64_BUILD="$1"
X64_BUILD="$2"

if [ -z "$ARM64_BUILD" ] || [ -z "$X64_BUILD" ]; then
  echo "Usage: $0 <arm64_build_dir> <x64_build_dir>"
  exit 1
fi

# Create .app structure
mkdir -p "NUPS.app/Contents/MacOS"
mkdir -p "NUPS.app/Contents/Resources"

# Create Info.plist
cat > "NUPS.app/Contents/Info.plist" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>NUPS</string>
    <key>CFBundleIdentifier</key>
    <string>com.londospark.nups</string>
    <key>CFBundleName</key>
    <string>Nintenlord UPS Patcher</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
</dict>
</plist>
EOF

# Copy icon if exists
if [ -f "Nintenlord UPS patcher/NUPS icon.ico" ]; then
  cp "Nintenlord UPS patcher/NUPS icon.ico" "NUPS.app/Contents/Resources/NUPS.ico"
fi

# Create launcher script that detects architecture
cat > "NUPS.app/Contents/MacOS/NUPS" << 'EOF'
#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ARCH=$(uname -m)

if [ "$ARCH" = "arm64" ]; then
  exec "$DIR/NUPS-arm64" "$@"
else
  exec "$DIR/NUPS-x64" "$@"
fi
EOF
chmod +x "NUPS.app/Contents/MacOS/NUPS"

# Copy both architecture binaries
cp -r "$ARM64_BUILD"/* "NUPS.app/Contents/MacOS/"
mv "NUPS.app/Contents/MacOS/Nintenlord UPS patcher.Avalonia" "NUPS.app/Contents/MacOS/NUPS-arm64"

cp -r "$X64_BUILD"/* "NUPS.app/Contents/MacOS/"
mv "NUPS.app/Contents/MacOS/Nintenlord UPS patcher.Avalonia" "NUPS.app/Contents/MacOS/NUPS-x64"

# Make binaries executable
chmod +x "NUPS.app/Contents/MacOS/NUPS-arm64"
chmod +x "NUPS.app/Contents/MacOS/NUPS-x64"

# Create DMG
hdiutil create -volname "NUPS" -srcfolder "NUPS.app" -ov -format UDZO "Nintenlord-UPS-Patcher-macOS-universal.dmg"

echo "✓ Created macOS universal app bundle: Nintenlord-UPS-Patcher-macOS-universal.dmg"
