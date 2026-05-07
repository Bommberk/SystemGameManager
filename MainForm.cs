using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Controller;
using Krassheiten.SystemGameManager.Service;
using Krassheiten.SystemGameManager.View;

namespace Krassheiten.SystemGameManager;

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
        Text = "System & Game Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);
        Width = 1180;
        Height = 760;
        BackColor = Color.FromArgb(245, 247, 250);
        DoubleBuffered = true;

        gameViewService = new GameViewService();
        pcInfoView = new PcInfoView();
        gameInfoView = new GameInfoView(gameViewService.Artwork, OpenGameDirectory);
        gameAudioView = new GameAudioView();

        var toolbar = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(12),
            BackColor = Color.White
        };

        btnLoadInfo = CreatePrimaryButton("Infos laden", 125);
        btnLoadInfo.Dock = DockStyle.Left;

        statusLabel = new Label()
        {
            Text = "Bereit",
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 7, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(55, 65, 81)
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

    private void OpenGameDirectory(string path)
    {
        if (!gameViewService.TryOpenDirectory(path, out var errorMessage))
        {
            MessageBox.Show(errorMessage, "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    private static Button CreatePrimaryButton(string text, int width)
    {
        var button = new Button()
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(67, 56, 202);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(99, 102, 241);
        return button;
    }

    private sealed record MainViewData(string SystemText, GameViewService.GameManagerViewData GameManager);
}