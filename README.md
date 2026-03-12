# VocabularyTrainer

A Windows desktop application for learning vocabulary through periodic quizzes. The app shows quizzes at configurable intervals and automatically increases the frequency of words you get wrong, helping you focus on difficult vocabulary. Was created for learning Dutch, but may work with any language pair if you replace the word list CSV file with your own. Be aware: word group auto recognition is Dutch-specific.

## Features

### Core Learning Features
- **Intelligent Weight System**: Words you get wrong gain weight and appear more frequently; words you consistently answer correctly appear less often
- **Spaced Repetition**: Quizzes are being shown with regular intervals, you can adjust it for your needs and style
- **Three Quiz Modes**:
  - **Direct (Dutch → English)**: Translate Dutch words to English
  - **Reverse (English → Dutch)**: Translate English words to Dutch
  - **Random**: Randomly alternate between both directions for varied practice
- **Word Grouping**: Automatic categorization of words into Nouns, Verbs, and Other. Quiz options are preferentially selected from the same group for more realistic practice. *Note: This feature is specific to Dutch and uses simple Dutch-specific heuristic rules for word recognition.*
- **Multiple Choice Quizzes**: Configurable number of answer options (2-10)

### User Experience
- **Persistent Progress**: Your learning progress, word weights, and streaks are saved automatically
- **Managed Word List**: Maintains a personal vocabulary list that merges with precompiled words on startup
- **Flexible Configuration**: Customize quiz interval, auto-close timing, number of options and direction of translation  
- **System Tray Integration**: Runs quietly in the background with easy access to pause, resume, and configure

### Technical Features
- **Lightweight**: Minimal resource usage, perfect for running continuously
- **Offline Operation**: No internet connection required
- **CSV-based Storage**: Simple, portable data format for easy backup and transfer

## System Requirements

- **Operating System**: Windows 10/11, macOS 10.15+, or Linux
- **.NET Runtime**: .NET 8.0 or later (not required for self-contained releases)

## Installation

