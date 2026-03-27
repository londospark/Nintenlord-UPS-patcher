#!/bin/bash
# Create a Type 2 AppImage for Linux manually.
# This avoids appimagetool entirely — we use mksquashfs + the official
# type2-runtime, concatenated together. No FUSE, no ARCH env var issues.
set -e

RUNTIME="$1"    # e.g. linux-x64, linux-arm64
ARCH="$2"       # e.g. x86_64, aarch64
BUILD_DIR="$3"

if [ -z "$RUNTIME" ] || [ -z "$ARCH" ] || [ -z "$BUILD_DIR" ]; then
  echo "Usage: $0 <runtime> <arch> <build_dir>"
  echo "Example: $0 linux-x64 x86_64 builds/linux-x64"
  exit 1
fi

OUTPUT="Nintenlord-UPS-Patcher-${RUNTIME}.AppImage"

# --- Build the AppDir structure ---
mkdir -p AppDir/usr/bin
mkdir -p AppDir/usr/share/applications
mkdir -p AppDir/usr/share/icons/hicolor/256x256/apps

cp -r "$BUILD_DIR"/* AppDir/usr/bin/
chmod +x AppDir/usr/bin/Nintenlord\ UPS\ patcher.Avalonia || true

# Desktop file — MimeType lets users double-click .ups files to open the patcher
cat > AppDir/nups.desktop << 'EOF'
[Desktop Entry]
Type=Application
Name=Nintenlord UPS Patcher
Exec=Nintenlord UPS patcher.Avalonia %f
Icon=nups
Categories=Utility;
MimeType=application/x-ups;
Terminal=false
EOF
cp AppDir/nups.desktop AppDir/usr/share/applications/nups.desktop

# AppRun entry point — sets LD_LIBRARY_PATH so bundled libs are found on SteamOS
cat > AppDir/AppRun << 'EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
BINDIR="${HERE}/usr/bin"

# Make bundled libraries visible to the dynamic linker
export LD_LIBRARY_PATH="${BINDIR}:${LD_LIBRARY_PATH}"
export PATH="${BINDIR}:${PATH}"

# Prefer Wayland when available; fall back to X11/XWayland automatically
# (Avalonia 11 auto-detects — no need to force either backend)

exec "${BINDIR}/Nintenlord UPS patcher.Avalonia" "$@"
EOF
chmod +x AppDir/AppRun

# Icon
cp .github/installers/nups.png AppDir/nups.png
cp .github/installers/nups.png AppDir/usr/share/icons/hicolor/256x256/apps/nups.png

# --- Download the type-2 runtime for the target architecture ---
RUNTIME_URL="https://github.com/AppImage/type2-runtime/releases/download/continuous/runtime-${ARCH}"
echo "Downloading type2-runtime for ${ARCH}..."
wget -q -O runtime "${RUNTIME_URL}"
chmod +x runtime

# --- Create squashfs from AppDir ---
echo "Creating squashfs..."
mksquashfs AppDir payload.squashfs -root-owned -noappend -comp zstd -quiet

# --- Concatenate runtime + squashfs = AppImage ---
echo "Assembling AppImage..."
cat runtime payload.squashfs > "${OUTPUT}"
chmod +x "${OUTPUT}"

# Verify the result
ls -lh "${OUTPUT}"
echo "Created AppImage: ${OUTPUT}"
