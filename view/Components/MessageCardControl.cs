namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Windows.Forms;

internal static class MessageCardControl
{
    public static Control Create(string title, string message)
    {
        var card = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(14),
            BackColor = Color.White
        };

        var layout = new FlowLayoutPanel()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };

        layout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 8)
        });

        layout.Controls.Add(new Label
        {
            Text = message,
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            ForeColor = Color.FromArgb(107, 114, 128)
        });

        card.Controls.Add(layout);
        return card;
    }
}
