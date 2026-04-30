using Velopack;

namespace Krassheiten.SystemGameManager;

async static Task CheckForUpdates()
{
    var mgr = new UpdateManager("https://github.com/");

    var update = await mgr.CheckForUpdatesAsync();

    if (update != null)
    {
        await mgr.DownloadUpdatesAsync(update);
        mgr.ApplyUpdatesAndRestart(update);
    }
}