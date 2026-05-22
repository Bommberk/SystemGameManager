using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Controller;
using Krassheiten.SystemGameManager.Service;
using Krassheiten.SystemGameManager.View;
using Krassheiten.SystemGameManager.View.Components;
using System.Text.Json;

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
        Text = $"System & Game Manager (v{GetVersionFromReleases()})";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1280, 820);
        Width = 1360;
        Height = 860;
        BackColor = UIHelpers.WindowBackground;
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 9F);

        gameViewService = new GameViewService();
        pcInfoView = new PcInfoView();
        gameInfoView = new GameInfoView(gameViewService.Artwork, OpenGameDirectory);
        gameAudioView = new GameAudioView();

        var toolbar = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(12),
            BackColor = UIHelpers.SurfaceBackground
        };

        btnLoadInfo = UIHelpers.CreatePrimaryButton("Infos laden", 132);
        btnLoadInfo.Dock = DockStyle.Left;

        statusLabel = new Label()
        {
            Text = "Bereit",
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 7, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UIHelpers.TextSecondaryColor
        };

        var tabs = new TabControl()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.WindowBackground,
            Padding = new Point(20, 12),
            DrawMode = TabDrawMode.OwnerDrawFixed,
            ItemSize = new Size(180, 40),
            SizeMode = TabSizeMode.Fixed
        };
        tabs.DrawItem += (_, e) => DrawTab(tabs, e);
        tabs.SelectedIndexChanged += (_, _) => tabs.Invalidate();

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

    private static void DrawTab(TabControl tabControl, DrawItemEventArgs e)
    {
        var page = tabControl.TabPages[e.Index];
        var bounds = e.Bounds;
        bool isSelected = e.Index == tabControl.SelectedIndex;

        using var backgroundBrush = new SolidBrush(UIHelpers.SurfaceBackground);
        e.Graphics.FillRectangle(backgroundBrush, bounds);

        var textBounds = Rectangle.Inflate(bounds, -10, -2);
        using var textFont = new Font("Segoe UI", 10F, isSelected ? FontStyle.Bold : FontStyle.Regular);
        TextRenderer.DrawText(
            e.Graphics,
            page.Text,
            textFont,
            textBounds,
            isSelected ? UIHelpers.TextPrimaryColor : UIHelpers.TextSecondaryColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        using var borderPen = new Pen(UIHelpers.BorderColor);
        e.Graphics.DrawLine(borderPen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

        if (!isSelected)
        {
            return;
        }

        using var accentBrush = new SolidBrush(UIHelpers.AccentColor);
        e.Graphics.FillRectangle(accentBrush, bounds.Left + 12, bounds.Bottom - 3, bounds.Width - 24, 3);
    }

    private static string GetVersionFromReleases()
    {
        try
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                if (version.Revision > 0)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        catch
        {
            // Fehler ignorieren
        }
        
        return "Unknown";
    }

    private sealed record MainViewData(string SystemText, GameViewService.GameManagerViewData GameManager);
}
