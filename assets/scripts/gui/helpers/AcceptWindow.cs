using System;
using Godot;

namespace Galatime.UI.Helpers;

/// <summary> Represents an accept window in the UI. It shows a text and an accept button or cancel button. </summary>
public partial class AcceptWindow : Control
{
    #region Nodes
    /// <summary> The buttons used in the accept window. Use <see cref="Accepted"/> or <see cref="Canceled"/> to get the result. </summary>
    public LabelButton AcceptButton, CancelButton;
    public Label TextLabel;
    #endregion

    #region Variables
    /// <summary> The text of the accept window. Use <see cref="CallAccept"/> to set the text. </summary>
    public string Text
    {
        get => TextLabel.Text;
        set => TextLabel.Text = value;
    }

    private bool shown = false;
    public bool Shown
    {
        get => shown;
        set => SetShown(value);
    }

    public void SetShown(bool value, bool force = false)
    {
        if (Tween != null && Tween.IsRunning()) return;

        shown = value;

        if (shown) Visible = true;
        else OnAcceptCallback = null;

        if (force) 
        {
            Visible = shown;
            return;
        }

        SetTween();
        Tween.TweenMethod(Callable.From<float>(x => Modulate = new Color(1, 1, 1, x)), shown ? 0f : 1f, shown ? 1f : 0f, .5f).Finished += () =>
        {
            if (!shown) Visible = false;
        };
    }

    /// <summary> The default colors of the accept and cancel buttons. </summary>
    public Color DefaultAcceptColor, DefaultCancelColor;

    public Tween Tween;

    public Control CurrentFocus { get; private set; }

    public const string AcceptWindowScenePath = "res://assets/objects/gui/helpers/AcceptWindow.tscn";
    #endregion

    #region Events
    public Action<bool> OnAcceptCallback;
    #endregion

    private void SetTween()
    {
        if (Tween != null && !Tween.IsRunning()) Tween?.Kill();
        Tween = GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);
    }

    public override void _Ready()
    {
        #region Get nodes
        TextLabel = GetNode<Label>("VBoxContainer/Name");
        AcceptButton = GetNode<LabelButton>("VBoxContainer/HBoxContainer/Yes");
        CancelButton = GetNode<LabelButton>("VBoxContainer/HBoxContainer/No");
        #endregion

        AcceptButton.Pressed += () => OnAcceptButtonPressed(true);
        CancelButton.Pressed += () => OnAcceptButtonPressed(false);

        DefaultAcceptColor = AcceptButton.HoverColor;
        DefaultCancelColor = CancelButton.HoverColor;
    }

    /// <summary> Creates an instance of the accept window and adds it to the scene. </summary>
    public static AcceptWindow CreateInstance()
    {
        var root = ((SceneTree)Engine.GetMainLoop()).Root;
        var instance = GD.Load<PackedScene>(AcceptWindowScenePath).Instantiate<AcceptWindow>();
        root.AddChild(instance);

        // By default, the accept window is not shown.
        instance.SetShown(false, true);

        return instance;
    }

    /// <summary> Shows the accept window and when accepted, calls the <paramref name="onAcceptCallback"/>. </summary>
    public void CallAccept(Action<bool> onAcceptCallback, string text = "Accept this", string acceptText = "Yes", string cancelText = "No",
        Color acceptColor = default, Color cancelColor = default, Control focus = null)
    {
        Shown = true;

        // Set text.
        Text = text;

        // Set buttons properties.
        SetButtonProperties(AcceptButton, acceptText, acceptColor);
        SetButtonProperties(CancelButton, cancelText, cancelColor);

        // Set callback.
        OnAcceptCallback = onAcceptCallback;

        AcceptButton.GrabFocus();
        CurrentFocus = focus;
    }

    private void SetButtonProperties(LabelButton button, string buttonText, Color hoverColor)
    {
        button.ButtonText = buttonText;
        button.HoverColor = hoverColor != default ? hoverColor : DefaultAcceptColor;
    }

    private void OnAcceptButtonPressed(bool value)
    {
        // Call callback one time.
        OnAcceptCallback?.Invoke(value);
        OnAcceptCallback = null;

        Shown = false;

        CurrentFocus?.GrabFocus();
    }
}