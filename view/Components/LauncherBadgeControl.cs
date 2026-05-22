using System.Drawing;
using System.Windows.Forms;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class LauncherBadgeControl
{
    public static Control Create(string title, string subtitle)
    {
        var shell = new HoverShadowPanel()
        {
            Width = 266,
            Height = 104,
            Margin = new Padding(0, 0, 14, 14)
        };

        var body = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.CardBackground,
            Padding = new Padding(14, 12, 14, 12),
            Margin = new Padding(0),
            ColumnCount = 3,
            RowCount = 2
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        UIHelpers.SetRoundedRegion(body, 18);

        var avatar = new Panel()
        {
            Width = 42,
            Height = 42,
            BackColor = Color.FromArgb(58, 63, 45),
            Margin = new Padding(0, 4, 12, 0)
        };
        UIHelpers.SetRoundedRegion(avatar, 12);

        avatar.Controls.Add(new Label
        {
            Text = GetLauncherMonogram(title),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UIHelpers.AccentColor,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        });

        body.Controls.Add(avatar, 0, 0);
        body.SetRowSpan(avatar, 2);

        body.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0, 4, 0, 6)
        }, 1, 0);

        body.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            MaximumSize = new Size(170, 0),
            ForeColor = UIHelpers.TextSecondaryColor,
            Margin = new Padding(0)
        }, 1, 1);

        var chevronLabel = new Label
        {
            Text = "›",
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Regular),
            ForeColor = UIHelpers.AccentColor,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            Margin = new Padding(8, 6, 0, 0)
        };
        body.Controls.Add(chevronLabel, 2, 0);
        body.SetRowSpan(chevronLabel, 2);

        shell.Controls.Add(body);
        HoverShadowPanel.AddHoverEffect(shell, body);
        return shell;
    }

    private static string GetLauncherMonogram(string title)
    {
        var words = title
            .Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0]))
            .ToArray();

        return words.Length == 0 ? "L" : new string(words);
    }
}
