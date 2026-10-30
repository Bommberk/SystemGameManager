# Changelog – vLatest

### 🔧 Game Audio Monitoring Logic
- **Hotkey Priority Override:** Introduced logic in `GameAudioMonitoringService` to give high priority to active hotkeys. When a hotkey is toggled on, the system no longer automatically adjusts audio monitoring settings based on game detection, ensuring continuous monitoring even when switching windows or starting new games.

### 🛠 Developer API
- **New Public Property:** Added `HotKeyListener.IsHotKeyActive` property to `HotKeyListener`. This boolean indicates whether a hotkey is currently active (toggled on), allowing external services to check the state before attempting audio configuration changes.
- **State Management:** Refactored internal state tracking to explicitly separate "baseline audio snapshots" from the current "active hotkey" session for cleaner lifecycle management.