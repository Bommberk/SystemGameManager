namespace SystemGameManager.View.Components;

using System.Windows.Forms;

class NormalButton : Button
{
    public NormalButton()
    {
        Dock = DockStyle.Fill;
        FlatStyle = FlatStyle.Flat;
        BackColor = ColorThemes.GetSecondaryBackgroundColor();
        ForeColor = ColorThemes.GetPrimaryTextColor();
        Cursor = Cursors.Hand;
        TabStop = false;

        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseDownBackColor = ColorThemes.GetSecondaryBackgroundColorHover();
        FlatAppearance.MouseOverBackColor = ColorThemes.GetSecondaryBackgroundColorHover();
    }

    public void SetHoverColor(Color hoverColor)
    {
        FlatAppearance.MouseOverBackColor = hoverColor;
    }
}