using System.ComponentModel;
using System.Drawing.Drawing2D;
using SystemGameManager.View.Components;

namespace SystemGameManager.view.Components;

public class ModernTrackBar : Control
{
    private int minimum;
    private int maximum = 100;
    private int value = 50;

    [Category("Behavior")]
    [DefaultValue(0)]
    public int Minimum
    {
        get => minimum;
        set
        {
            minimum = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(100)]
    public int Maximum
    {
        get => maximum;
        set
        {
            maximum = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [DefaultValue(50)]
    public int Value
    {
        get => this.value;
        set
        {
            this.value = Math.Clamp(value, minimum, maximum);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int lineY = Height / 2;

        using var backPen = new Pen(Color.FromArgb(60,60,60),4);
        using var fillPen = new Pen(ColorThemes.GetSecondaryTextColor(),4);

        g.DrawLine(backPen,10,lineY,Width-20,lineY);

        float percent = (Value-Minimum)/(float)(Maximum-Minimum);

        int x = 10 + (int)((Width-30)*percent);

        g.DrawLine(fillPen,10,lineY,x,lineY);

        g.FillRectangle(
            new SolidBrush(ColorThemes.GetSecondaryTextColor()),
            x-5,
            lineY-7,
            10,
            14);
    }
}