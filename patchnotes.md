# System & Game Manager – Changelog

### 🛠 Architecture & API Enhancements
- **New Web API Command:** Added `removeGameFromView` handler in `WebApiHandler.cs`. Allows users to permanently hide specific games from the dashboard without deleting database records.
- **Command Line Interface:** Introduced `--infos` flag in `Program.cs` to quickly retrieve system/game info via terminal args.
- **Entity Model Update:** Extended `Game` class with `IsRemovedFromView` boolean property to track hidden state across layers.

### 🎨 Visual & UX Overhauls
- **Grid Layout Upgrade:** Replaced the flex-based game list with a responsive CSS Grid (`display: grid`) in `gamemanager.css`. Automatically adjusts columns based on window width for better responsiveness.
- **Popup Interaction Fix:** Refactored menu popups to prevent overlap conflicts. Clicking anywhere outside a popup now correctly closes it, and active state toggling is now robust.
- **Slider Styling:** Reduced slider height from `8px` to `3px` and added dynamic gradient styling based on value for a more modern look.
- **Dark Mode Detection:** Automatically apply "light" theme class if the system prefers color-scheme is not dark, ensuring consistent appearance on boot.

### 🧹 Code Hygiene & Utilities
- **Volume Slider Helpers:** Centralized slider value handling into a new `changeValue` function, removing repetitive inline event listeners and fixing previous commented-out logic.
- **Cleanup:** Removed unused volume slider listeners and consolidated popup state management (`currentPopup`, `isActivatedPopup`) to ensure only one popup is open at a time.

### 📦 Asset & Data Handling
- **Removed Games Logic:** When a game is removed from the view, its `IsRemovedFromView` flag is set to `true`. The UI renders these games as invisible, while backend data remains intact for potential future restoration or archival.