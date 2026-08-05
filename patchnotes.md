# System & Game Manager – Changelog (v0.5.10 → v0.6.0)

### 🏗 Architecture Refactoring
- **WebView2 Integration:** Introduced a modern web-based frontend (`view2.0/`) utilizing `Microsoft.Web.WebView2`. The main application now embeds an HTML/CSS/JS interface for managing games and launchers.
- **Web API Handler:** Implemented bidirectional communication between the C# backend and the WebView via `Handler/WebApiHandler`, handling actions like `getGames`, `setLaunchers`, and audio configuration remotely from JS.
- **View Module Migration:** Updated project structure to support the new view hierarchy (`SystemGameManager.View` → `SystemGameManager.View2`) while maintaining legacy compatibility paths in build configurations.

### 🎨 Visual & UX Improvements
- **Modern UI Framework:** Replaced traditional WinForms controls with a responsive web-based interface featuring:
  - Sidebar navigation (Menu, Dashboard, Game Manager, Settings).
  - Card-based layouts for games and launchers using CSS Grid/Flexbox.
  - Real-time list rendering of installed applications via JavaScript data binding.
- **Theming Engine:** Native support for multiple color themes (**Dark**, **Light**, **Red**, **Pink**, **Yellow**) applied globally to the WebView content, respecting system settings where applicable.
- **Dynamic Asset Loading:** Optimized image handling with fallback logic and new launcher logos (Steam, Ubisoft Connect) integrated into `view2.0/assets/`.

### 📦 Data & API Enhancements
- **Serialized Game Names:** Added `SerializedGameName` property to the `Game` entity for safe JSON transmission between C# and JavaScript without special character encoding issues.
- **Smarthome Integration (Optional):** Refactored startup logic in `Program.cs` to conditionally initialize optional Smarthone API calls, ensuring stability even if external services are unavailable.
- **Utility Functions:** Introduced a centralized message box helper (`GlobalFunctions.msgbox`) for consistent user feedback across the application and WebView communication errors.

### 🧹 Code Hygiene & Build Updates
- **Project Configuration:** Streamlined `SystemGameManager.csproj` by updating icon asset patterns to support wildcards in `assets/icons/**/*` and properly registering new files under `view2.0`.
- **Asset Cleanup:** Removed obsolete static SVG icons from direct project copy lists, replacing them with web-based assets where appropriate for the new UI layer.