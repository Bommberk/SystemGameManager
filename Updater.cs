namespace SystemGameManager.Service;

using Velopack;
using Velopack.Sources;
using System;
using System.Threading.Tasks;
using SystemGameManager.Handler;

public class Updater
{
    public async Task AutoUpdate()
    {
        try
        {
            var repoUrl = GlobalConfig.Settings.AppConfig.RepositoryUrl;
            var source = new GithubSource(repoUrl, null, true);
            var mgr = new UpdateManager(source);

            if (!mgr.IsInstalled)
                return;

            var update = await mgr.CheckForUpdatesAsync();

            if (update == null)
                return;

            await mgr.DownloadUpdatesAsync(update);
            mgr.ApplyUpdatesAndRestart(update);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, ErrorSeverity.Warning);
        }
    }
}