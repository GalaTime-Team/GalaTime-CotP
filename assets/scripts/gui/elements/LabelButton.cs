using Godot;

namespace Galatime.UI;

[Tool]
public partial class LabelButton : Button
{
    #region Exports
    [Export] public Vector2 HoverScale = new(2.2f, 2.2f);
    [Export] public Color HoverColor = new(1, 1, 0);
    [Export] public Color PressedColor = new(0.49f, 0.49f, 0.49f);
    [Export] public Color DisabledColor = new(0.4f, 0.4f, 0.4f);
    [Export] public float Speed = 0.2f;
    [Export] public FontFile Font;
    private string buttonText = "BUTTON";
    [Export(PropertyHint.MultilineText)] public string ButtonText
    {
        get => buttonText;
        set
        {
            buttonText = value;
            if (Label != null) Label.Text = buttonText;
        }
    }
    #endregion

    #region Variables
    private Vector2 DefaultScale;
    private Color DefaultColor;
    private Tween Tw;
    #endregion

    #region Nodes
    public Label Label;
    #endregion  

    #region Audio
    [Export] public AudioStream AudioStreamHover;
    [Export] public AudioStream AudioStreamPressed;
    public AudioStreamPlayer AudioHover;
    public AudioStreamPlayer AudioPressed;
    #endregion

    public override void _Ready()
    {
        Label = GetNode<Label>("Label");

        ButtonText = ButtonText; // Initialize text.

        InitializeDefaults();
        InitializeAudios();

        Pressed += OnPressed;
    }

    public override void _Draw()
    {
        Label?.AddThemeColorOverride("font_color", Disabled ? DisabledColor : DefaultColor);
    }

    public override void _Process(double delta)
    {
        if (Label != null)
        {
            if (Disabled) Label.Scale = DefaultScale;
            Label.PivotOffset = Label.Size / 2;

            CustomMinimumSize = Label.Size;
            if (Font != null && GetThemeFont("font").GetFontName() != Font.GetFontName())
            {
                Label.AddThemeFontOverride("font", Font);
                AddThemeFontOverride("font", Font);
            }

            Label.Size = Vector2.Zero;

            /// Calculates and sets the position of the label within the container.
            /// The label will be centered horizontally and vertically.
            Label.Position = new Vector2()
            {
                X = (Size.X - Label.Size.X) / 2,
                Y = (Size.Y - Label.Size.Y) / 2
            };
        }
    }

    private void InitializeDefaults()
    {
        DefaultScale = Label.Scale;
        DefaultColor = GetThemeColor("font_color");
    }

    private void InitializeAudios()
    {
        AudioHover = GetNode<AudioStreamPlayer>("AudioHover");
        AudioPressed = GetNode<AudioStreamPlayer>("AudioPressed");

        if (AudioStreamHover != null) AudioHover.Stream = AudioStreamHover;
        if (AudioStreamPressed != null) AudioPressed.Stream = AudioStreamPressed;
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationMouseEnter:
            case NotificationFocusEnter:
                if (!Disabled) Hover();
                break;
            case NotificationMouseExit:
            case NotificationFocusExit:
                if (!Disabled) ExitHover();
                else
                {
                    Tw?.Kill();
                    Tw = GetTween();
                    TweenColor(Tw, Label.GetThemeColor("font_color"), DisabledColor, Speed * 4);
                }
                break;
            case NotificationResized when Label is not null:
                Label.PivotOffset = Label.Size / 2;
                break;
        }
    }

    // Method to get a Tween instance with preset settings
    Tween GetTween() => CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel(true);

    void Hover()
    {
        AudioHover.Play();
        ApplyHoverEffects(HoverColor, HoverScale);
    }

    void ExitHover() => ApplyHoverEffects(DefaultColor, DefaultScale);

    // Method to apply hover effects with specified color and scale
    void ApplyHoverEffects(Color color, Vector2 scale)
    {
        Tw?.Kill();
        Tw = GetTween();
        TweenColor(Tw, DefaultColor, color, Speed);
        Tw.TweenProperty(Label, "scale", scale, Speed);
    }

    private void TweenColor(Tween tw, Color From, Color To, float Duration)
    {
        tw.TweenMethod(Callable.From((Color x) => Label.AddThemeColorOverride("font_color", x)), From, To, Duration);
    }

    public void OnPressed()
    {
        AudioPressed.Play();
        Label.AddThemeColorOverride("font_color", PressedColor);
        Label.Size = HoverScale;

        Tw?.Kill();
        Tw = GetTween();

        float duration = Speed * 2;
        TweenColor(Tw, PressedColor, IsHovered() || HasFocus() ? HoverColor : DefaultColor, duration);
        Tw.TweenProperty(Label, "scale", DefaultScale, duration);
    }
}