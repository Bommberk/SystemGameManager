# System & Game Manager – Changelog

### 🛠 Architecture Refactoring
- **WebView Integration:** Migrated UI from `view2.0/` to `view/` (HTML5/CSS3/JS), leveraging Microsoft WebView2 for the main rendering engine. Removed legacy WinForms heavy-hydrate controls (RichTextBox, Panel-based cards) in favor of a hybrid WebView approach.
- **Project Structure Cleanup:** Removed `config/appsettings.json` loading logic from `GlobalConfig` (now uses default instantiation). Updated `.csproj` to exclude deleted configuration files and map new view assets correctly.
- **Code Consolidation:** Deleted extensive WinForms boilerplate code (`PcInfoView`, `GameManager`, `Page` hierarchy) that was redundant with the new WebView implementation.

### 🎨 Visual & UX Improvements
- **Asset Migration:** Moved all assets (icons, game logos, images) from `view2.0/` to `view/`. Updated build configurations to copy resources correctly.
- **Styling Updates:**
  - Added CSS scrollbars (`::-webkit-scrollbar`) and collapsible section logic (accordion styles for Launcher/Audio/Game lists).
  - Fixed layout overflow issues in Game Manager by adjusting container heights and adding padding.
  - Unified color variables (added `--scrollbar-thumb-color` / `--scrollbar-track-color` for Light/Dark modes).
- **Script Refactoring:** Split monolithic `app.js` into modular scripts (`games.js`, `router.js`) to improve maintainability and separate Game List logic from general routing.

### 📦 Asset & Data Handling
- **Configuration Simplification:** Replaced external JSON config loading with static default values in `GlobalConfig` for a lighter startup.
- **Path Logic Updates:** Modified `MainForm` to detect development environments dynamically, ensuring correct mapping of the `view/` folder during builds vs. local runs.

### 🔧 Technical Fixes & Utilities
- **Image Handling:** Enhanced `ConfigureLocalImageRequests` in WebView to better handle image MIME types and 404 fallbacks for missing game logos (using placeholder logic).
- **Error Handling:** Improved error messages when accessing non-existent game directories or loading invalid image paths.
- **Cleanup:** Removed unused classes (`ColorThemes`, `CardControls`, `HoverShadowPanel`, `NormalButton`) that were specific to the previous WinForms card system, as their functionality was abstracted or moved away from direct UI manipulation.

### 🧹 Code Hygiene
- **Namespace Renaming:** Aligned file paths and namespaces (`SystemGameManager.View` vs `SystemGameManager.View2`) with the new folder structure.
- **File Consolidation:** Deleted intermediate pages (`Info`, `MenuPage`, `Settings` placeholders) and services (`GameManagerViewService`, `ViewService`) that were no longer strictly necessary for the WebView-based routing system.