using System.Drawing;
using System.Windows.Forms;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class LauncherBadgeControl
{
    public static Control Create(string title, string subtitle)
    {
        var shell = new HoverShadowPanel()
        {
            Width = 330,
            Height = 104,
            Margin = new Padding(0, 0, 14, 14)
        };

        var body = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.CardBackground,
            Padding = new Padding(14),
            Margin = new Padding(0),
            ColumnCount = 2,
            RowCount = 2
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        UIHelpers.SetRoundedRegion(body, 18);

        body.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0, 4, 0, 6)
        }, 0, 0);

        body.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            MaximumSize = new Size(250, 0),
            ForeColor = UIHelpers.TextSecondaryColor,
            Margin = new Padding(0)
        }, 0, 1);

        body.Controls.Add(new Label
        {
            Text = "›",
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Regular),
            ForeColor = UIHelpers.AccentColor,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            Margin = new Padding(8, 6, 0, 0)
        }, 1, 0);
        body.SetRowSpan(body.Controls[^1], 2);

        shell.Controls.Add(body);
        HoverShadowPanel.AddHoverEffect(shell, body);
        return shell;
    }
}
