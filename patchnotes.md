# System & Game Manager – Changelog (v0.5.x)

### 🛠 Architecture Refactoring
- **Namespace Cleanup:** Removed the legacy `Krassheiten.` prefix from all namespaces, streamlining to standard scopes like `SystemGameManager.*`.
- **Module Restructuring:** Moved core functionality (`Database`, `PC Info`, `Games`) into a dedicated `/modules/` directory. Updated project references and `.gitignore` accordingly.
- **UI Decoupling & Page System:** Replaced the monolithic `MainForm.cs` logic with a modular page system under `/view/Pages/`. Introduced `GameManager`, `MenuPage`, `Settings`, and `Info` pages to decouple UI from service layers.

### 🎨 Visual & UX Improvements
- **Theming Engine:** Implemented robust `ColorThemes` class that automatically detects Dark/Light mode via Windows registry settings (`AppsUseLightTheme`) and applies consistent colors across the application.
- **Custom Controls:** Introduced reusable modular components:
  - `NormalButton`: Handles dynamic hover/down states based on theme configuration instead of hardcoded hex values.
  - `ModernTrackBar` & `HoverShadowPanel`: Custom implementations for volume controls and card shadows that respect global color themes.
  - SVG Icon Support: Integrated Font Awesome icons (SVG) loaded dynamically via helper methods, replacing legacy assets or images.
- **Layout Updates:** Redefined the UI with a dedicated side-bar navigation (`Navbar`) and consistent rounded corners based on global `ColorThemes`.

### 📦 Asset & Data Handling
- **Database Migration:** Adjusted configuration paths to load SQLite databases from `/modules/database/` (e.g., `systemgamemanager.db`). Updated `.csproj` copy logic.
- **Config Updates:** Enhanced EA Desktop integration in `knownLaunchers.json`, specifically adding support for the `"ProgramData"` path alongside standard installation paths.
- **Entity Refactoring:** Consolidated game and launcher data into new Entity modules under `/modules/game/`. Removed legacy entity classes (`Game.cs`, `Launcher.cs` from root) in favor of modular versions using generic arrays instead of Record types where appropriate.

### 🔧 Technical Fixes & Utilities
- **Error Handling:** Enhanced exception handling across view initialization states (e.g., data fetching failures, file missing errors).
- **Utility Helpers:** Added helper functions to darken/lighten colors dynamically (`Darker`, `Lighter`) based on theme settings and SVG processing utilities for dynamic icon color adjustment.
- **Console Output:** Fixed encoding issues in startup messages ("Audio-Monitoring läuft..." now displays correctly) by updating the target framework compatibility and string literals.

### 🧹 Code Hygiene & Cleanup
- **Legacy Removal:** Removed unused controllers (`PcInfoController`, `GameInfoView`) and legacy view classes that were replaced by new, modular components. Cleaned up monolithic service implementations (e.g., old `MainForm.cs` logic moved to pages).
- **Consolidation of Usings:** Standardized static usings in `GlobalUsings.cs` for cleaner file structures; removed unused imports and consolidated into specific module namespaces.