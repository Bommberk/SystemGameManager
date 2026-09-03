# System & Game Manager – Changelog

### 🎨 UI/UX Refactoring
- **WebView2 Layout**: Restructured the Audio Management section into distinct, scrollable cards for "Select Game", "Device Selection", "Game Volume", and "Music Volume".
- **New Logo**: Added `sysgamemanager_logo.png` to the header for better branding consistency.
- **Styling Updates**:
  - Introduced dedicated `.save-btn` classes with icon animations (save/check toggle) on focus/click.
  - Customized form elements (`input`, `select`, `button`) with rounded corners and theme-aware borders.
  - Added a custom CSS file for `h2` margins to improve section spacing.
- **Filtering UX**: Updated the game list header to dynamically display the count of filtered games (e.g., `(12)`).

### 🧠 Logic & Data Handling
- **Hidden Games Feature**: Implemented `IsRemovedFromView` flag in `Game.cs` and updated the Web API handler. Games marked as removed are excluded from the UI list (`handleGames`) but persist in the database for future restoration (if implemented).
- **Granular Audio Saving**: Split audio saving into three separate endpoints to allow modifying individual settings (Device, Game Volume, Music Volume) without resetting others.
- **Selection Logic**: Updated selection handlers to respect the `IsRemovedFromView` status, preventing interaction with hidden games.

### 🔧 Technical Fixes & Cleanup
- **Console Messaging**: Removed the `MessageBox.Show("Games were updated")` popup from `WebApiHandler.cs` to prevent intrusive alerts during background processes or console runs.
- **Null Handling**: Added explicit null checks (`!= false`) when loading database values into game entities to ensure consistency with boolean flags.

### 📦 Assets
- **Icons**: Replaced generic images in the header with a new custom logo asset.