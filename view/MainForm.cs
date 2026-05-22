namespace SystemGameManager.View;

using System.Drawing;
using System.Windows.Forms;
using SystemGameManager.Games.Controller;
using SystemGameManager.Games.Service;
using SystemGameManager.Pc.Controller;
using SystemGameManager.Service;
using SystemGameManager.View.Service;
using System.Text.Json;
using SystemGameManager.View.Components;

public class MainForm : Form
{
    private readonly Button btnLoadInfo;
    private readonly Label statusLabel;
    private readonly GameViewService gameViewService;
    private readonly PcInfoView pcInfoView;
    private readonly GameInfoView gameInfoView;
    private readonly GameAudioView gameAudioView;
    private GameAudioController? gameAudioController;

    public MainForm()
    {
        Text = $"System & Game Manager (v{ViewService.GetVersionFromReleases()})";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);
        Width = 1180;
        Height = 760;
        BackColor = ColorThemes.GetPrimaryBackgroundColor();
        DoubleBuffered = true;

        gameViewService = new GameViewService();
        pcInfoView = new PcInfoView();
        gameInfoView = new GameInfoView(gameViewService.Artwork, ViewService.OpenGameDirectory);
        gameAudioView = new GameAudioView();

        var toolbar = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(12),
            BackColor = ColorThemes.GetPrimaryBackgroundColor()
        };

        btnLoadInfo = UIHelpers.CreatePrimaryButton("Infos laden", 125);
        btnLoadInfo.Dock = DockStyle.Left;

        statusLabel = new Label()
        {
            Text = "Bereit",
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 7, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };

        var tabs = new TabControl()
        {
            Dock = DockStyle.Fill,
            Padding = new Point(18, 8)
        };

        tabs.TabPages.Add(pcInfoView.CreateTab());
        tabs.TabPages.Add(gameInfoView.CreateTab());
        tabs.TabPages.Add(gameAudioView.CreateTab());

        btnLoadInfo.Click += BtnLoadInfo_Click;
        Shown += async (_, _) => await LoadInfoAsync();

        toolbar.Controls.Add(statusLabel);
        toolbar.Controls.Add(btnLoadInfo);

        Controls.Add(tabs);
        Controls.Add(toolbar);

        pcInfoView.ShowLoadingState();
        gameInfoView.ShowLoadingState();
        gameAudioView.ShowLoadingState();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            gameAudioController?.Dispose();
            gameViewService.Dispose();
        }

        base.Dispose(disposing);
    }

    private async void BtnLoadInfo_Click(object? sender, EventArgs e)
    {
        await LoadInfoAsync();
    }

    private async Task LoadInfoAsync()
    {
        btnLoadInfo.Enabled = false;
        statusLabel.Text = "Lade Informationen...";
        pcInfoView.ShowLoadingState();
        gameInfoView.ShowLoadingState();
        gameAudioView.ShowLoadingState();

        try
        {
            var viewData = await Task.Run(BuildViewData);
            pcInfoView.ShowSystemText(viewData.SystemText);
            gameInfoView.Populate(viewData.GameManager);
            gameAudioView.RefreshGames();
            gameAudioController ??= new GameAudioController();
            statusLabel.Text = "Informationen geladen.";
        }
        catch (Exception ex)
        {
            pcInfoView.ShowError(ex.Message);
            gameInfoView.ShowErrorState(ex.Message);
            gameAudioView.ShowErrorState(ex.Message);
            statusLabel.Text = "Fehler beim Laden.";
        }
        finally
        {
            btnLoadInfo.Enabled = true;
        }
    }


    private MainViewData BuildViewData()
    {
        var pcInfo = new PcInfoController();
        _ = new GameInfoController();

        return new MainViewData(
            pcInfoView.BuildSystemText(pcInfo),
            gameViewService.BuildViewData());
    }

    private sealed record MainViewData(string SystemText, GameViewService.GameManagerViewData GameManager);
}