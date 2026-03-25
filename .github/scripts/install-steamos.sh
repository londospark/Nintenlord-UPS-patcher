#!/bin/bash
# Install Nintenlord UPS Patcher on SteamOS/Linux
set -e

INSTALL_DIR="$HOME/.local/share/nups"
DESKTOP_FILE="$HOME/.local/share/applications/nups.desktop"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

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
    exit 1
fi

echo "Detected architecture: $ARCH ($RUNTIME)"
echo "Install directory: $INSTALL_DIR"
echo ""

# Create install directory
mkdir -p "$INSTALL_DIR"
mkdir -p "$(dirname "$DESKTOP_FILE")"

# Copy files
echo "Installing files..."
if [ -f "$SCRIPT_DIR/Nintenlord UPS patcher.Avalonia" ]; then
    # Installing from extracted directory
    cp -r "$SCRIPT_DIR"/* "$INSTALL_DIR/"
else
    echo "ERROR: Cannot find application files"
    echo "Please run this script from the extracted archive directory"
    exit 1
fi

# Make executable
chmod +x "$INSTALL_DIR/Nintenlord UPS patcher.Avalonia"
if [ -f "$INSTALL_DIR/Nintenlord UPS patcher.Avalonia.sh" ]; then
    chmod +x "$INSTALL_DIR/Nintenlord UPS patcher.Avalonia.sh"
fi

# Convert icon (if ImageMagick is available)
if command -v convert >/dev/null 2>&1; then
    if [ -f "$INSTALL_DIR/NUPS icon.ico" ]; then
        echo "Converting icon..."
        convert "$INSTALL_DIR/NUPS icon.ico[0]" "$INSTALL_DIR/nups.png" 2>/dev/null || true
    fi
fi

# Create .desktop file
echo "Creating desktop entry..."
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

# Update desktop database (if available)
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "$HOME/.local/share/applications" 2>/dev/null || true
fi

echo ""
echo "========================================="
echo "Installation complete!"
echo "========================================="
echo ""
echo "Nintenlord UPS Patcher has been installed to:"
echo "  $INSTALL_DIR"
echo ""
echo "You can now launch it from:"
echo "  - Application menu (Utility category)"
echo "  - Game Mode (Non-Steam Games)"
echo ""
echo "To add to Steam:"
echo "  1. Switch to Desktop Mode"
echo "  2. Open Steam"
echo "  3. Games -> Add a Non-Steam Game"
echo "  4. Browse to: $INSTALL_DIR"
echo "  5. Select 'Nintenlord UPS patcher.Avalonia'"
echo ""
echo "To uninstall, run:"
echo "  rm -rf $INSTALL_DIR"
echo "  rm $DESKTOP_FILE"
echo ""
