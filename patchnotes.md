Here are the patchnotes based on your git diff data. The previous placeholder content in `patchnotes.md` was removed to make room for accurate details derived directly from the changes.

# System & Game Manager – Changelog (v0.5.0)

### 🛠 Architecture Refactoring
- **Namespace Cleanup:** Removed the legacy `Krassheiten.` prefix from all namespaces (`Controller`, `Service`, `Entity`), streamlining to standard scopes like `SystemGameManager.*`.
- **Module Restructuring:** Moved core functionality into a dedicated `/modules/` directory:
  - Renamed root database files and configs to `modules/database/systemgamemanager.db`.
  - Consolidated Services, Controllers, Views, and Entities under their respective module folders.
- **UI Decoupling:** Separated the primary application window logic from service layers by encapsulating it in the new `/view/MainForm.cs` structure. Removed legacy monolithic controllers (`Game`, `Launcher`) and replaced them with static entity managers within modules.

### 🎨 Visual & UX Improvements
- **Theming Engine:** Implemented a robust `ColorThemes` system that automatically detects Dark/Light mode via Windows registry settings and applies consistent colors across the application (`.Net 10`).
- **Custom Controls:** Introduced modular, reusable custom components:
  - `NormalButton`: Handles dynamic hover/down states based on theme configuration.
  - `ModernTrackBar`: Custom trackbar implementation for volume controls respecting current color themes.
  - Themed panels (`HoverShadowPanel`) that adapt background colors dynamically per view state.
- **Icon System:** Integrated Font Awesome SVG icons (via a new helper) for scalable, crisp iconography replacing hardcoded images or legacy assets in the navigation bar and cards.

### 📦 Asset & Data Handling
- **Database Migration:** Adjusted project references to load SQLite databases from the module structure (`modules/database/`). Added `.exe` files to release exclusions where necessary.
- **Config Updates:** Updated `knownLaunchers.json` for EA Desktop, specifically adding support for a new `"ProgramData"` path alongside standard installation paths.
- **Asset Management:** Consolidated asset handling logic (artwork loading) into modular services and updated project file references (`SystemGameManager.csproj`) to automatically copy SVG icons from `/assets/icons/`.

### 🔧 Technical Fixes & Utilities
- **Error Handling:** Enhanced exception handling across view initialization states (e.g., `LoadInfoAsync`, service creation) to provide clearer feedback during data fetching failures.
- **Utility Helpers:** Added helper functions (`Darker`, `Lighter`) and SVG processing utilities for dynamic icon color adjustment based on the active theme.
- **Console Output:** Fixed encoding issues in startup messages ("Audio-Monitoring läuft..." now displays correctly by updating target framework compatibility).

### 🧹 Code Hygiene & Refactoring
- **Legacy Cleanup:** Removed unused legacy controllers (`PcInfoController`, `GameInfoView`) and monolithic view classes (e.g., old `MainForm.cs`). Split functionality into smaller, page-based components adhering to the modular design pattern.
- **Consolidation of Usings:** Standardized static usings in `GlobalUsings.cs` for cleaner file structures; removed unused imports (`GameEntity = ...`) where they conflicted with direct usage patterns.