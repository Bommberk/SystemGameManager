# System & Game Manager – Changelog

### 🎯 User Experience Enhancements
- **Background & Startup Options:** Users can now enable background running, configure the app to minimize to the system tray on close, and choose between starting minimized or fully open at Windows startup.
- **Wallpaper Customization:** Added support for setting a custom default wallpaper (applied when no game is running) via an Open File Dialog.
- **Lively Wallpaper Integration:**
  - Automatic detection of `livelycu.exe` via Registry, known folders, and system PATH.
  - Manual selection fallback if automatic detection fails.
  - Enhanced handling for multi-monitor setups with specific wallpaper paths per game.
- **User Identity:** Introduced optional username saving in local settings to personalize the app instance.

### 🛠 Web API & Communication Updates
- **New Actions Added:** Extended the C# backend (`WebApiHandler`) and JavaScript frontend API with the following actions:
  - `getUserSettings`: Retrieves current user configuration.
  - `setUserName`, `setUserTheme`: Manage personalization.
  - `setUserBackgroundSettings` / `getDetectedWallpaperInfo`: Handle startup behavior and wallpaper paths.
  - `selectDefaultWallpaper` / `selectLivelyExe`: File picker integration for assets.
- **Data Payloads:** Defined new request/response structures (`UserSettingsRequest`, `UserWallpaperSettingsRequest`, etc.) to support the expanded API surface.

### 💾 Database & Persistence Refactoring
- **Generic Record Management:** Restructured `DatabaseService` to support configurable primary keys via `[Key]` attributes (defaulting to `Name` or `UserId`).
- **Dynamic Schema Handling:** Updated database creation logic to respect nullable properties and key columns automatically.
- **UUID Support:** Enhanced serialization/deserialization to correctly handle `Guid` types in the database schema.

### 🔌 Plugin Architecture Improvements
- **Lively Wallpaper Module Split:** Refactored the single-file plugin into a modular structure:
  - `LivelyWallpaper`: Core logic for applying wallpapers and managing state.
  - `LivelyLocator`: Handles discovery of `livelycu.exe` using Registry, PATH, and standard install locations.
  - `WallpaperCropper`: New utility to crop/warp images for multi-monitor scaling or aspect ratio correction before sending to Lively.
  - `WindowsWallpaperService`: Utility to read the current Windows desktop background via API calls.

### 🚀 Startup & Application Lifecycle
- **Startup Form:** Implemented a minimal startup window that checks for updates, prepares the database, and initializes plugins before launching the main GUI or tray icon.
- **Headless Mode:** Refined console arguments (`--console`, `--background`) to cleanly separate background execution from UI rendering tasks.

### 🐛 Fixes & Stability
- **Error Handling:** Improved exception catching in wallpaper switching and startup routines, logging issues without crashing the app.
- **Tray Icon Logic:** Added logic to toggle tray visibility based on user preferences (`EnableBackgroundRunning`), ensuring the app behaves correctly whether running in foreground or background.