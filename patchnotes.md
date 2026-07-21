# System & Game Manager – Changelog

### 📦 Data Handling & Storage Security
- **Database Relocation:** Moved the SQLite database location from a hardcoded `modules/database/` path within the application to `%APPDATA%\SystemGameManager\`. 
  - This improves user privacy and prevents installation conflicts by isolating game data in the standard Windows AppData directory.
  - Updated logic now checks for existing user-specific databases before falling back to template imports.

### 🔧 Path Logic Updates
- **Configuration Adjustments:** Modified `DatabaseController` to resolve paths via `Environment.GetFolderPath`, ensuring compatibility across different system configurations and installation directories.