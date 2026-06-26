namespace SystemGameManager.Games.Service;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SystemGameManager.Games.Entity;

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
    private string? previousAudioOutputDeviceId;
    private string? lastAppliedAudioOutputDeviceId;

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
            Game? runningGame = GetRunningOpenGame();
            int? currentMusicAppVolume = systemAudioService.GetMusicAppVolume(DEFAULT_MUSIC_APP_NAME);

            if (runningGame is not null)
            {
                if (!isGameMusicOverrideActive)
                {
                    previousMusicAppVolume = currentMusicAppVolume;
                    previousAudioOutputDeviceId = systemAudioService.GetDefaultAudioOutputDeviceId();
                    isGameMusicOverrideActive = true;
                    mlog($"Merke vorherige Musiklautstärke: {previousMusicAppVolume ?? Game.MUSIC_VOLUME_PERCENT}%");
                }

                string? currentGamePath = runningGame.InstallFolderPath;
                int targetMusicVolume = runningGame.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT;

                string? targetAudioOutputDeviceId = null;
                if (!string.IsNullOrWhiteSpace(runningGame.AudioOutputDevice))
                {
                    targetAudioOutputDeviceId = systemAudioService.GetAudioOutputDeviceIdByName(runningGame.AudioOutputDevice);
                }

                bool audioOutputChanged = !string.IsNullOrWhiteSpace(targetAudioOutputDeviceId)
                    && !string.Equals(lastAppliedAudioOutputDeviceId, targetAudioOutputDeviceId, StringComparison.OrdinalIgnoreCase);

                if (string.Equals(lastAppliedGamePath, currentGamePath, StringComparison.OrdinalIgnoreCase)
                    && lastAppliedMusicVolume == targetMusicVolume
                    && currentMusicAppVolume == targetMusicVolume
                    && !audioOutputChanged)
                {
                    return;
                }

                SetAudio(musicVolume: targetMusicVolume);

                if (audioOutputChanged && !string.IsNullOrWhiteSpace(targetAudioOutputDeviceId))
                {
                    mlog($"Setze Audioausgabe für Spiel '{runningGame.Name}': {runningGame.AudioOutputDevice}");
                    systemAudioService.SetDefaultAudioOutputDevice(targetAudioOutputDeviceId);
                    lastAppliedAudioOutputDeviceId = targetAudioOutputDeviceId;
                }

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

            if (!string.IsNullOrWhiteSpace(previousAudioOutputDeviceId) && !string.IsNullOrWhiteSpace(lastAppliedAudioOutputDeviceId))
            {
                mlog($"Stelle Audioausgabe auf vorheriges Gerät zurück.");
                systemAudioService.SetDefaultAudioOutputDevice(previousAudioOutputDeviceId);
            }

            lastAppliedGamePath = null;
            lastAppliedMusicVolume = restoreMusicVolume;
            lastAppliedAudioOutputDeviceId = null;
            previousMusicAppVolume = null;
            previousAudioOutputDeviceId = null;
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

    private static Game? GetRunningOpenGame()
    {
        if (Game.InstalledGames == null || Game.InstalledGames.Length == 0)
        {
            return null;
        }

        return TryGetForegroundGame(Game.InstalledGames);
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