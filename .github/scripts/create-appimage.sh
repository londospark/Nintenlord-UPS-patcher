#!/bin/bash
# Create AppImage for Linux
set -e

RUNTIME="$1"
ARCH="$2"
BUILD_DIR="$3"

if [ -z "$RUNTIME" ] || [ -z "$ARCH" ] || [ -z "$BUILD_DIR" ]; then
  echo "Usage: $0 <runtime> <arch> <build_dir>"
  echo "Example: $0 linux-x64 x86_64 builds/linux-x64"
  exit 1
fi

# Create AppImage structure
mkdir -p AppDir/usr/bin
mkdir -p AppDir/usr/share/applications
mkdir -p AppDir/usr/share/icons/hicolor/256x256/apps

# Copy executable
cp -r "$BUILD_DIR"/* AppDir/usr/bin/
chmod +x AppDir/usr/bin/Nintenlord\ UPS\ patcher.Avalonia || true

# Create desktop file
cat > AppDir/usr/share/applications/nups.desktop << 'EOF'
[Desktop Entry]
Type=Application
Name=Nintenlord UPS Patcher
Exec=Nintenlord UPS patcher.Avalonia
Icon=nups
Categories=Utility;
Terminal=false
EOF

# Create AppRun
cat > AppDir/AppRun << 'EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
export PATH="${HERE}/usr/bin:${PATH}"
exec "${HERE}/usr/bin/Nintenlord UPS patcher.Avalonia" "$@"
EOF
chmod +x AppDir/AppRun

# Copy icon if exists
if [ -f "Nintenlord UPS patcher/NUPS icon.ico" ]; then
  cp "Nintenlord UPS patcher/NUPS icon.ico" AppDir/usr/share/icons/hicolor/256x256/apps/nups.ico
fi

ln -s usr/share/applications/nups.desktop AppDir/nups.desktop
if [ -f AppDir/usr/share/icons/hicolor/256x256/apps/nups.ico ]; then
  ln -s usr/share/icons/hicolor/256x256/apps/nups.ico AppDir/nups.ico
fi

# Download appimagetool
wget -O appimagetool "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-${ARCH}.AppImage"
chmod +x appimagetool

# Build AppImage
ARCH="$ARCH" ./appimagetool AppDir "Nintenlord-UPS-Patcher-${RUNTIME}.AppImage"

echo "✓ Created AppImage: Nintenlord-UPS-Patcher-${RUNTIME}.AppImage"
