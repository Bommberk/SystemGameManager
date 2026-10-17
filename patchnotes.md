# System & Game Manager – Changelog

### 🛠 Architecture & Configuration
- **Git Ignore Updates:** Added `.VSCodeCounter` and `capture.wav` to `.gitignore` to prevent committing development artifacts and temporary audio files.
- **Framework Refinement:** Updated target framework configuration in `SystemGameManager.csproj` (`net10.0-windows` → `net10.0-windows10.0.26100.0`) for improved compatibility on the latest Windows builds.

### 🎮 Game Library Expansion (Minecraft Support)
- **Multi-Platform Detection:** Added comprehensive support for all Minecraft variants:
  - **Java Edition:** Detects default installation, profile-specific versions, and custom launcher profiles (via `launcher_profiles.json`).
  - **Bedrock Editions:** Scans Microsoft Store packages for Bedrock, Dungeons, Legends, and Education.
- **Process Heuristics:** Implemented smart detection for Minecraft running under `javaw.exe`. The system now parses the command line to:
  - Identify specific game folders from arguments (`--gameDir`, paths).
  - Distinguish between standard launches and third-party clients (Lunar, Badlion, Feather) based on argument markers.
- **JSON Parsing:** Integrated `System.Text.Json` to robustly parse Minecraft launcher profile configurations.

### 🔊 Audio Management Improvements
- **Volume Logic Overhaul:** Refactored volume detection in `SystemAudioService`:
  - Distinguishes between active audio sessions and the highest available volume across all instances of a specific app.
  - Prevents stale volume readings by prioritizing `AudioSessionStateActive`.
- **Robust Return:** Ensures a fallback value (`highestVolume`) is returned if no active session is found, preventing null returns for monitoring purposes.

### 🧹 Code Hygiene & Maintenance
- **Debug Noise Reduction:** Removed verbose debug logging (`mlog("asdf..."`) from `LivelyWallpaper.cs`.
- **Dependency Cleanup:** Updated namespace imports in audio and game modules to remove unused references (e.g., specific `NAudio` interfaces) while keeping essential dependencies.