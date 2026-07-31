# System & Game Manager – Changelog (v0.5.9)

### 🛠 Architecture Refactoring & Entry Point Logic
- **Main Execution Flow:** Modified `Program.Main` to execute asynchronous updates synchronously (`GetAwaiter().GetResult()`) within the UI thread, ensuring background checks complete before launching the application or console mode.
- **Environment Handling:** Implemented conditional logic in startup flow; system info loading is skipped exclusively for production builds where applicable, streamlining performance on initial launch.
- **Configuration Cleanup:** Removed reliance on external `appsettings.json`; all settings are now strictly defined via C# classes (`AppConfig`, `GlobalConfig`) for better type safety and deployment consistency.

### 🎨 Visual & UX Improvements
- **Enhanced Theming Engine:** Extended the theme system with new color properties:
  - Added support for `QuaternaryBackgroundColor` (secondary actions) in both Light/Dark modes.
  - Introduced `SecondaryCardBackgroundColor` to differentiate content areas from background layers.
- **Dynamic UI Components:** Refactored controls (`NormalButton`, `PictureBox`) to utilize dynamic hover states and rounded corners via helper methods instead of hardcoded values.
- **Game Card Optimization:** Game cards now enforce fixed dimensions for consistent layout, with wallpapers using zoom scaling rather than stretch distortion.
- **New Icons:** Integrated the vertical ellipsis icon into the project assets for menu triggers.

### 🧩 Feature Additions & Interactivity
- **Contextual Menus:** Added three-button context menus to game cards:
  - **Open Directory:** Launches File Explorer directly to the installed game's folder.
  - **Change Image:** Allows users to replace default wallpapers with custom images (`.png`, `.jpg`).
  - **Remove Game:** *(Temporarily disabled)* Previously added logic for removing games from the database UI.
- **Rounded Corners Engine:** Introduced `UIHelpers.RoundPictureBox` to apply consistent rounded regions to image displays based on corner radii parameters.

### 🔧 Database & Data Management
- **Update Mechanism:** New generic `DatabaseService.UpdateRecordByName()` method added, enabling dynamic updates of any record type by name without hardcoding field lists during instantiation.
- **Game Synchronization:** Refactored `Game.InstalledGames` to support immediate database writes via the new update service when image changes occur.

### 🧹 Code Hygiene & Utilities
- **Logging Standards:** Renamed verbose static logging method from `ConsoleLog()` to lowercase `log()` in helper functions for cleaner CLI output and consistency with C# naming conventions.
- **Path Centralization:** Added dedicated methods (`GetAppdataPath`, `GetCurrentDirectory`) to standardize path retrieval, ensuring the SQLite database resides safely within `%APPDATA%\SystemGameManager\` rather than the installation directory.
- **View Service Updates:** Decoupled version reporting from assembly reflection; now reads directly from configuration settings for easier patching and version bumping.