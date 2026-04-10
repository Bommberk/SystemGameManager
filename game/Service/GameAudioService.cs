using System.Diagnostics;
using Krassheiten.SystemGameManager.Entity;
using NAudio.CoreAudioApi;

namespace Krassheiten.SystemGameManager.Service;

class GameAudioService
{
    protected const string DEFAULT_MUSIC_APP_NAME = "Spotify";

    public void SetAudioSettings(Game.Record? game = null, int gameVolume = Game.GAME_VOLUME_PERCENT,string musicAppName = DEFAULT_MUSIC_APP_NAME, int musicVolume = Game.MUSIC_VOLUME_PERCENT)
    {
        if(game is not null){
            SetMusicValueForOneGame(game, musicVolume);
            SetGameValueForOneGame(game, gameVolume);
        }else{
            SetMusicValueForAllGames(musicVolume);
            SetGameValueForAllGames(gameVolume);
        }
    }

    private void SetMusicValueForAllGames(int musicVolume)
    {
        if(Game.InstalledGames is null) return;
        foreach(var game in Game.InstalledGames)
        {
            SetMusicValueForOneGame(game, musicVolume);
        }
    }
    private void SetMusicValueForOneGame(Game.Record game, int musicVolume)
    {
        game.MusicVolumePercent = musicVolume;
    }

    private void SetGameValueForAllGames(int gameVolume)
    {
        if(Game.InstalledGames is null) return;
        foreach(var game in Game.InstalledGames)
        {
            SetGameValueForOneGame(game, gameVolume);
        }
    }
    private void SetGameValueForOneGame(Game.Record game, int gameVolume)
    {
        game.GameVolumePercent = gameVolume;
    }

    private static void SetMusicAudio(string musicAppName, int musicVolumePercent)
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
                    session.SimpleAudioVolume.Volume = musicVolumePercent / 100f;
                }
            }
        }
    }

    private static void SetGameAudio(string gameName, int gameVolumePercent)
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
                if (!string.IsNullOrWhiteSpace(processName) && processName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                {
                    session.SimpleAudioVolume.Volume = gameVolumePercent / 100f;
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

    protected static void SetAudio(string? gameName = null, int gameVolume = Game.GAME_VOLUME_PERCENT,string musicAppName = DEFAULT_MUSIC_APP_NAME, int musicVolume = Game.MUSIC_VOLUME_PERCENT)
    {
        SetMusicAudio(musicAppName, musicVolume);
        if(!string.IsNullOrWhiteSpace(gameName))
        {
            SetGameAudio(gameName, gameVolume);
        }
    }
}