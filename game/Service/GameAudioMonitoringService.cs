namespace Krassheiten.SystemGameManager.Service;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Krassheiten.SystemGameManager.Entity;
using NAudio.CoreAudioApi;

class GameAudioMonitoringService
: GameAudioService, IDisposable
{
    private const int AUDIO_CHECK_INTERVAL_MS = 2000;

    private System.Threading.Timer? audioMonitorTimer;
    private int isCheckingAudio;
    private string? lastAppliedGamePath;
    private int? lastAppliedMusicVolume;
    private int? previousMusicAppVolume;
    private bool isGameMusicOverrideActive;

    public void StartAudioMonitoring(int intervalMs = AUDIO_CHECK_INTERVAL_MS)
    {
        int effectiveInterval = Math.Max(500, intervalMs);

        audioMonitorTimer?.Dispose();
        audioMonitorTimer = new System.Threading.Timer(_ =>
        {
            try
            {
                SetAudioWhenGameStarts();
            }
            catch
            {
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(effectiveInterval));
    }

    public void SetAudioWhenGameStarts()
    {
        if (Interlocked.Exchange(ref isCheckingAudio, 1) == 1)
        {
            return;
        }

        try
        {
            Game.Record? runningGame = GetRunningOpenGame();
            int? currentMusicAppVolume = GetMusicAppVolume();

            if (runningGame is not null)
            {
                if (!isGameMusicOverrideActive)
                {
                    previousMusicAppVolume = currentMusicAppVolume;
                    isGameMusicOverrideActive = true;
                    mlog($"Merke vorherige Musiklautstärke: {previousMusicAppVolume ?? Game.MUSIC_VOLUME_PERCENT}%");
                }

                string? currentGamePath = runningGame.InstallFolderPath;
                int targetMusicVolume = runningGame.MusicVolumePercent;

                if (string.Equals(lastAppliedGamePath, currentGamePath, StringComparison.OrdinalIgnoreCase)
                    && lastAppliedMusicVolume == targetMusicVolume
                    && currentMusicAppVolume == targetMusicVolume)
                {
                    return;
                }

                SetAudio(musicVolume: targetMusicVolume);
                lastAppliedGamePath = currentGamePath;
                lastAppliedMusicVolume = targetMusicVolume;
                return;
            }

            if (!isGameMusicOverrideActive)
            {
                return;
            }

            int restoreMusicVolume = previousMusicAppVolume ?? Game.MUSIC_VOLUME_PERCENT;
            mlog($"Kein Spiel mehr offen. Musiklautstärke wird auf {restoreMusicVolume}% zurückgesetzt.");
            SetAudio(musicVolume: restoreMusicVolume);

            lastAppliedGamePath = null;
            lastAppliedMusicVolume = restoreMusicVolume;
            previousMusicAppVolume = null;
            isGameMusicOverrideActive = false;
        }
        finally
        {
            Interlocked.Exchange(ref isCheckingAudio, 0);
        }
    }

    public void StopAudioMonitoring()
    {
        audioMonitorTimer?.Dispose();
        audioMonitorTimer = null;
    }

    public void Dispose()
    {
        StopAudioMonitoring();
    }

    private static Game.Record? GetRunningOpenGame()
    {
        if (Game.InstalledGames == null || Game.InstalledGames.Length == 0)
        {
            return null;
        }

        return TryGetForegroundGame(Game.InstalledGames);
    }

    private static Game.Record? TryGetForegroundGame(IEnumerable<Game.Record> installedGames)
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

    private static Game.Record? TryGetGameFromProcess(Process process, IEnumerable<Game.Record> installedGames)
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
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static bool MatchesGamePath(Game.Record game, string processPath)
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

    private int? GetMusicAppVolume(string musicAppName = DEFAULT_MUSIC_APP_NAME)
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var device in devices)
        {
            var sessions = device.AudioSessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                var processName = GetProcessName(session.GetProcessID);
                if (!string.IsNullOrWhiteSpace(processName) && processName.Equals(musicAppName, StringComparison.OrdinalIgnoreCase))
                {
                    return (int)Math.Round(session.SimpleAudioVolume.Volume * 100f);
                }
            }
        }

        return null;
    }
}