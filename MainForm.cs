using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Controller;
using Krassheiten.SystemGameManager.Entity;

namespace Krassheiten.SystemGameManager;

public class MainForm : Form
{
    private readonly Button btnLoadInfo;
    private readonly RichTextBox systemOutput;
    private readonly RichTextBox gameOutput;
    private readonly Label statusLabel;

    public MainForm()
    {
        Text = "System & Game Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Width = 1080;
        Height = 720;

        var toolbar = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 56,
            Padding = new Padding(12)
        };

        btnLoadInfo = new Button()
        {
            Text = "Infos laden",
            Width = 120,
            Height = 30,
            Dock = DockStyle.Left
        };

        statusLabel = new Label()
        {
            Text = "Bereit",
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 6, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var tabs = new TabControl()
        {
            Dock = DockStyle.Fill
        };

        systemOutput = CreateReadOnlyOutputBox();
        gameOutput = CreateReadOnlyOutputBox();

        tabs.TabPages.Add(CreateSystemManagerTab());
        tabs.TabPages.Add(CreateGameManagerTab());
        tabs.TabPages.Add(CreateGameAudioManagerTab());

        btnLoadInfo.Click += BtnLoadInfo_Click;
        Shown += async (_, _) => await LoadInfoAsync();

        toolbar.Controls.Add(statusLabel);
        toolbar.Controls.Add(btnLoadInfo);

        Controls.Add(tabs);
        Controls.Add(toolbar);
    }

    private TabPage CreateSystemManagerTab()
    {
        var tab = new TabPage("SystemManager");
        var wrapper = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        wrapper.Controls.Add(systemOutput);
        tab.Controls.Add(wrapper);
        return tab;
    }

    private TabPage CreateGameManagerTab()
    {
        var tab = new TabPage("Game-Manager");
        var wrapper = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        wrapper.Controls.Add(gameOutput);
        tab.Controls.Add(wrapper);
        return tab;
    }

