# System & Game Manager – Changelog (v2.0 Audio Filtering Update)

### 🔊 New Audio Device Management
- **Feature Implementation:** Added support for dynamically detecting and populating system audio output devices via `SystemAudioService`.
- **UI Enhancement:** Integrated a search bar into the "Game Manager" audio settings page to filter games by:
  - Name
  - Install Path
  - Assigned Audio Output Device

### 📜 API & Service Expansion (`Handler/WebApiHandler.cs`, `modules/game/Service/SystemAudioService.cs`)
- **New Endpoint:** Introduced `getAudioDevices` action in the Webview2 handler.
- **Refactoring:** Converted internal methods like `GetAudioOutputDeviceNames()` to `static` for improved testability and reusability across modules.

### 🎨 Frontend Logic Updates (`view2.0/assets/script/app.js`, `gamemanager.html`)
- **Search Functionality:** Implemented real-time filtering logic in JavaScript to sort the game list based on user input within dropdown contexts (Name, Path, or Audio Device).
- **Device Population:** Automated the populating of the `<select id="audioOutputDevice">` element with system-detected device names.

### 🛠 View Layer Refactoring (`view2.0/MainForm.cs`, `GameManager.cs`)
- **Controller Injection:** Initialized a dedicated `GameAudioController` instance within `MainForm`. Resources are now properly disposed of via the `FormClosed` event to prevent memory leaks in debug builds.
- **Code Cleanup:** Commented out legacy direct service calls in `GameManager.cs` as logic has migrated toward the controller pattern and new filtering utilities.