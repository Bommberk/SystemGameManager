# System & Game Manager – Changelog

### 🎙️ Audio Monitoring & Speech Detection
- **New Module:** Introduced `AudioManager` with dedicated services for system loopback capture and real-time audio monitoring.
- **Speech Recognition:** Integrated Picovoice Cobra (via `.Cobra` package) to detect voice activity in running games using VAD.
  - Requires a valid Access Key from Picovoice Console; gracefully deactivates if missing.
  - Processes audio frame-by-frame (~32ms) for immediate detection with configurable hangover timers.
- **Dynamic Activation:** Monitoring now starts/stops automatically based on the currently active game process name.

### 🖼️ Lively Wallpaper Integration
- **New Plugin:** Added `LivelyWallpaper` support to dynamically set wallpapers based on running games.
- **Smart Fallback:** Restores a default MSI wallpaper when no game image is specified or the game changes.
- **Multi-Monitor Support:** Automatically detects and applies the correct wallpaper to the monitor with the highest coordinates via a new screen selection modal.

### 🛠 Architecture Refactoring
- **Namespace Consolidation:** Moved `SystemAudioService` from `/modules/game/` to the new `/modules/AudioManager/`.
- **Code Extraction:** Separated foreground game detection logic into a reusable utility class (`GetGameProcess`) and consolidated it into `GameAudioMonitoringService`.
- **Dependency Updates:** Added `.Cobra` (v3.0.3) and fixed missing global usings for `AudioManager.Controller`.

### ⚙️ Configuration & Web API
- **New Config Section:** Added `SpeechDetectionConfig` to `global-config.cs` for managing the Picovoice Access Key.
- **Web API Extensions:** Enhanced `WebApiHandler` with new endpoints:
  - `getScreens`: Retrieves available display outputs.
  - `setScreenForGame`: Allows frontend selection of a specific monitor for a game's wallpaper.
  - `getScreenForGame`: Returns the currently assigned monitor for a specific game.

### 🧹 Code Hygiene & Fixes
- **CSS Enhancements:** Added `.modal` base styles and specific styling for screen selector UI (`.wallpaper-screens`, `.screen`).
- **Style Updates:** Unified `--accent-color` across Light/Dark modes and improved game image aspect ratios (`16/9`).
- **Cleanup:** Removed unused legacy logic (e.g., hardcoded foreground process detection) in favor of dynamic `GetGameProcess` utilities.