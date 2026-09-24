namespace SystemGameManager.Games.Service;

using System.Threading;
using SystemGameManager.AudioManager.Controller;
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
    private readonly AudioManagerController audioManagerController = new();

    /// <summary>
    /// true, sobald im Audio des aktuell laufenden Spiels Sprache erkannt wird. Läuft pro Spiel,
    /// da immer nur das gerade aktive (im Vordergrund laufende) Spiel überwacht wird.
    /// </summary>
    public bool IsCurrentlySpeech => audioManagerController.IsCurrentlySpeech;

    public void StartAudioMonitoring(int intervalMs = AUDIO_CHECK_INTERVAL_MS)
    {
        int effectiveInterval = Math.Max(500, intervalMs);

        audioMonitorTimer?.Dispose();
        audioMonitorTimer = new System.Threading.Timer(_ =>
        {
            try
            {
                // Einmal pro Tick ermitteln, welches Spiel gerade aktiv ist, damit Lautstärke-Anpassung
                // und Aufnahme-/Sprecherkennungs-Überwachung konsistent für dasselbe Spiel laufen
                // (kein hartkodiertes Programm, sondern immer das aktuell aktive Spiel).
                Game? runningGame = GetGameProcess.GetRunningOpenGame();
                SetAudioWhenGameStarts(runningGame);
                audioManagerController.StartCaptureMonitoring(runningGame?.ProzessName);
            }
            catch
            {
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(effectiveInterval));
    }

    public void SetAudioWhenGameStarts(Game? runningGame = null)
    {
        if (Interlocked.Exchange(ref isCheckingAudio, 1) == 1)
        {
            return;
        }

        try
        {
            runningGame ??= GetGameProcess.GetRunningOpenGame();
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
        audioManagerController.Dispose();
    }
}