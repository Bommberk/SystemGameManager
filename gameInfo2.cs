namespace Infos;

public class GameInfo2
{
    private const string MinecraftPackageFamilyName = "Microsoft.4297127D64EC6_8wekyb3d8bbwe";
    private const string KnownLaunchersFileName = "knownLaunchers.json";

    private static readonly Lazy<IReadOnlyList<LauncherDefinition>> KnownLaunchersCache = new(LoadKnownLaunchers);

    private static IReadOnlyList<LauncherDefinition> KnownLaunchers => KnownLaunchersCache.Value;

    private static readonly string[] UninstallRegistryPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];

    public IReadOnlyList<LauncherInstallation> Launchers { get; } = GetInstalledLaunchers();
    public string[]? Launcher { get; }
    public string[]? Games { get; }

    public GameInfo2()
    {
        Launcher = Launchers.Select(item => item.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public void Write()
    {
        Console.WriteLine("==============================");
        Console.WriteLine("      Game-Informationen      ");
        Console.WriteLine("==============================");

        if (Launchers.Count == 0)
        {
            Console.WriteLine("Keine installierten Spiele-Launcher gefunden.");
            Console.WriteLine();
            return;
        }

        foreach (var launcher in Launchers
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Drive)
            .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"{launcher.Name,-24} {launcher.Drive,-4} {launcher.Path}");
        }

        Console.WriteLine();
    }

    public static IReadOnlyList<LauncherInstallation> GetInstalledLaunchers()
    {
        var foundLaunchers = new Dictionary<string, LauncherInstallation>(StringComparer.OrdinalIgnoreCase);

        foreach (var launcher in FindSpecialLaunchers())
        {
            foundLaunchers[NormalizePath(launcher.Path)] = launcher;
        }

        foreach (var launcher in FindLaunchersFromRegistry())
        {
            foundLaunchers[NormalizePath(launcher.Path)] = launcher;
        }

        foreach (var launcher in FindLaunchersFromKnownPaths())
        {
            foundLaunchers.TryAdd(NormalizePath(launcher.Path), launcher);
        }

        return foundLaunchers.Values
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Drive)
            .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<LauncherInstallation> FindSpecialLaunchers()
    {
        var steamLauncher = FindSteamLauncher();
        if (steamLauncher is not null)
        {
            yield return steamLauncher;
        }

        var minecraftLauncher = FindMinecraftLauncher();
        if (minecraftLauncher is not null)
        {
            yield return minecraftLauncher;
        }
    }

    private static LauncherInstallation? FindSteamLauncher()
    {
        foreach (var view in new[] { Microsoft.Win32.RegistryView.Registry64, Microsoft.Win32.RegistryView.Registry32 })
        {
            try
            {
                using var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, view);
                using var steamKey = baseKey.OpenSubKey(@"Software\Valve\Steam");
                if (steamKey is null)
                {
                    continue;
                }

                var steamExe = ExtractExecutablePath(steamKey.GetValue("SteamExe") as string);
                if (!string.IsNullOrWhiteSpace(steamExe) && File.Exists(steamExe))
                {
                    return CreateInstallation("Steam", steamExe);
                }

                var steamPath = steamKey.GetValue("SteamPath") as string;
                if (!string.IsNullOrWhiteSpace(steamPath))
                {
                    var candidatePath = Path.Combine(Environment.ExpandEnvironmentVariables(steamPath), "steam.exe");
                    if (File.Exists(candidatePath))
                    {
                        return CreateInstallation("Steam", candidatePath);
                    }
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static LauncherInstallation? FindMinecraftLauncher()
    {
        var localPackages = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Packages",
            MinecraftPackageFamilyName);

        if (!Directory.Exists(localPackages))
        {
            return null;
        }

        var windowsAppsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps");
        if (!Directory.Exists(windowsAppsRoot))
        {
            return null;
        }

        try
        {
            var packageDirectory = Directory.EnumerateDirectories(windowsAppsRoot, "Microsoft.4297127D64EC6*", SearchOption.TopDirectoryOnly)
                .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (packageDirectory is null)
            {
                return null;
            }

            var gameLaunchHelperPath = Path.Combine(packageDirectory, "GameLaunchHelper.exe");
            if (File.Exists(gameLaunchHelperPath))
            {
                return CreateInstallation("Minecraft Launcher", gameLaunchHelperPath);
            }
        }
        catch
        {
        }

        return CreateInstallation("Minecraft Launcher", localPackages);
    }

    private static IReadOnlyList<LauncherDefinition> LoadKnownLaunchers()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, KnownLaunchersFileName);
        if (!File.Exists(filePath))
        {
            return [];
        }

        using var stream = File.OpenRead(filePath);
        using var document = System.Text.Json.JsonDocument.Parse(stream);

        if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return [];
        }

        var launchers = new List<LauncherDefinition>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var name = ReadRequiredString(element, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            launchers.Add(new LauncherDefinition(
                name,
                ReadStringArray(element, "executableNames"),
                ReadStringArray(element, "displayNames"),
                ReadStringArray(element, "pathMarkers"),
                ReadStringArray(element, "knownRelativePaths")));
        }

        return launchers;
    }

    private static string ReadRequiredString(System.Text.Json.JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == System.Text.Json.JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static IReadOnlyList<string> ReadStringArray(System.Text.Json.JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return [];
        }

        return property.EnumerateArray()
            .Where(item => item.ValueKind == System.Text.Json.JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();
    }

    private static IEnumerable<LauncherInstallation> FindLaunchersFromRegistry()
    {
        foreach (var view in new[] { Microsoft.Win32.RegistryView.Registry64, Microsoft.Win32.RegistryView.Registry32 })
        {
            foreach (var hive in new[] { Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryHive.CurrentUser })
            {
                Microsoft.Win32.RegistryKey? baseKey;

                try
                {
                    baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(hive, view);
                }
                catch
                {
                    continue;
                }

                using (baseKey)
                {
                    foreach (var uninstallPath in UninstallRegistryPaths)
                    {
                        using var uninstallKey = baseKey.OpenSubKey(uninstallPath);
                        if (uninstallKey is null)
                        {
                            continue;
                        }

                        foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                        {
                            using var appKey = uninstallKey.OpenSubKey(subKeyName);
                            var match = TryCreateFromRegistryValues(appKey);
                            if (match is not null)
                            {
                                yield return match;
                            }
                        }
                    }

                    foreach (var definition in KnownLaunchers)
                    {
                        foreach (var executableName in definition.ExecutableNames)
                        {
                            using var appPathsKey = baseKey.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{executableName}");
                            var match = TryCreateFromRegistryValues(appPathsKey, definition);
                            if (match is not null)
                            {
                                yield return match;
                            }
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<LauncherInstallation> FindLaunchersFromKnownPaths()
    {
        foreach (var drive in System.IO.DriveInfo.GetDrives()
            .Where(drive => drive.IsReady && drive.DriveType is DriveType.Fixed or DriveType.Removable))
        {
            foreach (var definition in KnownLaunchers)
            {
                foreach (var relativePath in definition.KnownRelativePaths)
                {
                    var candidatePath = Path.Combine(drive.RootDirectory.FullName, relativePath);
                    if (File.Exists(candidatePath))
                    {
                        yield return CreateInstallation(definition.Name, candidatePath);
                    }
                }
            }
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userCandidates = new[]
        {
            Path.Combine(localAppData, "Programs", "Minecraft Launcher", "MinecraftLauncher.exe"),
            Path.Combine(localAppData, "Riot Games", "Riot Client", "RiotClientServices.exe"),
            Path.Combine(roamingAppData, "Spotify", "EpicGamesLauncher.exe")
        };

        foreach (var candidatePath in userCandidates.Where(File.Exists))
        {
            var definition = MatchDefinitionByPath(candidatePath);
            if (definition is not null)
            {
                yield return CreateInstallation(definition.Name, candidatePath);
            }
        }
    }

    private static LauncherInstallation? TryCreateFromRegistryValues(Microsoft.Win32.RegistryKey? appKey, LauncherDefinition? knownDefinition = null)
    {
        if (appKey is null)
        {
            return null;
        }

        var displayName = appKey.GetValue("DisplayName") as string;
        var displayIcon = appKey.GetValue("DisplayIcon") as string;
        var installLocation = appKey.GetValue("InstallLocation") as string;
        var executablePath = appKey.GetValue(string.Empty) as string;

        var definition = knownDefinition
            ?? MatchDefinitionByDisplayName(displayName)
            ?? MatchDefinitionByPath(displayIcon)
            ?? MatchDefinitionByPath(installLocation)
            ?? MatchDefinitionByPath(executablePath);

        if (definition is null)
        {
            return null;
        }

        var candidatePaths = new[]
        {
            ExtractExecutablePath(displayIcon),
            ExtractExecutablePath(executablePath),
            BuildPathFromInstallLocation(installLocation, definition)
        };

        var resolvedPath = candidatePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Where(path => IsValidLauncherPath(definition, path!))
            .FirstOrDefault(File.Exists);

        return resolvedPath is null ? null : CreateInstallation(definition.Name, resolvedPath);
    }

    private static LauncherDefinition? MatchDefinitionByDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        return KnownLaunchers.FirstOrDefault(definition =>
            definition.DisplayNames.Any(name => displayName.Contains(name, StringComparison.OrdinalIgnoreCase)));
    }

    private static LauncherDefinition? MatchDefinitionByPath(string? rawPath)
    {
        var extractedPath = ExtractExecutablePath(rawPath) ?? rawPath;
        if (string.IsNullOrWhiteSpace(extractedPath))
        {
            return null;
        }

        var path = NormalizeSlashes(extractedPath);

        var fileName = Path.GetFileName(path);
        var hasExtension = Path.HasExtension(path);

        return KnownLaunchers.FirstOrDefault(definition =>
            ((hasExtension && definition.ExecutableNames.Any(name => string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase)))
             || definition.PathMarkers.Any(marker => path.Contains(marker, StringComparison.OrdinalIgnoreCase)))
            && MatchesPathMarkers(definition, path));
    }

    private static string? BuildPathFromInstallLocation(string? installLocation, LauncherDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(installLocation) || !Directory.Exists(installLocation))
        {
            return null;
        }

        foreach (var executableName in definition.ExecutableNames)
        {
            var directPath = Path.Combine(installLocation, executableName);
            if (File.Exists(directPath))
            {
                return directPath;
            }
        }

        return definition.ExecutableNames
            .Select(executableName => SafeEnumerateFiles(installLocation, executableName).FirstOrDefault())
            .FirstOrDefault(path => path is not null && IsValidLauncherPath(definition, path));
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root, string executableName)
    {
        try
        {
            return Directory.EnumerateFiles(root, executableName, new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false,
                MaxRecursionDepth = 4
            });
        }
        catch
        {
            return [];
        }
    }

    private static string? ExtractExecutablePath(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var value = Environment.ExpandEnvironmentVariables(rawValue.Trim());

        if (value.StartsWith('"'))
        {
            var endQuote = value.IndexOf('"', 1);
            if (endQuote > 1)
            {
                return value[1..endQuote];
            }
        }

        var exeIndex = value.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exeIndex >= 0)
        {
            return value[..(exeIndex + 4)];
        }

        return value;
    }

    private static LauncherInstallation CreateInstallation(string name, string path)
    {
        var normalizedPath = NormalizePath(path);
        var root = Path.GetPathRoot(normalizedPath) ?? "?";
        return new LauncherInstallation(name, normalizedPath, root);
    }

    private static bool IsValidLauncherPath(LauncherDefinition definition, string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        var normalizedPath = NormalizeSlashes(path);
        var fileName = Path.GetFileName(normalizedPath);

        return definition.ExecutableNames.Any(name => string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase))
            && MatchesPathMarkers(definition, normalizedPath);
    }

    private static bool MatchesPathMarkers(LauncherDefinition definition, string path)
    {
        return definition.PathMarkers.Count == 0
            || definition.PathMarkers.Any(marker => path.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
    }

    private static string NormalizeSlashes(string path)
    {
        return path.Replace('/', '\\');
    }

    public sealed record LauncherInstallation(string Name, string Path, string Drive);

    private sealed record LauncherDefinition(
        string Name,
        IReadOnlyList<string> ExecutableNames,
        IReadOnlyList<string> DisplayNames,
        IReadOnlyList<string> PathMarkers,
        IReadOnlyList<string> KnownRelativePaths);
}