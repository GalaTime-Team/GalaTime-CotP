using Godot;
using System;

public partial class LabelButton : Label
{
    [Export] public Vector2 DefaultScale = new(2, 2);
    [Export] public Vector2 HoverScale = new(2.2f, 2.2f);
    [Export] public Color DefaultColor = new(1, 1, 1);
    [Export] public Color HoverColor = new(1, 1, 0);
    [Export] public Color PressedColor = new(0.49f, 0.49f, 0.49f);
    [Export] public float Speed = 0.2f;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mbe && mbe.ButtonIndex == MouseButton.Left && mbe.Pressed) OnPressed();
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationMouseEnter:
            case NotificationFocusEnter:
                Hover();
                break;
            case NotificationMouseExit:
            case NotificationFocusExit:
                ExitHover();
                break;
            case NotificationResized:
                PivotOffset = Size / 2;
                break;
        }
    }

    // Method to get a Tween instance
    Tween GetTween() => GetTree().CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel(true);

    void Hover() => ApplyHoverEffects(HoverColor, HoverScale);
    void ExitHover() => ApplyHoverEffects(DefaultColor, DefaultScale);

    // Method to apply hover effects with specified color and scale
    void ApplyHoverEffects(Color color, Vector2 scale)
    {
        var tween = GetTween();
        TweenColor(tween, DefaultColor, color, Speed);
        tween.TweenProperty(this, "scale", scale, Speed);
    }

    private void TweenColor(Tween tween, Color From, Color To, float Duration) => 
        tween.TweenMethod(Callable.From((Color x) => AddThemeColorOverride("font_color", x)), From, To, Duration);

    public void OnPressed()
    {
        AddThemeColorOverride("font_color", PressedColor);
        Size = HoverScale;
        var tween = GetTween();
        tween.TweenProperty(this, "scale", DefaultScale, Speed);
    }
}
