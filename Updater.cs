using Velopack;

namespace Krassheiten.SystemGameManager;

public static class Updater
{
    public static async Task CheckForUpdates()
    {
        var mgr = new UpdateManager(GlobalConfig.GetSettings().AppConfig.RepositoryUrl);

        var update = await mgr.CheckForUpdatesAsync();

        if (update != null)
        {
            await mgr.DownloadUpdatesAsync(update);
            mgr.ApplyUpdatesAndRestart(update);
        }
    }
}