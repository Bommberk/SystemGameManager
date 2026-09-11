namespace SystemGameManager.Games.Service;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SystemGameManager.Games.Entity;

class GetGameProcess
{
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
                    game.ProzessName = process.ProcessName;
                    return game;
                }
            }
        }
        catch
        {
            return null;
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