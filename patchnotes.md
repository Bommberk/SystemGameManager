# System & Game Manager – Changelog

### 🛠 Architecture & Runtime Stability
- **Working Directory Fix:** Resolved startup path issues in production builds. The application now explicitly sets the working directory to `AppContext.BaseDirectory` via `Program.cs`, ensuring assets and configurations load correctly regardless of launch location.
- **Asset Path Standardization:** Updated `LauncherService` to dynamically construct asset paths using `Path.Combine`. This prevents `FileNotFoundException` errors caused by hardcoded relative paths.

### 🔐 Hardware Identification Update
- **Field Renaming:** Replaced the term `DeviceID` with `PublisherUUID` across `PcInfoController` and associated smart home plugins.
  - *Impact:* Aligns internal terminology with WMI standards (`Win32_ComputerSystemProduct`).
  - *Breaking Change:* External integrations expecting the key `DeviceID` will now receive data under `PublisherUUID` (or encounter nulls if strictly checking the old property name).
- **Error Messaging:** Updated exception messages in `SmarthomeApi.cs` to reflect the new `PublisherUUID` terminology.

### ⚙️ Technical Details
- **File Structure:** No changes to module directory layout, but path resolution logic has been hardened for cross-drive installations.