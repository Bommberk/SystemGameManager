namespace Krassheiten.SystemGameManager.Service;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Krassheiten.SystemGameManager.Entity;
using NAudio.CoreAudioApi;

class GameAudioMonitoringService
: GameAudioService
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
                    mlog($"Merke vorherige Musiklautstärke: {previousMusicAppVolume ?? DEFAULT_MUSIC_VOLUME_PERCENT}%");
                }

                string? currentGamePath = runningGame.InstallFolderPath;
                int targetMusicVolume = runningGame.MusicVolume;

                if (string.Equals(lastAppliedGamePath, currentGamePath, StringComparison.OrdinalIgnoreCase)
                    && lastAppliedMusicVolume == targetMusicVolume
                    && currentMusicAppVolume == targetMusicVolume)
                {
                    return;
                }

                SetMusicAudio(musicVolumePercent: targetMusicVolume);
                lastAppliedGamePath = currentGamePath;
                lastAppliedMusicVolume = targetMusicVolume;
                return;
            }

            if (!isGameMusicOverrideActive)
            {
                return;
            }

            int restoreMusicVolume = previousMusicAppVolume ?? DEFAULT_MUSIC_VOLUME_PERCENT;
            mlog($"Kein Spiel mehr offen. Musiklautstärke wird auf {restoreMusicVolume}% zurückgesetzt.");
            SetMusicAudio(musicVolumePercent: restoreMusicVolume);

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

    private static Game.Record? GetRunningOpenGame()
    {
        if (Game.InstalledGames == null || Game.InstalledGames.Length == 0)
        {
            return null;
        }

        uint? foregroundProcessId = GetForegroundProcessId();
        if (foregroundProcessId is null || foregroundProcessId == 0)
        {
            return null;
        }

        try
        {
            using var foregroundProcess = Process.GetProcessById((int)foregroundProcessId.Value);
            if (foregroundProcess.HasExited || string.IsNullOrWhiteSpace(foregroundProcess.MainWindowTitle))
            {
                return null;
            }

            string? processPath = TryGetProcessPath(foregroundProcess);
            if (string.IsNullOrWhiteSpace(processPath))
            {
                return null;
            }

            foreach (var game in Game.InstalledGames)
            {
                if (string.IsNullOrWhiteSpace(game.InstallFolderPath))
                {
                    continue;
                }

                string installFolderPath = Path.GetFullPath(game.InstallFolderPath);
                if (processPath.StartsWith(installFolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    mlog($"Spiel im Vordergrund erkannt: {game.Name} | Prozess: {foregroundProcess.ProcessName} | Fenster: {foregroundProcess.MainWindowTitle}");
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

    private static string? TryGetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
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