using Godot;
using System;

public partial class PausePagesButtonsContainer : HBoxContainer
{
    Godot.Collections.Array<Node> pagesButtons;
    Godot.Collections.Array<Node> pagesNodes = new Godot.Collections.Array<Node>();

    public override void _Ready()
    {
        pagesButtons = GetChildren();
        for (int i = 0; i < pagesButtons.Count; i++)
        {
            var node = GetChild(i) as Control;
            var id = i;
            node.GuiInput += (InputEvent @event) => _onButtonsGuiInput(@event, id);
        }

        pagesNodes.Add(GetNode("../inventory"));
        pagesNodes.Add(GetNode("../StatsContainer"));
        pagesNodes.Add(GetNode("../AbilitiesContainer"));
    }

    public void _onButtonsGuiInput(InputEvent @event, int id)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                GD.Print("Esss");
                for (int i = 0; i < pagesNodes.Count; i++)
                {
                    if (pagesNodes[i] != null)
                    {
                        ((Control)pagesNodes[i]).Visible = false;
                        var button = pagesButtons[i] as Control;
                        button.RemoveThemeStyleboxOverride("normal");
                        button.AddThemeColorOverride("font_color", new Color(1, 1, 1));
                    }
                    else
                    {
                        GD.PushWarning("Can't switch page, page is null");
                    }
                }
                GD.Print(pagesNodes);
                GD.Print(id);
                if (pagesNodes[id] != null)
                {
                    var button = (Label)pagesButtons[id];
                    var styleBox = new StyleBoxFlat().Duplicate() as StyleBoxFlat;
                    styleBox.BorderColor = new Color(1, 1, 1);
                    styleBox.BgColor = new Color(1, 1, 1);
                    styleBox.BorderWidthLeft = 3;
                    styleBox.BorderWidthTop = 3;
                    styleBox.BorderWidthRight = 3;
                    styleBox.BorderWidthBottom = 3;
                    button.AddThemeColorOverride("font_color", new Color(0, 0, 0));
                    button.AddThemeStyleboxOverride("normal", styleBox);
                    GD.Print(button.HasThemeStyleboxOverride("normal")); 
                    ((Control)pagesNodes[id]).Visible = true;
                }
            }
        }
    }
}
