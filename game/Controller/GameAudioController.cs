using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.Controller;

class GameAudioController
{
    private readonly GameAudioService gameAudioService = new();
    private readonly GameAudioMonitoringService gameMonitoringService = new();

    public GameAudioController()
    {
        gameAudioService.SetGameAudioSettings();
        gameMonitoringService.StartAudioMonitoring();
    }
}