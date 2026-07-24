# System & Game Manager – Changelog (v0.5.8)

### 🛠 Architecture Refactoring & Code Cleanup
*   **Legacy View Removal:** Removed legacy view classes (`GameInfoView`, `GameAudioView`) to streamline the application flow and reduce bloat in `/view/MainForm.cs`. UI initialization logic was significantly simplified by commenting out unused controls and loading states.
*   **Configuration Migration:** Eliminated external `appsettings.json` configuration file. Application settings are now strictly defined via C# classes (`AppConfig`, `DatabaseConfig`) for better type safety and deployment consistency.
*   **Environment Handling:** Introduced new `/config/global-dev-config.cs` class to dynamically handle environment switching (e.g., forcing specific database paths in Debug builds) without requiring external config file changes.
*   **Author Attribution:** Unified author attribution from "Bommberk" to **"Krassheiten"** across configuration classes and release metadata.

### 🧹 Path Logic & Database Handling
*   **Path Management Centralization:** Added helper methods `GlobalFunctions.GetAppdataPath()` and `GetCurrentDirectory()` to standardize directory retrieval logic.
*   **Database Relocation:** Explicitly moved the SQLite database location from the installation directory (`modules/database/`) to `%APPDATA%\SystemGameManager\`. This ensures user privacy, prevents installation conflicts, and isolates game data in the standard Windows AppData directory.
*   **Template Syncing Robustness:** Refactored `/modules/Database/DatabaseController.cs` to utilize dynamic file paths derived from `GlobalConfig`, making template importing and schema synchronization more robust across different environments.

### 🧹 Utility & Startup Flow Optimization
*   **Logging Helper Simplification:** Renamed static logging method in `GlobalFunctions.cs` from verbose `ConsoleLog()` to lowercase `log()`. This improves consistency with C# naming conventions and cleans up CLI output handling (fixing startup messages like "Audio-Monitoring").
*   **Startup Flow Cleanup:** Removed commented-out code blocks regarding launcher badges and game card initialization, streamlining the entry point (`Program.cs`).
*   **Debug Mode Config:** Implemented conditional logic in `Program.Main` to instantiate new debug configuration handlers only when building/debugging.