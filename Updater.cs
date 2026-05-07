namespace Krassheiten.SystemGameManager.Service;

using Velopack;

public class Updater
{
    public async Task AutoUpdate()
    {
        var repoUrl = GlobalConfig.Settings.AppConfig.RepositoryUrl;
        var mgr = new UpdateManager(repoUrl);

        if (!mgr.IsInstalled)
            return;

        var update = await mgr.CheckForUpdatesAsync();

        if (update == null)
            return;

        await mgr.DownloadUpdatesAsync(update);
        mgr.ApplyUpdatesAndRestart(update);
    }
}
// test