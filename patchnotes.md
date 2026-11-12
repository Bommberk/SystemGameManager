# System & Game Manager - Changelog

### 🚀 Single Instance Architecture
- **Global Activation:** Implemented a robust `SingleInstanceWindow` handler to prevent multiple instances from running simultaneously, even when the main window is minimized or in the system tray.
- **Tray Integration:** Launched apps now automatically activate the existing hidden instance or restore the tray icon upon double-click, ensuring seamless state retention.

### 📱 Window Behavior & Restoration
- **Position Persistence:** Added comprehensive window state saving (`WindowX`, `Y`, Size, Maximized) to user settings. Windows no longer default to full-screen on every launch; they restore to your last used position and size.
- **Smart Start Logic:** Decoupled startup logic:
  - **"Start with Windows"** now registers the app in Startup regardless of running state.
  - **"Start in Background"** requires both "Enable Background Running" and "Start with Windows".

### 🎨 Visual Enhancements & UI Updates
- **Active Theme Highlighting:** Updated `app.js` to visually indicate the currently selected theme via active button states and improved styling (`elements.css`, `components.css`).
- **Section Layouts:** Reorganized Settings pages using a consistent `.section-1-2` flex layout for grouped settings (User/Startup vs. Wallpaper) and refined sidebar interactions.
- **CSS Refactoring:**
  - Standardized padding and margins across game management and audio settings sections.
  - Improved button hover effects and active states (`button.active`) with background color transitions.
  - Enhanced table aesthetics in Hotkeys section with distinct borders and spacing.

### 🖼️ Wallpaper Management Overhaul
- **Detection Improvements:** Simplified logic to distinguish between `DefaultWallpaperPath` and `LivelyExePath`, ensuring correct UI rendering for both static and live wallpapers.
- **Visual Feedback:** Added clear visual indicators in the Settings UI showing exactly which wallpaper source is currently active (Choose Default vs. Use Windows).

### 🛠 Technical Refinements
- **Startup Scripting:** Adjusted Windows Task Scheduler arguments to conditionally append `--background` flags based on user preferences.
- **Resource Management:** Properly disposed of window state timers during application shutdown and minimized states to prevent background memory leaks.
- **Development Mode:** Bumped internal version state from `stable` to `dev` for the next release cycle (v0.7.2).

### 📝 Documentation Update
- Clarified behavior regarding independent Windows Startup settings in `README.md`.