# System & Game Manager – Changelog

### 🛡️ Robustness & Crash Recovery (`Program.cs`)
- **Centralized Exception Handling:** Replaced scattered `try/catch` blocks with a unified fallback in `Main()`. Even if the application fails during initialization, the error is now captured and reported to the user before termination.
- **Safe Logging Registration:** Moved `ErrorHandler.Register()` to after the initial app start attempt, ensuring global exception hooks are active even for crashes occurring early on startup or updates failing later.

### 🚑 Enhanced Error Reporting (`Handler/ErrorHandler.cs`)
- **Persistent Logging Infrastructure:** Implemented daily log rotation and thread-safe file appending in `%APPDATA%\SystemGameManager\logs`. Errors now persist locally regardless of UI visibility, preventing data loss during critical failures.
- **Structured Exception Details:** Overhauled the error report to include Timestamp, Severity Level (`Warning`, `Error`, `Fatal`), Type, Source, Full Stack Trace, and recursive Inner Exceptions for root cause analysis.
- **Global Safety Net:** Registered handlers for:
  - UI Thread exceptions.
  - Non-UI background thread failures (e.g., database operations).
  - Asynchronous task unobserved exceptions (`TaskScheduler.UnobservedTaskException`).

### 🔧 Update Stability (`Updater.cs`)
- **Soft-Failed Updates:** Upgrades that encounter issues now log a warning instead of showing an abrupt error dialog. This prevents the application from crashing immediately after checking for updates, allowing it to continue running normally and attempting subsequent fixes or manual restarts.