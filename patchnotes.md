# System & Game Manager – Changelog (v0.5.0)

### 🛠 Architecture Refactoring  
- **Namespace Cleanup:** Removed the `Krassheiten.` prefix from all namespaces, streamlining to standard scopes like `SystemGameManager.*`.
- **Module Restructuring:** Moved core functionality (`Database`, `PC Info`, `Games`) into dedicated modules within a `/modules/` directory. The project file now references assets and databases at this new path (e.g., `modules/database/systemgamemanager.db`).
- **Main Form Separation:** Decoupled UI from service layers by encapsulating the primary application window logic in the new `SystemGameManager.View.MainForm`, while moving view-specific pages into a `/view/Pages/` structure.

### 🎨 Visual & UX Improvements  
- **Theming Engine:** Implemented `ColorThemes` to automatically detect and apply system-wide Dark/Light mode settings via Windows registry detection (`ViewService`).
- **Custom Controls:** Introduced reusable custom themed components including:  
  - `NormalButton`: Handles dynamic hover/down states based on current theme.  
  - `ModernTrackBar`: Custom trackbar implementation for volume controls respecting colors.  
  - Card panels and containers that dynamically apply background/foreground colors per theme configuration.
- **Icon System:** Integrated Font Awesome SVG icons (e.g., `grip`, `bars`, `gamepad`) via a new helper (`UIHelpers.LoadIcon`), enabling scalable, crisp iconography across the UI.

### 📦 Asset & Data Handling  
- **Database Migration:** Moved SQLite database configuration from root `/database/` to the module structure at `/modules/database/systemgamemanager.db`.
- **Config Updates:** Adjusted paths for EA Desktop integration in `knownLaunchers.json`, specifically adding support for a new `"ProgramData"` path.
- **Asset Loading:** Consolidated asset handling logic into modular services, improving reliability when loading game artwork and icons from various application or user directory structures.

### 🔧 Technical Fixes & Utilities  
- **Error Handling:** Enhanced exception handling across view initialization states (e.g., `LoadInfoAsync`) to provide clearer feedback during data fetching failures.
- **Utility Helpers:** Added helper functions in `UIHelpers` to dynamically darken/lighten colors based on theme settings and SVG processing utilities for icon rendering.
- **Console Output:** Fixed encoding issues in startup messages ("Audio-Monitoring läuft..." now displays correctly) by updating the main project target framework (now targeting `.NET 10`).

### 🧹 Code Hygiene  
- Removed unused legacy controllers (disabled `PcInfoController` and `GameAudioController` instantiation where applicable).
- Consolidated static usings (`GlobalUsings.cs`) for cleaner file structure.
- Streamlined View logic by removing monolithic view classes like `MainForm`, splitting them into smaller page-based components that adhere to a modular design pattern.