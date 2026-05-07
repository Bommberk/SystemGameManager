using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Krassheiten.SystemGameManager.View.Components;

internal sealed class HoverShadowPanel : Panel
{
    private bool isHovered;

    public bool IsHovered
    {
        get => isHovered;
        set
        {
            if (isHovered == value)
            {
                return;
            }

            isHovered = value;
            Invalidate();
        }
    }

    public HoverShadowPanel()
    {
        BackColor = Color.Transparent;
        Padding = new Padding(8, 8, 14, 14);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var cardBounds = new Rectangle(6, 6, Width - 22, Height - 22);
        var layers = IsHovered ? 6 : 3;
        var baseAlpha = IsHovered ? 18 : 8;

        for (var layer = layers; layer >= 1; layer--)
        {
            var shadowBounds = new Rectangle(
                cardBounds.X + layer,
                cardBounds.Y + layer + 1,
                Math.Max(1, cardBounds.Width),
                Math.Max(1, cardBounds.Height));

            using var path = UIHelpers.CreateRoundedRectanglePath(shadowBounds, 18);
            using var brush = new SolidBrush(Color.FromArgb(baseAlpha + (layer * 5), 15, 23, 42));
            e.Graphics.FillPath(brush, path);
        }
    }

    public static void AddHoverEffect(HoverShadowPanel shell, Panel body)
    {
        void SetState(bool hovered)
        {
            shell.IsHovered = hovered;
            body.BackColor = hovered ? Color.FromArgb(245, 247, 255) : Color.White;
        }

        void EnterHandler(object? sender, EventArgs e) => SetState(true);

        void LeaveHandler(object? sender, EventArgs e)
        {
            var cursor = shell.PointToClient(Cursor.Position);
            if (!shell.ClientRectangle.Contains(cursor))
            {
                SetState(false);
            }
        }

        WireHoverEvents(shell, EnterHandler, LeaveHandler);
    }

    private static void WireHoverEvents(Control control, EventHandler enterHandler, EventHandler leaveHandler)
    {
        control.MouseEnter += enterHandler;
        control.MouseLeave += leaveHandler;

        foreach (Control child in control.Controls)
        {
            WireHoverEvents(child, enterHandler, leaveHandler);
        }
    }
}
