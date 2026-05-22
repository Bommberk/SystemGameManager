namespace SystemGameManager.Games.Service;

using SystemGameManager.Games.Entity;

class GameAudioService
{
    public const string DEFAULT_MUSIC_APP_NAME = "Spotify";
    protected readonly SystemAudioService systemAudioService = new();

    public void SetAudioSettings(Game.Record? game = null, int gameVolume = Game.GAME_VOLUME_PERCENT, string musicAppName = DEFAULT_MUSIC_APP_NAME, int musicVolume = Game.MUSIC_VOLUME_PERCENT)
    {
        if(game is not null){
            SetMusicValueForOneGame(game, musicVolume);
            SetGameValueForOneGame(game, gameVolume);
        }else{
            SetMusicValueForAllGames(musicVolume);
            SetGameValueForAllGames(gameVolume);
        }
        Game.SaveGames();
        
        SetAudio(game?.Name, gameVolume, musicAppName, musicVolume);
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

    protected void SetAudio(string? gameName = null, int gameVolume = Game.GAME_VOLUME_PERCENT, string musicAppName = DEFAULT_MUSIC_APP_NAME, int musicVolume = Game.MUSIC_VOLUME_PERCENT)
    {
        systemAudioService.SetMusicAudio(musicAppName, musicVolume);
        if(!string.IsNullOrWhiteSpace(gameName))
        {
            systemAudioService.SetGameAudio(gameName, gameVolume);
        }
    }
}