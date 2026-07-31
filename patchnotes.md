# System & Game Manager – Changelog (v0.5.9)

### 🛠 Architecture Refactoring & Entry Point Logic
- **Main Execution Flow:** Modified `Program.Main` to execute asynchronous updates synchronously (`GetAwaiter().GetResult()`) within the UI thread, ensuring background checks complete before launching the application or console mode.
- **Environment Handling:** Implemented conditional logic in startup flow; system info loading is now skipped exclusively for production builds where applicable (via `AppConfig.Environment`), streamlining performance on initial launch.
- **Configuration Cleanup:** Removed reliance on external `appsettings.json`; all settings are now strictly defined via C# classes (`AppConfig`, `GlobalConfig`) with a new `SmarthomeApiConfig` class for smart home integration defaults.

### 🧪 Feature Additions & Interactivity
- **Smart Home Integration:** Added logic to fetch PC details (UUID) and send them to the configured Smart Home API on startup in non-dev environments. A dedicated HTTP client (`modules/Plugin/SmarthomeApi`) handles this communication with error handling via `ErrorHandler`.
- **Contextual Menus:** Introduced a three-button menu on game cards:
  - **Open Directory:** Launches File Explorer directly to the installed game's folder.
  - **Change Image:** Allows users to replace default wallpapers with custom images (`.png`, `.jpg`). Changes are persisted immediately via `Game.Update()`.
  - **Remove Game:** *(Temporarily disabled)* Previously added logic for removing games from the database UI is currently commented out but available if needed.

### 🎨 Visual & UX Improvements
- **Enhanced Theming Engine:** Extended the theme system with new color properties:
  - Added `QuaternaryBackgroundColor` (secondary actions) and `SecondaryCardBackgroundColor`.
  - Introduced dedicated Error background colors for alert elements.
- **Dynamic UI Components:** Refactored controls to utilize dynamic hover states via helper methods (`SetHoverColor`) instead of hardcoded values, supporting consistent dark/light modes.
- **Game Card Optimization:** Game cards now enforce fixed dimensions (300px width) with `Zoom` scaling for wallpapers rather than stretch distortion. Rounded corners applied globally using `UIHelpers.RoundPictureBox`.

### 🗄️ Database & Data Management
- **Generic Update Mechanism:** New generic `DatabaseService.UpdateRecordByName()` method added, enabling dynamic updates of any record type (e.g., `Game`, `PC`) by name without hardcoding field lists during instantiation.
- **Path Centralization:** Added dedicated methods (`GetAppdataPath`, etc.) to standardize path retrieval, ensuring the SQLite database resides safely within `%APPDATA%\SystemGameManager\` rather than the installation directory.

### 🧹 Code Hygiene & Utilities
- **Logging Standards:** Renamed verbose static logging method from `ConsoleLog()` to lowercase `log()` in helper functions for cleaner CLI output and consistency with C# naming conventions.
- **Startup Logic Simplification:** Removed commented-out code blocks regarding launcher badges, streamlining the entry point (`Program.cs`) and reducing confusion around initialization flows.
- **Path Handling Refinement:** Centralized app data path retrieval logic to ensure consistent behavior across development and production builds.