# System & Game Manager – Changelog

### 📦 Asset & Data Handling
- **Launcher Registry:** Added support for tracking game launchers with file paths. Two new JSON assets have been introduced and deployed to the output directory:
  - `assets\game\knownLaunchersWithPath.json`: Contains known launcher executables configured with full installation path data.
  - `assets\game\knownLaunchers.json`: Stores a registry of recognized generic launchers without specific paths.