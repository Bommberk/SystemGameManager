# SystemGameManager – v0.6.8 Patchnotes

## 🎉 New Features: HotKey Plugin
**The biggest update this version brings is a fully functional HotKey system**, allowing you to trigger audio changes via physical key presses.

- **New HotKey Module:** Introduced `modules/Plugin/HotKeys/`. This plugin uses low-level Windows hooks (`WH_KEYBOARD_LL`) and Raw Input API to detect keystrokes (including macro keys and HID media keys).
- **Web API Integration:** The internal WebView now exposes new endpoints: `getHotKeys`, `saveHotKey`, `deleteHotKey`, and a "Capture" mode.
- **UI Expansion:** A dedicated **"HotKeys"** section has been added to the Settings view (`settings.html`). Users can now create profiles with custom names, descriptions, volume limits, and output device assignments.
- **Smart Activation:** Hotkeys apply settings immediately upon press. Pressing an active hotkey again restores previous system states (including which game was running).
- **Global vs. Specific:** Hotkeys can apply changes globally or only when a specific game is running (via process name matching).

## 📡 Architecture & Infrastructure
**Improved release workflow and startup logic.**

- **Release Asset Updates:** Updated `releases/releases.win.json` with new checksums for version 0.6.7. The Full package size increased (~67MB vs ~60MB) due to the inclusion of the new plugin DLL and UI assets.
- **Synchronization Context:** Refactored `Program.cs` to explicitly set a `WindowsFormsSynchronizationContext`. This ensures that the low-level keyboard hook and raw input window can safely communicate with the WebView2 bridge on the main UI thread without threading issues.
- **Cleanup:** Removed unused `shutdownSignal` logic in favor of direct `Application.Exit()` for cleaner shutdown handling.

## 🔊 Audio Improvements
**Better logging and device handling.**

- **Verbose Logging:** Enhanced `SystemAudioService.cs` to log actions (e.g., "Setze Musiklautstärke auf...") with a flag to prevent spamming logs during rapid state changes.
- **Robust Device Enumeration:** Added fallback logic in audio device switching. If the friendly name of an audio device cannot be retrieved, the system gracefully falls back to using the Device ID instead of crashing or failing silently.

## 💻 UI & Frontend Refactor
**Polishing the user experience.**

- **State Management:** JavaScript state (`games`, `launchers`, `hotKeys`) is now properly initialized and handled via a centralized router, ensuring lists refresh correctly after API updates.
- **UX Enhancements:** Improved the game list selection logic to visually reflect "All Selected/Deselected" states in the checkbox menu. Added specific handling for "Capture" mode inputs in the HotKey modal.
- **CSS Styling:** Redesigned the HotKey table (`settings.css`) with rounded corners, proper spacing, and ellipsis truncation for long descriptions.

## 🔧 Under-the-Hood Changes
**Internal cleanups and refactors.**

- **Namespace Cleanups:** Continued consolidation of static usings and module structures.
- **Data Integrity:** Fixed `Game.cs` to persist volume settings directly onto installed game entities when updating, preventing data loss on hotkeys associated with known titles.
- **Error Handling:** Added robust checks in `WebApiHandler` and `HotKeyListener` to handle null data or failed hook registrations gracefully (logging failures via `mlog`).