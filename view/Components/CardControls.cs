namespace SystemGameManager.View.Components;

class CardControls
{
    public CardControls()
    {
        
    }
    public static FlowLayoutPanel GetRoundedCardPanel(int radius, bool isBoxShadow = false, bool isDropDown = false)
    {
        var panel = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(10),
            BackColor = ColorThemes.GetCardBackgroundColor(),
        };
        if(isBoxShadow)
        {
            SetBoxShadow(panel);
        }

        UIHelpers.SetRoundedRegion(panel, radius);
        if(isDropDown)
        {
            GetDropDownCard(panel);
        }
        return panel;
    }
    private static FlowLayoutPanel GetDropDownCard(FlowLayoutPanel card)
    {
        var dropDownArrow = new PictureBox()
        {
            Image = UIHelpers.LoadIcon("assets/icons/chevron-down-solid-full.svg", new Size(16, 16)),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 0),
        };
        dropDownArrow.Click += (_, _) => ToggleCardDropDown(card, dropDownArrow);
        card.Controls.Add(dropDownArrow);
        return card;
    }
    private static void SetBoxShadow(FlowLayoutPanel card)
    {
        var shadowPanel = new HoverShadowPanel()
        {
            Dock = DockStyle.Fill,
            IsHovered = false,
        };
        card.Controls.Add(shadowPanel);
        shadowPanel.SendToBack();
    }
    private static void ToggleCardDropDown(FlowLayoutPanel card, PictureBox arrow)
    {
        bool isExpanded = card.Height > 60; // Assuming 60 is the height of the header
        if (isExpanded)
        {
            card.Height = 60;
            arrow.Image = UIHelpers.LoadIcon("assets/icons/chevron-down-solid-full.svg", new Size(16, 16));
        }
        else
        {
            card.Height = card.GetPreferredSize(new Size(card.Width, int.MaxValue)).Height;
            arrow.Image = UIHelpers.LoadIcon("assets/icons/chevron-up-solid-full.svg", new Size(16, 16));
        }
    }
}