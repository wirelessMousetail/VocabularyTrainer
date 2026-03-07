#!/bin/bash
# Script to create a macOS .app bundle from the published application
# This script should be run from the publish directory
# Usage: ./create-macos-app.sh

APP_NAME="VocabularyTrainer"
APP_BUNDLE="${APP_NAME}.app"

echo "Creating macOS app bundle..."

# Check if we're in the publish directory (look for the executable)
if [ ! -f "$APP_NAME" ]; then
    echo "Error: ${APP_NAME} executable not found in current directory"
    echo "Please run this script from the publish directory:"
    echo "  cd bin/Release/net8.0/osx-x64/publish"
    echo "  ./create-macos-app.sh"
    exit 1
fi

# Create app bundle structure
echo "Creating bundle structure..."
mkdir -p "${APP_BUNDLE}/Contents/MacOS"
mkdir -p "${APP_BUNDLE}/Contents/Resources"

# Copy all files to MacOS directory (except the app bundle itself and this script)
echo "Copying application files..."
for item in *; do
    if [ "$item" != "$APP_BUNDLE" ] && [ "$item" != "create-macos-app.sh" ]; then
        cp -r "$item" "${APP_BUNDLE}/Contents/MacOS/"
    fi
done

# Copy icon to Resources (if exists)
if [ -f "${APP_BUNDLE}/Contents/MacOS/Resources/tray.ico" ]; then
    echo "Note: .ico format detected. For best results, convert to .icns format"
fi

# Create Info.plist
echo "Creating Info.plist..."
cat > "${APP_BUNDLE}/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>${APP_NAME}</string>
    <key>CFBundleIdentifier</key>
    <string>com.vocabularytrainer.app</string>
    <key>CFBundleName</key>
    <string>${APP_NAME}</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>LSUIElement</key>
    <false/>
</dict>
</plist>
EOF

# Make executable
chmod +x "${APP_BUNDLE}/Contents/MacOS/${APP_NAME}"

# Remove quarantine attribute if running on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Removing quarantine attributes..."
    xattr -cr "${APP_BUNDLE}"
fi

echo ""
echo "✅ macOS app bundle created successfully!"
echo "Location: $(pwd)/${APP_BUNDLE}"
echo ""
echo "To distribute:"
echo "  zip -r ${APP_NAME}.zip ${APP_BUNDLE}"
echo ""
echo "To test:"
echo "  open ${APP_BUNDLE}"
echo ""
echo "Users can extract and double-click ${APP_BUNDLE} to launch."
