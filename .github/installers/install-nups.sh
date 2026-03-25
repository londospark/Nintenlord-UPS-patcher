#!/bin/bash
# Nintenlord UPS Patcher - One-Click Installer
# Downloads latest release from GitHub and installs to ~/.local/share/nups
set -e

REPO="londospark/Nintenlord-UPS-patcher"
INSTALL_DIR="$HOME/.local/share/nups"
DESKTOP_FILE="$HOME/.local/share/applications/nups.desktop"

echo "========================================="
echo "Nintenlord UPS Patcher Installer"
echo "========================================="
echo ""

# Detect architecture
ARCH=$(uname -m)
if [ "$ARCH" = "x86_64" ]; then
    RUNTIME="linux-x64"
elif [ "$ARCH" = "aarch64" ]; then
    RUNTIME="linux-arm64"
else
    echo "ERROR: Unsupported architecture: $ARCH"
    echo "Supported: x86_64, aarch64"
    exit 1
fi

echo "Detected architecture: $ARCH ($RUNTIME)"
echo ""

# Check for required tools
if ! command -v curl >/dev/null 2>&1 && ! command -v wget >/dev/null 2>&1; then
    echo "ERROR: Neither curl nor wget found. Please install one of them."
    exit 1
fi

if ! command -v tar >/dev/null 2>&1; then
    echo "ERROR: tar not found. Please install tar."
    exit 1
fi

# Get latest release URL
echo "Fetching latest release..."
if command -v curl >/dev/null 2>&1; then
    DOWNLOAD_CMD="curl -sL"
    RELEASE_INFO=$(curl -sL "https://api.github.com/repos/$REPO/releases/latest")
else
    DOWNLOAD_CMD="wget -qO-"
    RELEASE_INFO=$(wget -qO- "https://api.github.com/repos/$REPO/releases/latest")
fi

# Extract download URL for our architecture
DOWNLOAD_URL=$(echo "$RELEASE_INFO" | grep "browser_download_url.*${RUNTIME}.tar.gz" | cut -d'"' -f4)

if [ -z "$DOWNLOAD_URL" ]; then
    echo "ERROR: Could not find download URL for $RUNTIME"
    echo "Please check https://github.com/$REPO/releases"
    exit 1
fi

echo "Found release: $DOWNLOAD_URL"
echo ""

# Create temp directory
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

# Download
echo "Downloading..."
cd "$TEMP_DIR"
$DOWNLOAD_CMD "$DOWNLOAD_URL" -o nups.tar.gz

# Extract
echo "Extracting..."
tar -xzf nups.tar.gz

# Remove old installation
if [ -d "$INSTALL_DIR" ]; then
    echo "Removing old installation..."
    rm -rf "$INSTALL_DIR"
fi

# Install
echo "Installing to $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"
cp -r ./* "$INSTALL_DIR/"

# Make executable
chmod +x "$INSTALL_DIR/Nintenlord UPS patcher.Avalonia"
if [ -f "$INSTALL_DIR/Nintenlord UPS patcher.Avalonia.sh" ]; then
    chmod +x "$INSTALL_DIR/Nintenlord UPS patcher.Avalonia.sh"
fi

# Convert icon if possible
if command -v convert >/dev/null 2>&1 && [ -f "$INSTALL_DIR/NUPS icon.ico" ]; then
    convert "$INSTALL_DIR/NUPS icon.ico[0]" "$INSTALL_DIR/nups.png" 2>/dev/null || \
    cp "$INSTALL_DIR/NUPS icon.ico" "$INSTALL_DIR/nups.png" 2>/dev/null || true
elif [ -f "$INSTALL_DIR/NUPS icon.ico" ]; then
    cp "$INSTALL_DIR/NUPS icon.ico" "$INSTALL_DIR/nups.png" 2>/dev/null || true
fi

# Create .desktop file
echo "Creating desktop entry..."
mkdir -p "$(dirname "$DESKTOP_FILE")"
cat > "$DESKTOP_FILE" << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=Nintenlord UPS Patcher
Comment=UPS patch file utility
Exec=$INSTALL_DIR/Nintenlord UPS patcher.Avalonia
Icon=$INSTALL_DIR/nups.png
Terminal=false
Categories=Utility;Game;
Keywords=patch;ups;rom;retro;
StartupNotify=true
EOF

# Update desktop database
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "$HOME/.local/share/applications" 2>/dev/null || true
fi

echo ""
echo "========================================="
echo "Installation complete!"
echo "========================================="
echo ""
echo "Nintenlord UPS Patcher installed to:"
echo "  $INSTALL_DIR"
echo ""
echo "Launch from:"
echo "  - Application menu (Utility/Games)"
echo "  - Game Mode (Non-Steam Games)"
echo ""
echo "To add to Steam:"
echo "  Games -> Add Non-Steam Game -> Browse"
echo "  Navigate to: $INSTALL_DIR"
echo "  Select: Nintenlord UPS patcher.Avalonia"
echo ""
echo "To uninstall:"
echo "  rm -rf $INSTALL_DIR"
echo "  rm $DESKTOP_FILE"
echo ""