### Pre-built Releases
Download the appropriate release package for your platform from the [Releases](https://github.com/wirelessMousetail/VocabularyTrainer/releases) page.

**Self-contained releases** include the .NET runtime and don't require any additional installation.

### Building from Source
1. Ensure you have [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
2. Clone the repository:
   ```bash
   git clone https://github.com/wirelessMousetail/VocabularyTrainer.git
   cd VocabularyTrainer
   ```
3. Build the project:
   ```bash
   dotnet build -c Release
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Creating Release Builds

### Self-Contained Releases (Recommended)
These include the .NET runtime and work without requiring .NET installation on the target machine.

**Windows (x64):**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
Output: `bin/Release/net8.0/win-x64/publish/`

**macOS (Apple Silicon - M1/M2/M3):**
```bash
# Build the release (on any platform)
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true

# Create .app bundle (run on Mac after transferring the publish folder)
cd bin/Release/net8.0/osx-arm64/publish
chmod +x create-macos-app.sh
./create-macos-app.sh
```
Output: `bin/Release/net8.0/osx-arm64/publish/VocabularyTrainer.app`

**macOS (Intel):**
```bash
# Build the release (on any platform)
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true

# Create .app bundle (run on Mac after transferring the publish folder)
cd bin/Release/net8.0/osx-x64/publish
chmod +x create-macos-app.sh
./create-macos-app.sh
```
Output: `bin/Release/net8.0/osx-x64/publish/VocabularyTrainer.app`

**Linux (x64):**
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```
Output: `bin/Release/net8.0/linux-x64/publish/`

### Distribution Package Contents

The publish folder contains all necessary files. To create a distribution package:

**Required files:**
- Main executable (`VocabularyTrainer.exe` / `VocabularyTrainer`)
- `Data/` folder with `words.csv`
- `appsettings.json`
- Native libraries:
  - `av_libglesv2.dll` (Windows) / `.dylib` (macOS) / `.so` (Linux)
  - `libHarfBuzzSharp.dll` / `.dylib` / `.so`
  - `libSkiaSharp.dll` / `.dylib` / `.so`

**Optional (can be excluded):**
- `VocabularyTrainer.pdb` - Debug symbols (~20-50MB, only needed for debugging)
- `appdata.csv` - Will be auto-created on first run

**Package size:** ~60-80 MB (self-contained, without .pdb)

### Framework-Dependent Releases (Smaller Size)
If users already have .NET 8 installed, you can create smaller releases:

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

**Package size:** ~1-2 MB (requires .NET 8 Runtime on target machine)

### Platform-Specific Notes

**macOS:**
- The app is not signed with an Apple Developer certificate, so macOS will block it by default
- **Creating .app bundle:** Navigate to the publish folder and run:
  ```bash
  cd bin/Release/net8.0/osx-x64/publish

  # Make script executable and run
  chmod +x create-macos-app.sh
  ./create-macos-app.sh
  ```
- **Running .app bundles:** Right-click `VocabularyTrainer.app` → "Open" → Click "Open" in the dialog
  - Or: Remove quarantine: `xattr -cr VocabularyTrainer.app` then double-click to launch
- **For standalone executable:** After extracting the archive, open Terminal and run:
  ```bash
  # Remove quarantine attribute from all files
  xattr -cr .

  # Make the executable runnable
  chmod +x VocabularyTrainer

  # Run the app
  ./VocabularyTrainer
  ```
- Alternative: System Settings → Privacy & Security → "Open Anyway" (appears after first launch attempt)
- Note: The .app bundle prevents the Terminal window from appearing when launching from Finder

**Linux:**
- Make the binary executable: `chmod +x VocabularyTrainer`
- System tray support varies by desktop environment (confirmed working on Ubuntu/GNOME)

## Quick Start

### First Launch
1. **Initial Quiz**: The application starts immediately with your first quiz
2. **System Tray Icon**: Look for the VocabularyTrainer icon in your system tray (notification area)
3. **Answer the Quiz**: Click on one of the multiple-choice options
   - ✓ Correct answers close automatically after 5 seconds (default)
   - ✗ Wrong answers let you try again

### Configuring Settings
1. Right-click the system tray icon
2. Select **"Options"**
3. Configure your preferences:
   - **Quiz Interval**: Time between quizzes (in seconds)
   - **Auto-close Delay**: How long to wait before closing correct answers (in seconds)
   - **Number of Options**: How many answer choices to display (2-10)
   - **Quiz Direction**: Choose Direct, Reverse, or Random mode

### Managing Your Word List
Your vocabulary is stored in two locations:
- **Data/words.csv**: Your vocabulary source file. Edit this file to add or replace words. This is the intended place for managing your word list. Note: removing a word from this list won't remove it from the app word list (see appdata.csv)
- **appdata.csv**: Your managed list with progress tracking (in the application directory). This file is automatically maintained by the app. Do not edit unless you know what you're doing.

On startup, the application automatically merges new words from `Data/words.csv` into your managed list, preserving your progress data.

### Pausing and Resuming
- **Pause**: Right-click tray icon → "Pause" (stops new quizzes)
- **Resume**: Right-click tray icon → "Resume" (restarts quiz timer)

### Exiting
Right-click the tray icon and select **"Exit"** to close the application.

## Understanding the Weight System

The VocabularyTrainer uses a weight-based algorithm to optimize your learning:

### How Weights Work
- **Initial Weight**: All words start with weight 0
- **On Mistake**: Weight increases exponentially: `weight = (weight × 3) + 1` (capped at 100)
- **On Correct Answer**:
  - First 5 correct in a row: Weight decreases linearly by 1
  - After 5 correct: Weight decreases exponentially: `weight = weight × 0.5`
- **Selection Probability**: Each word gets `1 + weight` tickets in the selection pool
  - Weight 0 word: 1 ticket (base probability)
  - Weight 10 word: 11 tickets (11× more likely to appear)
  - Weight 100 word: 101 tickets (101× more likely to appear)

### Example Learning Path
```
Word: "aandacht" (attention)
1. First quiz - Wrong answer → Weight: 1, Streak: 0
2. Second quiz - Wrong answer → Weight: 2, Streak: 0
3. Third quiz - Correct answer → Weight: 1, Streak: 1
4. Fourth quiz - Correct answer → Weight: 0, Streak: 2
5. Weight 0 reached → Word appears at base frequency
```

## Word List Format

### Precompiled Words (Data/words.csv)
Simple two-column format:
```
question;answer
aandacht;attention
afleidingen;distractions
begeleiding;guidance
```

### Managed Words (appdata.csv)
*Note: This file is automatically maintained by the application and is not intended for manual editing.*

Includes progress tracking:
```
question;answer;weight;streak;group
aandacht;attention;5;0;Noun
afleidingen;distractions;0;3;Noun
begeleiding;guidance;12;0;Noun
```

**Fields:**
- **question**: The Dutch word or phrase
- **answer**: The English translation
- **weight**: Current word weight (0-100)
- **streak**: Number of consecutive correct answers
- **group**: Word category (Noun, Verb, Other)

## Data Backup

If you want to backup/copy your progress and options, you need to save these two files from the runtime directory:
- `appdata.csv` (your progress and managed word list)
- `appsettings.json` (your preferences)

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
