# System & Game Manager – Changelog v2.0.4 (WebView Migration)

### 🖥️ WebView2 Web Interface Integration
- **New WebView Module:** Introduced `Microsoft.Web.WebView2` to render a modern, web-based UI directly within the application window.
- **JavaScript Bridge:** Implemented bidirectional communication (`WebApiHandler`) allowing the C# backend and JavaScript frontend to exchange data (games/launchers) via postMessage API calls.
- **Cross-Origin Mapping:** Enabled virtual host mapping (`systemgamemanager://`) in `MainForm.cs` to load local assets without security errors.

### 🎨 Frontend Redesign (`view2.0`)
- **SPA Architecture:** Replaced the legacy WinForms UI with a Single Page Application structure containing:
  - Dynamic routing via `/assets/script/router.js`.
  - Modular pages: Dashboard, Game Manager, Settings, and About sections.
- **Theming Engine:** Added client-side CSS variables supporting multiple themes (Dark, Light, Red, Pink, Yellow) based on system preferences or manual override in settings.
- **Asset Management:** Migrated icon packs to `/assets/images/launcher_logos` with automated fallback logic for missing images.

### ⚙️ Backend & Data Handling Updates
- **Game Entity Refactor:** Added `SerializedGameName` property to `Game.cs`. This sanitizes folder names (replacing invalid chars) to ensure reliable database lookups and unique identifiers across the UI.
- **Bulk Operations:** Introduced `UpdateMultibleGames()` method in `GameService.cs` for efficient batch-updates via the new API interface.
- **API Utilities:** Created a dedicated `SmarthomeApi` integration that now runs automatically on startup (non-dev environments) to fetch device info, wrapped with robust error handling (`ErrorSeverity.Warning`).

### 🛠 Utility Improvements
- **Global Helper Added:** New `msgbox()` method in `GlobalFunctions.cs` for standardized MessageBox usage across the application.
- **Asset Cleanup:** Consolidated `<ItemGroup>` definitions in `.csproj`; switched specific SVG icons to a wildcard pattern (`assets\icons\**\*`) to reduce file-level configuration overhead.

### 📦 Package & Configuration Changes
- **Dependency Update:** Added `Microsoft.Web.WebView2 v1.0.4129` dependency; removed unused Font Awesome icon package references.
- **Build Script Updates:** Updated project file paths for new view resources and ensured assets copy correctly in both Debug/Release modes using wildcard patterns where applicable.