using System.Drawing;
using System.Windows.Forms;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class StateCardControl
{
    public static Control Create(string title, string message)
    {
        var shell = new HoverShadowPanel()
        {
            Width = 420,
            Height = 160,
            Margin = new Padding(0, 0, 14, 14)
        };

        var body = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18)
        };
        UIHelpers.SetRoundedRegion(body, 18);

        var textLayout = new FlowLayoutPanel()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        textLayout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 8)
        });

        textLayout.Controls.Add(new Label
        {
            Text = message,
            AutoSize = true,
            MaximumSize = new Size(360, 0),
            ForeColor = Color.FromArgb(107, 114, 128)
        });

        body.Controls.Add(textLayout);
        shell.Controls.Add(body);
        return shell;
    }
}
