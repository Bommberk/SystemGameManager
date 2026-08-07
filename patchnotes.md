# System & Game Manager – Changelog v0.6.2

### 🛠 Feature: Local Image Asset Loading
- **Custom WebView Handler:** Implemented `ConfigureLocalImageRequests` in `MainForm.cs` to serve game assets from the local filesystem (`assets/`) directly within the Electron/WebView environment.
- **Dynamic Content Types:** Added logic to detect image extensions (PNG, JPG, WEBP, GIF) and return appropriate MIME types for seamless rendering.
- **Fallback Handling:** Images failing to load on localhost automatically revert to a default placeholder (`../assets/images/bild.jpg`).

### 🔌 Feature: Game Image Management API
- **New Endpoint `changeGameImage`:** Added handling in `WebApiHandler.cs` and exposed via the new `api.js` module.
- **File Upload Flow:** Users can now click an icon to upload a custom image (`.png`, `.jpg`, etc.) from their local drive for specific games.
- **Menu Actions:** Introduced context menus on game cards allowing users to:
  - Launch Game
  - Change Cover Image
  - Open Install Folder
  - Remove Entry

### 🎨 Visual & UX Improvements
- **Sidebar Refactor:** 
  - Replaced inline SVGs with Font Awesome icons (`fa-house`, `fa-headset`, etc.) for cleaner code.
  - Implemented collapsible sidebar layout using CSS variables (`--sidebar-width` vs `--sidebar-width-collapsed`).
  - Added animated hover effects and text labels appearing on the right side of menu items when collapsed.
- **Game Cards:** 
  - Updated layout to accommodate custom cover images loaded via local paths.
  - Added a vertical ellipsis button (⋮) triggering a popup menu for game-specific actions.
- **Styling Consistency:** 
  - Standardized slider components across the UI with theme-aware colors and rounded thumbs.
  - Adjusted color palette variables (`--tertiary-text-color`) to improve contrast in light/dark modes.

### 📄 Code Hygiene & Architecture
- **API Script Extraction:** Consolidated JavaScript API definitions into a dedicated `assets/script/api.js` file, separating data models and communication logic from the main application script (`app.js`).
- **Cleanup:** Removed unused inline SVGs and redundant global variable declarations in favor of scoped functions.