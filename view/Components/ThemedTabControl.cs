namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Windows.Forms;

internal sealed class ThemedTabControl : TabControl
{
    private const int WmPaint = 0x000F;

    public ThemedTabControl()
    {
        DrawMode = TabDrawMode.OwnerDrawFixed;
        BackColor = ColorThemes.GetPrimaryBackgroundColor();
        ItemSize = new Size(180, 32);
        SizeMode = TabSizeMode.Fixed;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);

        if (e.Control is TabPage page)
        {
            page.UseVisualStyleBackColor = false;
            page.BackColor = ColorThemes.GetPrimaryBackgroundColor();
            page.ForeColor = ColorThemes.GetSecondaryTextColor();
        }
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var tabBounds = GetTabRect(e.Index);
        bool isSelected = SelectedIndex == e.Index;

        Color tabBackColor = isSelected
            ? ColorThemes.GetPrimaryBackgroundColor()
            : ColorThemes.GetCardBackgroundColor();

        Color tabTextColor = isSelected
            ? ColorThemes.GetSecondaryTextColor()
            : ColorThemes.GetPrimaryTextColor();

        using var backgroundBrush = new SolidBrush(tabBackColor);
        using var borderPen = new Pen(ColorThemes.GetPrimaryBackgroundColor());

        e.Graphics.FillRectangle(backgroundBrush, tabBounds);
        e.Graphics.DrawRectangle(borderPen, tabBounds.X, tabBounds.Y, tabBounds.Width - 1, tabBounds.Height - 1);

        TextRenderer.DrawText(
            e.Graphics,
            TabPages[e.Index].Text,
            Font,
            tabBounds,
            tabTextColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg == WmPaint)
        {
            using var graphics = CreateGraphics();
            DrawHeaderGapBackground(graphics);
            HideSystemEdgeStrips(graphics);
        }
    }

    private void DrawHeaderGapBackground(Graphics graphics)
    {
        if (TabCount == 0)
        {
            return;
        }

        int headerBottom = GetTabRect(0).Bottom;
        if (headerBottom <= 0)
        {
            return;
        }

        int firstLeft = GetTabRect(0).Left;
        int lastRight = GetTabRect(TabCount - 1).Right;

        using var headerBrush = new SolidBrush(ColorThemes.GetPrimaryBackgroundColor());

        if (firstLeft > 0)
        {
            graphics.FillRectangle(headerBrush, new Rectangle(0, 0, firstLeft, headerBottom + 1));
        }

        if (lastRight < ClientSize.Width)
        {
            graphics.FillRectangle(headerBrush, new Rectangle(lastRight, 0, ClientSize.Width - lastRight, headerBottom + 1));
        }
    }

    private void HideSystemEdgeStrips(Graphics graphics)
    {
        Color backgroundColor = ColorThemes.GetPrimaryBackgroundColor();
        var pageBounds = DisplayRectangle;
        using var backgroundBrush = new SolidBrush(backgroundColor);

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        graphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, ClientSize.Width, 1));
        graphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, 1, ClientSize.Height));
        graphics.FillRectangle(backgroundBrush, new Rectangle(Math.Max(ClientSize.Width - 1, 0), 0, 1, ClientSize.Height));
        graphics.FillRectangle(backgroundBrush, new Rectangle(0, Math.Max(ClientSize.Height - 1, 0), ClientSize.Width, 1));

        if (pageBounds.Left > 0)
        {
            graphics.FillRectangle(backgroundBrush, new Rectangle(0, pageBounds.Top, pageBounds.Left, pageBounds.Height));
        }

        if (pageBounds.Right < ClientSize.Width)
        {
            graphics.FillRectangle(backgroundBrush, new Rectangle(pageBounds.Right, pageBounds.Top, ClientSize.Width - pageBounds.Right, pageBounds.Height));
        }

        if (pageBounds.Bottom < ClientSize.Height)
        {
            graphics.FillRectangle(backgroundBrush, new Rectangle(0, pageBounds.Bottom, ClientSize.Width, ClientSize.Height - pageBounds.Bottom));
        }

        graphics.FillRectangle(backgroundBrush, new Rectangle(Math.Max(pageBounds.Left - 1, 0), Math.Max(pageBounds.Top - 1, 0), pageBounds.Width + 2, 1));
        graphics.FillRectangle(backgroundBrush, new Rectangle(Math.Max(pageBounds.Left - 1, 0), pageBounds.Bottom, pageBounds.Width + 2, 1));
        graphics.FillRectangle(backgroundBrush, new Rectangle(Math.Max(pageBounds.Left - 1, 0), Math.Max(pageBounds.Top - 1, 0), 1, pageBounds.Height + 2));
        graphics.FillRectangle(backgroundBrush, new Rectangle(pageBounds.Right, Math.Max(pageBounds.Top - 1, 0), 1, pageBounds.Height + 2));
    }
}