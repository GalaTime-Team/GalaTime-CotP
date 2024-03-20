using System;
using System.Collections.Generic;
using Godot;

namespace Galatime.UI.Helpers;

/// <summary> Represents an accept window in the UI. It shows a text and an accept button or cancel button. </summary>
public partial class AcceptWindow : Control
{
    /// <summary> Accept type of the accept window. </summary>
    public enum AcceptType
    {
        YesNo,
        Ok,
        OkCancel,
        YesNoCancel,
        Understood
    }

    public Dictionary<AcceptType, (string acceptText, string cancelText)> AcceptActions = new()
        {
            { AcceptType.Ok, ("Ok", "") },
            { AcceptType.OkCancel, ("Ok", "Cancel") },
            { AcceptType.YesNo, ("Yes", "No") },
            { AcceptType.YesNoCancel, ("Yes", "No") },
            { AcceptType.Understood, ("Understood", "") },
        };

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

        DefaultAcceptColor = new Color(1f, 1f, 0f);
        DefaultCancelColor = new Color(1f, 0f, 0f);
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

    /// <summary> Calls the accept window with options to customize the accept action. </summary>
    /// <param name="text"> The text to be displayed in the accept window. Default value is "Accept this". </param>
    /// <param name="acceptType"> The type of accept action to be used. </param>
    /// <param name="yesIsBad"> Indicates whether "Yes" option should be considered as a bad action. </param>
    /// <param name="focus"> The control to be focused when accepted. </param>
    public void CallAccept(Action<bool> onAcceptCallback, string text = "Accept this", AcceptType acceptType = AcceptType.Ok, bool yesIsBad = false, Control focus = null)
    {
        var (acceptText, cancelText) = AcceptActions[acceptType];

        var acceptColor = yesIsBad ? DefaultCancelColor : DefaultAcceptColor;
        var cancelColor = !yesIsBad ? DefaultCancelColor : DefaultAcceptColor;

        CallAccept(onAcceptCallback, text, acceptText, cancelText, acceptColor, cancelColor, !yesIsBad, focus: focus);
    }

    /// <summary> Shows the accept window and when accepted, calls the <paramref name="onAcceptCallback"/>. </summary>
    /// <param name="focusOnBit"> Indicates whether the focus should be grabbed on the accept or cancel button. </param>
    public void CallAccept(Action<bool> onAcceptCallback, string text = "Accept this", string acceptText = "Yes", string cancelText = "No",
        Color acceptColor = default, Color cancelColor = default, bool focusOnBit = true, Control focus = null)
    {
        Shown = true;
        Text = text;

        SetButtonProperties(AcceptButton, acceptText, acceptColor);
        SetButtonProperties(CancelButton, cancelText, cancelColor);

        OnAcceptCallback = onAcceptCallback;

        // Grab focus on the button.
        if (focusOnBit) AcceptButton.GrabFocus();
        else CancelButton.GrabFocus();

        CurrentFocus = focus;
    }

    private void SetButtonProperties(LabelButton button, string buttonText, Color hoverColor)
    {
        button.Visible = !string.IsNullOrEmpty(buttonText);
        button.ButtonText = buttonText;
        button.HoverColor = hoverColor;
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