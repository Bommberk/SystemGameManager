namespace SystemGameManager.View.Service;

using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using SystemGameManager.Games.Entity;
using SystemGameManager.View.Components;
using SystemGameManager.View.Pages;

class GameManagerViewService
{
    private GameManager gameManagerPage;

    public GameManagerViewService(GameManager gameManagerPage)
    {
        this.gameManagerPage = gameManagerPage;
    }

    public async Task RefreshGameAndLauncherInfoAsync()
    {
        await Task.Run(() => new GameInfoController());
        gameManagerPage.RefreshGameAndLauncherInfo();
    }

    public TableLayoutPanel GetNewSection(string title)
    {
        var section = new TableLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 12),
        };
        var sectionTitlePanel = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
        };
        var sectionTitle = new Label()
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        sectionTitlePanel.Controls.Add(sectionTitle);
        section.Controls.Add(sectionTitlePanel);
        return section;
    }
    public Launcher.Record[] GetInstalledLauncher()
    {
        var launchers = Launcher.GetLaunchers();
        return launchers ?? Array.Empty<Launcher.Record>();
    }

    public Game.Record[] GetInstalledGames()
    {
        var games = Game.GetGames();
        return games ?? Array.Empty<Game.Record>();
    }
    public List<string> GetAudioOutputDeviceNames()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator
                .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(device => device.FriendlyName)
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}