    private static TabPage CreateGameAudioManagerTab()
    {
        var tab = new TabPage("Game-Audio-Manager");

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label()
        {
            Text = "Audio-Steuerung für Spiele und Musik",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };

        var autoMode = new CheckBox()
        {
            Text = "Automatische Audio-Anpassung aktivieren",
            AutoSize = true,
            Checked = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        var volumeGroup = new GroupBox()
        {
            Text = "Audio-Level",
            Dock = DockStyle.Top,
            Height = 170,
            Padding = new Padding(12)
        };

        var volumeLayout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };

        volumeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        volumeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        volumeLayout.Controls.Add(new Label { Text = "Spiel-Lautstärke", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        volumeLayout.Controls.Add(new TrackBar { Minimum = 0, Maximum = 100, Value = 80, TickFrequency = 10, Dock = DockStyle.Fill }, 1, 0);
        volumeLayout.Controls.Add(new Label { Text = "Musik-Lautstärke", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        volumeLayout.Controls.Add(new TrackBar { Minimum = 0, Maximum = 100, Value = 45, TickFrequency = 10, Dock = DockStyle.Fill }, 1, 1);

        volumeGroup.Controls.Add(volumeLayout);

        var note = new Label()
        {
            Text = "Die Audio-Logik ist als Oberfläche vorbereitet und kann jetzt an `GameAudioController` angebunden werden.",
            Dock = DockStyle.Top,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 12, 0, 0)
        };

        layout.Controls.Add(title);
        layout.Controls.Add(autoMode);
        layout.Controls.Add(volumeGroup);
        layout.Controls.Add(note);

        tab.Controls.Add(layout);
        return tab;
    }

    private async void BtnLoadInfo_Click(object? sender, EventArgs e)
    {
        await LoadInfoAsync();
    }

    private async Task LoadInfoAsync()
    {
        btnLoadInfo.Enabled = false;
        statusLabel.Text = "Lade Informationen...";
        systemOutput.Text = "Bitte warten...";
        gameOutput.Text = "Bitte warten...";

        try
        {
            var viewData = await Task.Run(BuildViewData);
            systemOutput.Text = viewData.SystemText;
            gameOutput.Text = viewData.GameText;
            statusLabel.Text = "Informationen geladen.";
        }
        catch (Exception ex)
        {
            var errorText = $"Fehler beim Laden:\r\n{ex.Message}";
            systemOutput.Text = errorText;
            gameOutput.Text = errorText;
            statusLabel.Text = "Fehler beim Laden.";
        }
        finally
        {
            btnLoadInfo.Enabled = true;
        }
    }

    private static ViewData BuildViewData()
    {
        var pcInfo = new PcInfoController();
        _ = new GameInfoController();

        return new ViewData(BuildSystemText(pcInfo), BuildGameText());
    }

    private static string BuildSystemText(PcInfoController pcInfo)
    {
        var builder = new StringBuilder();

        builder.AppendLine("=== PC INFO ===");
        AppendField(builder, "PC-Name", pcInfo.PcName);
        AppendField(builder, "IPv4 Lokal", pcInfo.LocalIpv4Address);
        AppendField(builder, "IPv6 Lokal", pcInfo.LocalIpv6Address);
        AppendField(builder, "IPv4 Public", pcInfo.PublicIpv4Address);
        AppendField(builder, "IPv6 Public", pcInfo.PublicIpv6Address);
        AppendField(builder, "MAC", pcInfo.MACAddress);
        AppendField(builder, "OS", pcInfo.OSVersion);
        AppendField(builder, "CPU", pcInfo.CPU);
        AppendField(builder, "RAM", pcInfo.RAM);
        AppendField(builder, "GPU", pcInfo.GPU);
        AppendField(builder, "Akku", pcInfo.Battery);

        builder.AppendLine();
        builder.AppendLine("=== SPEICHER ===");
        AppendField(builder, "Gesamt", $"{pcInfo.Storage.TotalUsedGb}/{pcInfo.Storage.TotalSizeGb} GB (frei: {pcInfo.Storage.TotalFreeGb} GB)");

        foreach (var drive in pcInfo.Storage.Drives)
        {
            var driveName = string.IsNullOrWhiteSpace(drive.Label)
                ? drive.Letter
                : $"{drive.Letter} ({drive.Label})";

            var driveValue = $"{drive.UsedGb}/{drive.SizeGb} GB (frei: {drive.FreeGb} GB)";

            if (drive.VirtualHostDrive is not null)
            {
                driveValue += $" [virtuell auf {drive.VirtualHostDrive}]";
            }
            else if (drive.VirtualReservedGb > 0 && drive.VirtualDriveNames.Count > 0)
            {
                driveValue += $" (-{drive.VirtualReservedGb} GB für {string.Join(", ", drive.VirtualDriveNames)})";
            }

            builder.AppendLine($"- {driveName}: {driveValue}");
        }

        return builder.ToString();
    }

    private static string BuildGameText()
    {
        var builder = new StringBuilder();

        builder.AppendLine("=== LAUNCHER ===");
        if (Launcher.InstalledLaunchers is { Length: > 0 } launchers)
        {
            foreach (var launcher in launchers.OrderBy(launcher => launcher.DisplayName))
            {
                builder.AppendLine($"- {launcher.DisplayName}");
                builder.AppendLine($"  Installationspfad: {launcher.InstallPath}");
                builder.AppendLine($"  Spielordner:       {launcher.GameFolderPath}");
            }
        }
        else
        {
            builder.AppendLine("Keine Launcher gefunden.");
        }

        builder.AppendLine();
        builder.AppendLine("=== SPIELE ===");
        if (Game.InstalledGames is { Length: > 0 } games)
        {
            foreach (var game in games.OrderBy(game => game.Name))
            {
                builder.AppendLine($"- {game.Name}");
                builder.AppendLine($"  Pfad: {game.InstallPath}");
            }
        }
        else
        {
            builder.AppendLine("Keine Spiele gefunden.");
        }

        return builder.ToString();
    }

    private static RichTextBox CreateReadOnlyOutputBox()
    {
        return new RichTextBox()
        {
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10F),
            BackColor = SystemColors.Window,
            WordWrap = false,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static void AppendField(StringBuilder builder, string label, string? value)
    {
        builder.AppendLine($"{label + ":",-14} {value ?? "nicht verfügbar"}");
    }

    private sealed record ViewData(string SystemText, string GameText);
}