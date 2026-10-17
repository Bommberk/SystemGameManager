namespace SystemGameManager.Games.Service;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SystemGameManager.Games.Entity;

class GetGameProcess
{
    private const string JAVA_PROCESS_NAME = "javaw";
    private static readonly string[] MinecraftCommandLineMarkers =
    [
        "net.minecraft",
        "--assetIndex",
        "--accessToken",
        "--gameDir",
        "com.mojang",
        "net.fabricmc",
        "net.minecraftforge",
        "cpw.mods",
        "lunar",
        "badlion",
        "feather"
    ];
    public static Game? GetRunningOpenGame()
    {
        if (Game.InstalledGames == null || Game.InstalledGames.Length == 0)
        {
            return null;
        }

        Game? gameProcess = TryGetForegroundGame(Game.InstalledGames);
        // Prozess in db speichern
        if (gameProcess is not null)
        {
            Game.UpdateGame(gameProcess);
        }
        return gameProcess;
    }

    private static Game? TryGetForegroundGame(IEnumerable<Game> installedGames)
    {
        uint? foregroundProcessId = GetForegroundProcessId();
        if (foregroundProcessId is null || foregroundProcessId == 0)
        {
            return null;
        }

        try
        {
            using var foregroundProcess = Process.GetProcessById((int)foregroundProcessId.Value);
            var match = TryGetGameFromProcess(foregroundProcess, installedGames);
            if (match is not null)
            {
                mlog($"Spiel im Vordergrund erkannt: {match.Name} | Prozess: {foregroundProcess.ProcessName} | Fenster: {foregroundProcess.MainWindowTitle}");
            }

            return match;
        }
        catch
        {
            return null;
        }
    }

    private static Game? TryGetGameFromProcess(Process process, IEnumerable<Game> installedGames)
    {
        try
        {
            if (process.HasExited)
            {
                return null;
            }

            string? processPath = TryGetProcessPath(process);
            if (string.IsNullOrWhiteSpace(processPath))
            {
                return null;
            }

            string normalizedProcessPath = Path.GetFullPath(processPath);

            foreach (var game in installedGames)
            {
                if (MatchesGamePath(game, normalizedProcessPath))
                {
                    return game;
                }
            }

            string processFileName = Path.GetFileNameWithoutExtension(normalizedProcessPath);
            if (IsJavaProcess(processFileName))
            {
                return TryGetJavaGame(process, installedGames);
            }

            return installedGames.FirstOrDefault(game =>
                string.IsNullOrWhiteSpace(game.ExePath)
                && string.Equals(processFileName, game.ProzessName, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJavaProcess(string processFileName)
    {
        return processFileName.Equals(JAVA_PROCESS_NAME, StringComparison.OrdinalIgnoreCase)
            || processFileName.Equals("java", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Minecraft runs inside javaw.exe, which lives in the Java runtime folder and not in the game folder.
    /// So the game is detected via the command line of the process.
    /// </summary>
    private static Game? TryGetJavaGame(Process process, IEnumerable<Game> installedGames)
    {
        string? commandLine = TryGetCommandLine(process.Id);
        if (string.IsNullOrEmpty(commandLine))
        {
            return null;
        }

        var javaGames = installedGames
            .Where(game => game.ProzessName.Equals(JAVA_PROCESS_NAME, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // 1. The game folder is part of the command line (e.g. --gameDir). The longest path is the most specific one.
        var gameWithFolderMatch = javaGames
            .Where(game => !string.IsNullOrWhiteSpace(game.InstallFolderPath))
            .OrderByDescending(game => game.InstallFolderPath.Length)
            .FirstOrDefault(game => commandLine.Contains(
                game.InstallFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase));
        if (gameWithFolderMatch is not null)
        {
            return gameWithFolderMatch;
        }

        // 2. Clients (Lunar, Badlion, Feather, ...) use their own game folder but still start Minecraft.
        //    The default .minecraft entry (without brackets in the name) is preferred.
        if (IsMinecraftCommandLine(commandLine))
        {
            return javaGames
                .OrderBy(game => game.Name.Contains('(') ? 1 : 0)
                .FirstOrDefault();
        }

        return null;
    }

    private static bool MatchesGamePath(Game game, string processPath)
    {
        if (!string.IsNullOrWhiteSpace(game.ExePath))
        {
            try
            {
                string normalizedExePath = Path.GetFullPath(game.ExePath);
                if (string.Equals(processPath, normalizedExePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch
            {
            }
        }

        if (string.IsNullOrWhiteSpace(game.InstallFolderPath))
        {
            return false;
        }

        try
        {
            string normalizedInstallFolder = Path.GetFullPath(game.InstallFolderPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return processPath.StartsWith(normalizedInstallFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetDirectoryName(processPath), normalizedInstallFolder, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsMinecraftCommandLine(string commandLine)
    {
        return MinecraftCommandLineMarkers.Any(marker =>
            commandLine.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryGetCommandLine(int processId)
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            foreach (var result in searcher.Get())
            {
                return result["CommandLine"] as string;
            }
        }
        catch
        {
        }

        return null;
    }

    private static string? TryGetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            try
            {
                var builder = new StringBuilder(1024);
                uint size = (uint)builder.Capacity;
                return QueryFullProcessImageName(process.Handle, 0, builder, ref size)
                    ? builder.ToString()
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }

    private static uint? GetForegroundProcessId()
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(foregroundWindow, out uint processId);
        return processId == 0 ? null : processId;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref uint lpdwSize);
}