using Godot;

public partial class MainMenuButton : Button
{
    [Export] Vector2 hoverScale = new Vector2(1.25f, 1.25f);
    [Export] Vector2 hoverScaleExited = new Vector2(1f, 1f);

    AudioStreamPlayer audioHover;
    AudioStreamPlayer audioPressed;

    public override void _Ready()
    {
        audioHover = GetNode<AudioStreamPlayer>("AudioStreamPlayerButtonHover");
        audioPressed = GetNode<AudioStreamPlayer>("AudioStreamPlayerButtonAccept");

        FocusEntered += _focus;
        MouseEntered += _focus;

        FocusExited += _focusExited;
        MouseExited += _focusExited;

        _centerPivot();
    }

    public void _centerPivot()
    {
        PivotOffset = Size / 2;
    }

    public override void _Pressed()
    {
        Disabled = true;

        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        Set("theme_override_colors/font_color", new Color(0.5f, 0.5f, 0.5f));
        tween.TweenProperty(this, "theme_override_colors/font_color", new Color(1, 1, 1), 0.6f);

        audioPressed.Play();
    }

    public void _focus()
    {
        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        tween.TweenProperty(this, "scale", hoverScale, 0.1f);
        audioHover.Play();
    }

    public void _focusExited()
    {
        var tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut).SetParallel(true);

        tween.TweenProperty(this, "scale", hoverScaleExited, 0.1f);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
