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
    private readonly TabControl tabs;
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

        var root = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.WindowBackground,
            ColumnCount = 2,
            RowCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var sidebar = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.SidebarBackground,
            Padding = new Padding(10, 76, 10, 14)
        };

        var sidebarActions = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            AutoSize = true
        };

        var menuButton = CreateSidebarButton("☰");
        var homeButton = CreateSidebarButton("⌗", true);
        var settingsButton = CreateSidebarButton("⚙");
        var infoButton = CreateSidebarButton("i");

        menuButton.Margin = new Padding(0, 0, 0, 16);
        homeButton.Margin = new Padding(0, 0, 0, 16);
        settingsButton.Margin = new Padding(0, 0, 0, 16);
        infoButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

        sidebarActions.Controls.Add(menuButton);
        sidebarActions.Controls.Add(homeButton);
        sidebarActions.Controls.Add(settingsButton);
        sidebar.Controls.Add(sidebarActions);

        var infoHost = new Panel()
        {
            Dock = DockStyle.Bottom,
            Height = 44
        };
        infoHost.Controls.Add(infoButton);
        sidebar.Controls.Add(infoHost);

        var shell = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.WindowBackground,
            ColumnCount = 1,
            RowCount = 2
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 86,
            Padding = new Padding(16, 18, 16, 10),
            BackColor = UIHelpers.SurfaceBackground
        };

        btnLoadInfo = UIHelpers.CreatePrimaryButton("Infos laden", 132);
        btnLoadInfo.Dock = DockStyle.Left;
        btnLoadInfo.Text = "ⓘ  Infos laden";

        statusLabel = new Label()
        {
            Text = "Bereit",
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 7, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UIHelpers.TextSecondaryColor
        };

        tabs = new TabControl()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.WindowBackground,
            Padding = new Point(22, 12),
            DrawMode = TabDrawMode.OwnerDrawFixed,
            ItemSize = new Size(210, 42),
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

        shell.Controls.Add(toolbar, 0, 0);
        shell.Controls.Add(tabs, 0, 1);

        root.Controls.Add(sidebar, 0, 0);
        root.Controls.Add(shell, 1, 0);

        Controls.Add(root);

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

    private static Button CreateSidebarButton(string text, bool isActive = false)
    {
        var button = new Button()
        {
            Text = text,
            Width = 50,
            Height = 50,
            FlatStyle = FlatStyle.Flat,
            BackColor = isActive ? UIHelpers.SidebarActiveBackground : Color.Transparent,
            ForeColor = isActive ? UIHelpers.TextPrimaryColor : Color.FromArgb(223, 231, 210),
            Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = UIHelpers.SidebarActiveBackground;
        button.FlatAppearance.MouseOverBackColor = UIHelpers.SidebarActiveBackground;
        return button;
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
