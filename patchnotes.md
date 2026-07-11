# System & Game Manager – Changelog

### 🔧 Main Entry Logic Refactor
- **Safe Execution Flow:** Replaced the raw `try/catch` block in `Program.cs` with a dedicated exception handling pattern. All startup logic (Console vs GUI) now runs within a unified error-handling scope to prevent silent failures from crashing the app on launch arguments or unexpected exceptions.

### 🛡 Error Handling Integration
- **ErrorHandler Utilization:** Instantiated and integrated `SystemGameManager.Handler.ErrorHandler` into the main entry point (`Program.cs`).
- **Exception Logging/Display:** Captured all unhandled exceptions during startup (e.g., argument parsing errors, missing dependencies) and now route them to a centralized handling routine instead of terminating abruptly or showing default OS dialogs.

### ⚙️ Code Cleanup & Dependencies
- **Namespace Import:** Added `using SystemGameManager.Handler;` to resolve the new exception handler usage without circular dependency issues.