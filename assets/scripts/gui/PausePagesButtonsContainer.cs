using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PausePagesButtonsContainer : HBoxContainer
{
    List<Node> PagesButtons = new();
    List<Node> PagesNodes = new();

    public ColorRect SelectedBlock;

    public Tween Tween;
    public Label PreviousButton;

    public Tween GetTween() => GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);

    public override void _Ready()
    {
        SelectedBlock = GetNode<ColorRect>("../SelectedBlock");

        PagesButtons = GetChildren().ToList();
        for (int i = 0; i < PagesButtons.Count; i++)
        {
            var node = GetChild(i) as Control;
            var id = i;
            node.GuiInput += (InputEvent @event) => OnButtonsInput(@event, id);
        }

        PagesNodes.Add(GetNode("../Inventory"));
        PagesNodes.Add(GetNode("../StatsContainer"));
        PagesNodes.Add(GetNode("../AbilitiesContainer"));
    }

    public void OnButtonsInput(InputEvent @event, int id)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            for (int i = 0; i < PagesNodes.Count; i++)
            {
                if (PagesNodes[i] != null)
                {
                    ((Control)PagesNodes[i]).Visible = false;
                    
                    var b = PagesButtons[i] as Label;
                    if (b != null)
                    {
                        var t = GetTween();
                        t.TweenMethod(Callable.From<Color>(x => b.AddThemeColorOverride("font_color", x)),
                            b.GetThemeColor("font_color"), new Color(1f, 1f, 1f), 0.5f);
                    }
                }
                else
                    GD.Print("Can't switch page, page is null");
            }
            if (PagesNodes[id] != null)
            {
                var b = PagesButtons[id] as Label;
                var margin = 24;

                var calculatedSize = (b.Size * 2) with { Y = b.Size.Y * 2.22f };
                var calculatedMargin = new Vector2(margin, margin * .22f);

                Tween = GetTween();
                Tween?.TweenMethod(Callable.From<Vector2>(x =>
                    SelectedBlock.Size = x), SelectedBlock.Size, calculatedSize + calculatedMargin, 0.5f);
                Tween?.TweenMethod(Callable.From<Vector2>(x =>
                    SelectedBlock.GlobalPosition = x), SelectedBlock.GlobalPosition, b.GlobalPosition - calculatedMargin / 2, 0.5f);
                Tween?.TweenMethod(Callable.From<Color>(x => b.AddThemeColorOverride("font_color", x)),
                    b.GetThemeColor("font_color"), new Color(0f, 0f, 0f), 0.5f);

                ((Control)PagesNodes[id]).Visible = true;
            }
        }
    }
}
