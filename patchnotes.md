# System & Game Manager – Changelog  

### 🛠 Architecture Refactoring  
- **Namespace Cleanup:** Removed the `Krassheiten.` prefix from all namespaces (e.g., `GlobalController.cs` → `SystemGameManager.Functions`).  
- **Module Restructuring:** Core components moved into a dedicated `/modules/` directory. Specifically:
  - Moved files like `DatabaseController`, `GameInfoController`, and view logic to subdirectories under `/modules/`. 
  - Created separate modules for games, game entities (`SystemGameManager.Games.Entity`) etc., consolidating the project structure into more manageable chunks.
- **Main Form Separation:** Encapsulated UI logic into new pages located in `/view/Pages/` (e.g., `GameManager`, `GameAudioView`). Moved from a single file approach to separate modules for cleaner separation of concerns and scalability.  

### 🎨 Visual & UX Improvements  
- **Theming Engine:** Introduced `ColorThemes` class with support for automatic detection of system-wide Dark/Light mode settings via Windows registry (`SystemGameManager.View.Service.ViewService`) ensuring consistent theming across all components.
  - Dynamic button states using helper methods in `UIHelpers.Lighter/Darker()` to adapt colors based on current theme settings, replacing hardcoded hex values.  
- **Custom Controls:** Introduced reusable custom themed controls including: 
  - Themed `TableLayoutPanel` and panels that respect global color themes without manual overrides (e.g., via `ColorThemes.GetPrimaryBackgroundColor()`).
  - Rounded corner panels using a dedicated helper (`UIHelpers.SetRoundedRegion`).  
- **Icon Management:** Replaced hardcoded image paths with Font Awesome SVG icons in `/assets/icons/`. Added dynamic loading of these SVGs into the application.  

### 📦 Asset & Data Handling  
- **New Icons:** Added Font Awesome 7.x icons as SVG assets to `/assets/icons/` including: `gamepad`, `gear`, `house`, and more.  
- **Database Structure:** Moved SQLite database configuration from root folder (`database/systemgamemanager.db`) into the new module structure at `/modules/database/systemgamemanager.db`. Added a template DB file as well for easier deployment. 
- **Config Updates:** Adjusted paths in `.csproj` files to reflect modular directory structures, and added entries for tracking release-related build outputs like `publishnewversion.ps1` scripts.  

### 🔧 Technical Fixes & Utilities  
- **Error Handling:** Enhanced exception handling across app startup (`Program.cs`) with commented-out debug functions removed during production builds. 
- **Utility Helpers:** Introduced helper methods in `UIHelpers.cs`:
  - Added color manipulation utilities for theme support (e.g., darkening/lightening colors dynamically).
  - SVG icon loader function that adjusts stroke width/color based on current theme settings (`ChangeNavbarMenuIconColor`).  
- **Console Output:** Fixed encoding issue where `"Audio-Monitoring läuft..."` displayed incorrectly by removing unnecessary characters and simplifying startup messaging logic.  

### 🧹 Code Hygiene & UI Cleanup
- Removed unused or redundant controllers (e.g., disabled old PC info functionality) and consolidated static usings in `GlobalUsings.cs`.  
- Refactored game card layouts into modular components (`CardControls`, `GameAudioCardControl`) enabling consistent styling across the app.