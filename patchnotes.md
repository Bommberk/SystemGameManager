# System & Game Manager – Changelog (v0.5.7)

### 🏗 Architecture Refactoring & Code Cleanup
*   **UI Module Restructuring:** Removed legacy view classes (`GameInfoView`, `GameAudioView`) from the codebase, simplifying the main application flow in `/view/MainForm.cs`.
*   **Config Migration:** Eliminated external `appsettings.json` configuration file. Application settings are now strictly defined via C# classes and hardcoded defaults for better type safety.
    *   Updated versioning logic (`AppConfig.Version`) to `0.5.7`.
    *   Unified author attribution under "Krassheiten".
*   **Dev Environment:** Introduced new `/config/global-dev-config.cs` class to handle dynamic environment switching (e.g., forcing database paths in Debug builds) without configuration file changes.

### 💾 Database & File Handling
*   **Path Logic Update:** Centralized app data path retrieval via `GlobalFunctions.GetAppdataPath()`.
    *   The SQLite database is now explicitly located in `%APPDATA%\SystemGameManager\` instead of the install directory, ensuring better portability and user isolation.
*   **Database Initialization:** Refactored `/modules/Database/DatabaseController.cs` to use dynamic file paths derived from `GlobalConfig`, making template syncing logic more robust across environments.

### 🧹 Maintenance & Optimization
*   **Log Helper Simplification:** Renamed static logging method in `GlobalFunctions.cs` from ConsoleLog() to lowercase `log()` for consistency with C# naming conventions and cleaner CLI output handling (including startup messages like "Audio-Monitoring").
*   **Startup Flow Cleanup:** Removed commented-out code blocks regarding launcher badges and game cards initialization, streamlining the entry point (`Program.cs`).