namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Windows.Forms;

internal static class LauncherBadgeControl
{
    public static Control Create(string title, string subtitle)
    {
        var shell = new Panel()
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 10, 10),
            Padding = new Padding(1),
            BackColor = Color.FromArgb(221, 227, 237)
        };

        var body = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(12),
            Margin = new Padding(0)
        };

        body.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55)
        });

        body.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            MaximumSize = new Size(280, 0),
            ForeColor = Color.FromArgb(107, 114, 128)
        });

        shell.Controls.Add(body);
        return shell;
    }
}
