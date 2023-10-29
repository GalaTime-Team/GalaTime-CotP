using Godot;
using System;

public partial class LabelButton : Label
{
    #region Exports
    [Export] public Vector2 HoverScale = new(2.2f, 2.2f);
    [Export] public Color HoverColor = new(1, 1, 0);
    [Export] public Color PressedColor = new(0.49f, 0.49f, 0.49f);
    [Export] public float Speed = 0.2f;
    #endregion

    #region Variables
    private Vector2 DefaultScale;
    private Color DefaultColor;
    #endregion

    #region Audio
    [Export] public AudioStream AudioStreamHover;
    [Export] public AudioStream AudioStreamPressed;
    public AudioStreamPlayer AudioHover;
    public AudioStreamPlayer AudioPressed;
    #endregion

    #region Events
    /// <summary> Emitted when the button is pressed. </summary>
    [Signal] public delegate void PressedEventHandler();
    #endregion

    public override void _Ready()
    {
        InitializeDefaults();
        InitializeAudios();

        Pressed += OnPressed;
    }

    private void InitializeDefaults() {
        DefaultScale = Scale;
        DefaultColor = GetThemeColor("font_color");
    }

    private void InitializeAudios()
    {
        AudioHover = GetNode<AudioStreamPlayer>("AudioHover");
        AudioPressed = GetNode<AudioStreamPlayer>("AudioPressed");

        if (AudioStreamHover != null) AudioHover.Stream = AudioStreamHover;
        if (AudioStreamPressed != null) AudioPressed.Stream = AudioStreamPressed;
    }


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

    // Method to get a Tween instance with preset settings
    Tween GetTween() => GetTree().CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel(true);

    void Hover() 
    {
        AudioHover.Play();
        ApplyHoverEffects(HoverColor, HoverScale);
    }

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
        AudioPressed.Play();
        AddThemeColorOverride("font_color", PressedColor);
        Size = HoverScale;

        var tween = GetTween();
        TweenColor(tween, PressedColor, DefaultColor, Speed);
        tween.TweenProperty(this, "scale", DefaultScale, Speed);
    }
}
