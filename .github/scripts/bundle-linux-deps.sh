#!/bin/bash
# Bundle Linux dependencies for SteamOS compatibility
set -e

PUBLISH_DIR="$1"
if [ -z "$PUBLISH_DIR" ]; then
  echo "Usage: $0 <publish_dir>"
  exit 1
fi

cd "$PUBLISH_DIR"

# Create a wrapper script that sets LD_LIBRARY_PATH
cat > "Nintenlord UPS patcher.Avalonia.sh" << 'EOF'
#!/bin/bash
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export LD_LIBRARY_PATH="$DIR:$LD_LIBRARY_PATH"
exec "$DIR/Nintenlord UPS patcher.Avalonia" "$@"
EOF
chmod +x "Nintenlord UPS patcher.Avalonia.sh"

# Copy required shared libraries that might not be on SteamOS
mkdir -p lib

# Find and copy dependencies (excluding system-critical ones)
for lib in $(ldd "Nintenlord UPS patcher.Avalonia" | grep "=> /" | awk '{print $3}' | grep -v "^/lib64" | grep -v "^/lib/x86_64"); do
  if [ -f "$lib" ]; then
    cp "$lib" . 2>/dev/null || true
  fi
done

echo "✓ Bundled Linux dependencies for SteamOS"
