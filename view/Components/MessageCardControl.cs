using System.Drawing;
using System.Windows.Forms;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class MessageCardControl
{
    public static Control Create(string title, string message)
    {
        var card = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(14),
            BackColor = UIHelpers.CardBackground
        };
        UIHelpers.SetRoundedRegion(card, 18);

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
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0, 0, 0, 8)
        });

        layout.Controls.Add(new Label
        {
            Text = message,
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            ForeColor = UIHelpers.TextSecondaryColor
        });

        card.Controls.Add(layout);
        return card;
    }
}
