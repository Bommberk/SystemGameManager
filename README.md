# System & Game Manager

System & Game Manager is a Windows desktop app for managing installed games, game launchers, audio settings, and hotkeys from a local WebView2 interface.

## Working Features

### 1) Detect games and launchers
- Finds supported launchers such as Steam, Epic Games Launcher, GOG Galaxy, Ubisoft Connect, EA App, Battle.net, Rockstar Games Launcher, Origin, Minecraft Launcher, and CurseForge.
- Uses Windows registry entries, known launcher paths, and Steam library folders to locate installed games.
- Combines duplicate game entries by installation path.

### 2) Manage the game library
- Browse, search, filter, select, and hide games.
- Save per-game music volume, game volume, audio output device, and custom game artwork in SQLite.
- Apply an audio configuration to multiple selected games.

### 3) Automate audio while gaming
- Detects the active game from the foreground window and its process path.
- Adjusts the configured music app volume and can switch the Windows default audio output device.
- Restores the previous music volume and output device when the game is no longer active.
- The game-volume setting is stored, but is not yet applied by the automatic monitor.

### 4) Configure hotkeys
- Create audio modes with music/game volume, output device, description, and a key combination.
- Use a hotkey globally or associate it with a game.

### 5) Set user preferences
- Change the app theme using the existing Dark, Light, Red, Pink, and Yellow settings.
- Enable background running, choose whether closing the window keeps the app in the system tray, and optionally start with Windows either minimized to the tray or fully open.
- Launching the app again opens the existing window, including when it is running in the system tray.
- Optionally save a user name. User preferences are stored locally in the SQLite database; login and registration are not implemented.

## Preview

There are currently no screenshots in `assets/screenshots`. Add screenshots there when they are available.

## How to use (User)

1. Open the [latest release](https://github.com/Bommberk/SystemGameManager/releases/latest).
2. Download and run `setup.exe`.
3. Install and launch System & Game Manager.
4. Review the detected library, select games, and save their audio settings.
5. Set a theme or optional name under **Settings**. Enable background running, then choose whether closing the window keeps the app in the system tray and whether it starts with Windows.

## How to use (Dev)

Requirements: Windows 10 or 11, the .NET 10 SDK, and the Microsoft Edge WebView2 Runtime.

```powershell
git clone https://github.com/Bommberk/SystemGameManager.git
cd SystemGameManager
dotnet restore
dotnet run
```

Open `SystemGameManager.sln` in Visual Studio or VS Code, or publish a Windows build with:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

## How it works

- On startup, the app checks known registry locations and launcher folders, reads Steam library folders when available, and combines matching game paths.
- Game and user settings are persisted in a local SQLite database. The user row has a generated UUID and optional name and theme.
- When background running and close-to-background are enabled, closing the window hides the app to its system-tray icon; optional Windows startup behavior is registered for the current user.
- While the GUI is open, the app checks the foreground process path to recognize a configured game and applies the supported music-volume and output-device settings.
- When no configured game is active, it restores the previous music volume and output device.
- The interface is an HTML/CSS/JavaScript app displayed in WebView2; its JavaScript API sends requests to the C# backend.

The database is normally stored at `%AppData%\SystemGameManager\systemgamemanager.db`. The schema template is in `modules/database/`.

## Coming next

These items are described in [programm-requirements.md](.github/programm-requirements.md) and are not all implemented yet:

### 1) More audio management
- Support audio settings for chat applications such as Discord and TeamSpeak.
- Add reusable audio profiles, such as music-focused and story-focused modes, with automatic changes while gaming.
- Expand shortcuts for switching profiles and output devices.

### 2) System information
- Add a view or plugin for PC and system information.

## Current limitations

- Login and registration, and dedicated music profiles, are not implemented.
- Games hidden from the library cannot currently be restored through the UI.
- The **Start** and **Open folder** menu actions are not implemented.
- Game-volume values are stored but are not yet applied by the automatic monitor.
- Launcher discovery may miss custom or non-standard installations.

## Technologies

- C# / .NET 10 / Windows Forms
- Microsoft WebView2
- SQLite (`Microsoft.Data.Sqlite`)
- NAudio
- Velopack

## License

No license file is currently included in this repository.
