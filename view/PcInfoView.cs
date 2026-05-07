using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Controller;

namespace Krassheiten.SystemGameManager.View;

internal sealed class PcInfoView
{
    private readonly RichTextBox systemOutput = CreateReadOnlyOutputBox();

    public TabPage CreateTab()
    {
        var tab = new TabPage("SystemManager")
        {
            BackColor = Color.FromArgb(245, 247, 250)
        };

        var wrapper = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        wrapper.Controls.Add(systemOutput);
        tab.Controls.Add(wrapper);
        return tab;
    }

    public void ShowLoadingState() => systemOutput.Text = "Bitte warten...";

    public void ShowSystemText(string text) => systemOutput.Text = text;

    public void ShowError(string message) => systemOutput.Text = $"Fehler beim Laden:\r\n{message}";

    public string BuildSystemText(PcInfoController pcInfo)
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

    private static RichTextBox CreateReadOnlyOutputBox()
    {
        return new RichTextBox()
        {
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10F),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(31, 41, 55),
            WordWrap = false,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static void AppendField(StringBuilder builder, string label, string? value)
    {
        builder.AppendLine($"{label + ":",-14} {value ?? "nicht verfügbar"}");
    }
}