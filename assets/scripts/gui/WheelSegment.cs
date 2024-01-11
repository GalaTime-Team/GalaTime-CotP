using Godot;
using System;

namespace Galatime.UI;

public partial class WheelSegment : TextureRect
{
    #region Events
    /// <summary> Performs an action when the wheel segment is pressed. </summary>
    public Action Pressed;
    /// <summary> Performs an action when the wheel segment is hovered. </summary>
    public Action<bool> Hover;
    #endregion

    #region Properties
    public AudioStreamPlayer Sound;
    public Button Button;
    #endregion

    #region Variables
    /// <summary> The index of the wheel segment in the wheel. </summary>
    public int Index;
    /// <summary> The name of the wheel segment shown in the GUI. </summary>
    public string SegmentName;

    private float OffsetN = 0f;
    /// <summary> The current offset of the wheel segment. Just Y position of the wheel + initial Y position. </summary>
    public float Offset
    {
        get => OffsetN;
        set
        {
            OffsetN = value;
            // Calculate the offset vector based on the rotation angle
            var offset = Vector2.Up.Rotated(Rotation) * OffsetN;
            // Add the offset vector to the initial position to get the final position
            Position = InitialPosition + offset;
        }
    }

    private bool shown;
    /// <summary> If the wheel segment is shown. Changing the shown property will also play the animation. </summary>
    public bool Shown
    {
        get => shown;
        set
        {
            shown = value;
            SetTween();

            if (!shown) Button.Disabled = true;
            TweenOffset(shown ? FromOffset : InitialOffset, shown ? InitialOffset : FromOffset);
            TweenColor(shown ? TransparentColor : Disabled ? DisabledColor : SelectedColor, shown ? Disabled ? DisabledColor : UnselectedColor : TransparentColor).Finished += () => {
                if (shown) Button.Disabled = false;
            };
        }
    }
    public bool Disabled;

    private Tween Tween;
    #endregion

    #region Values
    private Vector2 InitialPosition = new(-18, -57);

    public float TransitionDuration = 0.2f;

    public float InitialOffset = 0;
    public float FinalOffset = 6f;
    public float FromOffset { get => InitialPosition.Y; }

    public Color SelectedColor = new(1f, 1f, 1f, 1f);
    public Color UnselectedColor = new(1f, 1f, 1f, 0.5f);
    public Color TransparentColor = new(1f, 1f, 1f, 0f);
    public Color DisabledColor = new(1, 0, 0, 0.5f);
    #endregion

    public override void _Ready()
    {
        #region Get nodes
        Sound = GetNode<AudioStreamPlayer>("Sound");
        Button = GetNode<Button>("Button");
        #endregion

        InitializeButton();
    }

    private void InitializeButton()
    {
        Button.MouseEntered += () => MouseAction();
        Button.FocusEntered += () => MouseAction();
        Button.MouseExited += () => MouseAction(false);
        Button.FocusExited += () => MouseAction(false);
        Button.Pressed += () =>
        {
            if (Disabled) return;
            Pressed?.Invoke();
        };
    }


    public void MouseAction(bool entered = true)
    {
        if (Disabled) return;

        Hover?.Invoke(entered);
        if (Button.Disabled) return;
        SetTween();
        TweenOffset(entered ? InitialOffset : FinalOffset, entered ? FinalOffset : InitialOffset);
        TweenColor(entered ? UnselectedColor : SelectedColor, entered ? SelectedColor : UnselectedColor);
        if (entered) Sound.Play();
    }

    public MethodTweener TweenOffset(float from, float to) => Tween.TweenMethod(Callable.From<float>(x => Offset = x), from, to, TransitionDuration);
    public MethodTweener TweenColor(Color from, Color to) => Tween.TweenMethod(Callable.From<Color>(x => Modulate = x), from, to, TransitionDuration * 0.75f);

    private void SetTween()
    {
        if (Tween != null) Tween?.Kill();
        Tween = GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);
    }
}
