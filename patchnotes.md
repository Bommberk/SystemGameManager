# System & Game Manager – Changelog (Latest Update)

### 🛠 Architecture Refactoring
- **Namespace Cleanup:** Removed the `Krassheiten.` prefix from all namespaces, streamlining to standard scopes like `SystemGameManager.*`.
- **Module Restructuring:** Moved core functionality (`Database`, `PC Info`, `Games`) into dedicated modules within a new `/modules/` directory.
- **Main Form Separation:** The primary application window logic is now encapsulated in the new `/view/MainForm.cs`, decoupling UI from service layers.

### 🎨 Visual & UX Improvements
- **Theming Engine:** Implemented `ColorThemes` to automatically detect and apply system-wide Dark/Light mode settings via Windows registry detection (`ViewService`).
- **Custom Controls:** Introduced custom themed controls including:
  - `ThemedTabControl`: Tab pages now respect theme colors without manual overrides.
  - Dynamic button states using helper methods for hover/down effects instead of hardcoded hex values.
- **Layout Updates:** Redesigned the UI with a new side-bar layout and consistent rounded corners based on global color themes.

### 📦 Asset & Data Handling
- **New Icons:** Added Font Awesome icons (SVG) to `assets/icons/` (`grip`, `bars`, `circle-info`).
- **Database Structure:** Moved SQLite database configuration from root `/database/` to the new module structure at `/modules/database/systemgamemanager.db`.
- **Config Updates:** Adjusted paths for EA Desktop integration and fixed null-handling in Launcher data.

### 🔧 Technical Fixes & Utilities
- **Error Handling:** Enhanced exception handling across `MainForm` loading states (`LoadInfoAsync`).
- **Utility Helpers:** Added helper functions to darken/lighten colors dynamically based on theme settings.
- **Console Output:** Fixed encoding issue in startup messages ("Audio-Monitoring läuft..." now displays correctly).

### 🧹 Code Hygiene & Cleanup
- Removed unused controllers (disabled `PcInfoController` and `GameAudioController` instantiation in debug builds to reduce overhead during info loading).
- Consolidated static usings (`GlobalUsings.cs`) for cleaner file structure.