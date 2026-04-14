using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.Controller;

internal sealed class GameAudioController : IDisposable
{
    private readonly GameAudioMonitoringService gameMonitoringService = new();
    private bool disposed;

    public GameAudioController()
    {
        gameMonitoringService.StartAudioMonitoring();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        gameMonitoringService.Dispose();
        disposed = true;
    }
}