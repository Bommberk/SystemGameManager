# System & Game Manager

System & Game Manager is a Windows desktop app (WinForms, .NET 8) that combines system monitoring, game library discovery, and automatic game/music audio handling.

It is built to aggregate data from multiple sources (Windows registry, launcher data files, and folder scans), store synced results in SQLite, and provide a simple UI for daily use.

## Working Features

### 1) System Information Dashboard
- Shows core PC data in one place:
- Computer name, local/public IPs, MAC address, OS, CPU, RAM, GPU, battery status.
- Storage overview per drive, including handling for virtual disk scenarios.

### 2) Launcher Detection
- Detects installed launchers based on known launcher definitions.
- Uses Windows uninstall registry keys and launcher-specific registry paths.
- Resolves launcher install paths and library folders automatically where possible.

### 3) Installed Games Discovery
- Collects games from launcher game folders.
- Reads game entries from launcher-related registry keys.
- Merges duplicate results and normalizes the final game list.

### 4) SQLite Data Sync
- Persists launcher and game data into a local SQLite database.
- Reuses saved volume settings for games when data is refreshed.
- Keeps data consistent between discovery runs and UI state.

### 5) Game Manager UI
- Shows detected launchers and installed games in a card-based layout.
- Provides quick access to open a game installation directory.
- Includes loading and error states for better usability.

### 6) Audio Manager (Global + Per Game)
- Lets you define global game/music volume values.
- Lets you define individual game/music values per detected game.
- Saves audio profiles persistently.

### 7) Automatic Audio Adjustment During Gameplay
- Monitors the active foreground process and matches it against known games.
- Lowers/sets music volume when a configured game is active.
- Restores previous music volume when no monitored game is running.

### 8) Console and GUI Modes
- GUI mode starts by default.
- Console mode can be started with:
- `SystemGameManager.exe --console`

### 9) Windows Integration
- Native Windows integration via registry access and process/window APIs.
- Designed for Windows systems (`net8.0-windows`).

## Preview

### App Artwork
![Artwork](assets/bild.jpg)

### Screenshots
- Add your current UI screenshots to `assets/screenshots/` and reference them here.
- Suggested file names:
- `assets/screenshots/system-manager.png`
- `assets/screenshots/game-manager.png`
- `assets/screenshots/audio-manager.png`

## How to Use (User)

1. Go to the latest release of this repository.
2. Download the setup file (`systemgamemanager_setup.exe`).
3. Run the installer.
4. Launch System & Game Manager.
5. Click the load button to fetch system/game data.
6. Configure audio values in the Game Audio Manager tab.

## How to Use (Dev)

1. Clone the repository:
	`git clone https://github.com/Bommberk/SystemGameManager`
2. Open the solution file in Visual Studio or VS Code:
	`SystemGameManager.sln`
3. Restore dependencies:
	`dotnet restore`
4. Run in Debug:
	`dotnet run`
5. Optional: run console mode for audio monitoring:
	`dotnet run -- --console`
6. Build Release output:
	`dotnet publish -c Release -r win-x64 --self-contained false`
7. Optional: build installer using Inno Setup with `installer.iss`.

## How It Works

1. The app starts in GUI mode (or console mode with `--console`).
2. Launcher definitions are loaded from JSON assets.
3. Installed launchers are detected via Windows registry.
4. Library and install paths are resolved for each launcher.
5. Installed games are discovered from folders and registry entries.
6. Data is merged and synchronized with SQLite.
7. The UI renders system info, launcher badges, game cards, and audio controls.
8. Audio monitoring checks the foreground process in intervals.
9. When a configured game is active, music volume is adjusted automatically.
10. When the game is no longer active, previous music volume is restored.

## Roadmap / Coming Next

### 1) Better Detection Quality
- Improve edge-case handling for launcher and game path detection.
- Expand known launcher definitions and fallback strategies.

### 2) More Flexible Audio Profiles
- Add profile presets and smarter auto-rules for different game types.
- Extend support for additional music apps.

### 3) Expanded Integrations
- Improve external API integration workflows (for example Smart Home use cases).
- Add cleaner export/sync options for external systems.

### 4) UX and Setup Improvements
- More in-app guidance for first launch.
- More release packaging convenience and installer improvements.