namespace SystemGameManager.View;

using System.Drawing;
using System.Windows.Forms;
using SystemGameManager.Games.Controller;
using SystemGameManager.Games.Service;
using SystemGameManager.View.Service;
using SystemGameManager.View.Components;

public class MainForm : Form
{
    private readonly Button btnLoadInfo;
    private readonly Label statusLabel;
    private readonly GameViewService gameViewService;
    private readonly GameInfoView gameInfoView;
    private readonly GameAudioView gameAudioView;
    private GameAudioController? gameAudioController;
    private readonly ViewService viewService = new ViewService();

    public MainForm()
    {
        Text = $"System & Game Manager (v{ViewService.GetVersionFromReleases()})";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);
        Width = 1180;
        Height = 760;
        BackColor = ColorThemes.GetPrimaryBackgroundColor();
        DoubleBuffered = true;
        
        Color test = ColorThemes.GetSecondaryBackgroundColor();

        var sideBar = new Panel()
        {
            Dock = DockStyle.Left,
            Width = 60,
            Padding = new Padding(12),
            BackColor = ColorThemes.GetSecondaryBackgroundColor()
        };
        this.Controls.Add(sideBar);

        var containerWrapper = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(60+20, 20, 20, 20),
            BackColor = Color.Transparent
        };

        var container = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = ColorThemes.GetPrimaryBackgroundColor()
        };
        containerWrapper.Controls.Add(container);
        this.Controls.Add(containerWrapper);


        gameViewService = new GameViewService();
        gameInfoView = new GameInfoView(gameViewService.Artwork, ViewService.OpenGameDirectory);
        gameAudioView = new GameAudioView();

        var toolbar = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(12),
            BackColor = Color.Transparent
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
        
        btnLoadInfo.Click += BtnLoadInfo_Click;
        Shown += async (_, _) => await LoadInfoAsync();

        toolbar.Controls.Add(statusLabel);
        toolbar.Controls.Add(btnLoadInfo);
        container.Controls.Add(toolbar);

        return;

        var tabs = new ThemedTabControl()
        {
            Dock = DockStyle.Fill,
            Padding = new Point(18, 8)
        };

        tabs.TabPages.Add(gameInfoView.CreateTab());
        tabs.TabPages.Add(gameAudioView.CreateTab());

        Controls.Add(tabs);
        gameInfoView.ShowLoadingState();
        gameAudioView.ShowLoadingState();
    }

    private async void BtnLoadInfo_Click(object? sender, EventArgs e)
    {
        await LoadInfoAsync();
    }

    private async Task LoadInfoAsync()
    {
        btnLoadInfo.Enabled = false;
        statusLabel.Text = "Lade Informationen...";
        gameInfoView.ShowLoadingState();
        gameAudioView.ShowLoadingState();

        try
        {
            var viewData = await Task.Run(BuildViewData);
            gameInfoView.Populate(viewData.GameManager);
            gameAudioView.RefreshGames();
            gameAudioController ??= new GameAudioController();
            statusLabel.Text = "Informationen geladen.";
        }
        catch (Exception ex)
        {
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
        _ = new GameInfoController();

        return new MainViewData(gameViewService.BuildViewData());
    }

    private sealed record MainViewData(GameViewService.GameManagerViewData GameManager);
}