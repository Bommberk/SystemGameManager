namespace Krassheiten.SystemGameManager.Service;

using System.Diagnostics;
using Krassheiten.SystemGameManager.Entity;
using NAudio.CoreAudioApi;

class GameAudioService
{
    protected const string DEFAULT_MUSIC_APP_NAME = "Spotify";
    protected const int DEFAULT_MUSIC_VOLUME_PERCENT = 50;

    public void SetGameAudioSettings(int gameVolumePercent = 100, int musicVolumePercent = 50, Game.Record? selectedGame = null)
    {
        if (selectedGame is not null)
        {
            SetMusicAudioForGame(selectedGame, musicVolumePercent);
        }
        else
        {
            SetMusicAudioForAllGames(musicVolumePercent);
        }
    }

    private void SetMusicAudioForGame(Game.Record game, int musicVolumePercent)
    {
        if (Game.InstalledGames == null) return;

        int clampedMusicVolume = Math.Clamp(musicVolumePercent, 0, 100);

        Game.InstalledGames =
        [
            .. Game.InstalledGames.Select(installedGame =>
                string.Equals(installedGame.InstallFolderPath, game.InstallFolderPath, StringComparison.OrdinalIgnoreCase)
                    ? installedGame with { MusicVolume = clampedMusicVolume }
                    : installedGame)
        ];
    }

    private void SetMusicAudioForAllGames(int musicVolumePercent)
    {
        if (Game.InstalledGames == null) return;

        foreach (var game in Game.InstalledGames)
        {
            SetMusicAudioForGame(game, musicVolumePercent);
        }
    }

    protected static void SetMusicAudio(string musicAppName = DEFAULT_MUSIC_APP_NAME, int musicVolumePercent = 50)
    {
        int clampedMusicVolume = Math.Clamp(musicVolumePercent, 0, 100);

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
                    session.SimpleAudioVolume.Volume = clampedMusicVolume / 100f;
                }
            }
        }
    }

    protected static string? GetProcessName(uint processId)
    {
        try
        {
            if (processId == 0)
            {
                return null;
            }

            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }

}