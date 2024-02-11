using Galatime.UI;
using Godot;
using System;
using System.Collections.Generic;

namespace Galatime;

/// <summary> Represents a value bar in the UI, displaying the current value of the any kind. </summary>
public partial class ValueBar : Control
{
    #region Exports
    /// <summary> Text, which is displayed and placed to the right of the bar. Like "HP" or "MANA" and so on. </summary>
    [Export] public string ValueText = "VAL";
    /// <summary> The texture path of the transient texture for transient effect. </summary>
    [Export(PropertyHint.File)] public string TransientTexturePath;
    [Export] public Color ChangedColor;
    [Export] public Color NormalColor;

    [Export] public NodePath ProgressBarNodePath;
    [Export] public NodePath LabelNodePath;
    [Export] public NodePath TextureNodePath;
    [Export] public Godot.Collections.Array<NodePath> ShakeControlNodePaths;
    #endregion

    #region Nodes
    public TextureProgressBar ProgressBar;
    public TextureProgressBar TransientProgressBar;
    public Label Label;
    public TextureRect Texture;
    public List<ShakeControl> ShakeControls = new();

    /// <summary> Used to delay change of the current delayed progress bar. </summary>
    public Timer TransientTimer;

    public Tween TweenTransientProgressBar;
    public Tween TweenColor;
    #endregion

    #region Variables
    /// <summary> Currently loaded transient texture. </summary>
    public Texture2D TransientTexture;
    /// <summary> Used when delay is over and then assigns the current delayed progress bar value. </summary>
    private float TransientValue = 0;
    /// <summary> Used to set the current delayed progress bar value. </summary>
    public TextureProgressBar CurrentTransientProgressBar;

    private float value = 0f;
    /// <summary> The current value of the value bar. When changed it will change progress bar. </summary>
    public float Value
    {
        get => value; set
        {
            this.value = value;
            SetValue(value);
        }
    }

    /// <summary> Sets the value of the progress bar and updates the display. </summary>
    /// <param name="forceUpdate"> If set to true, the shake effect will not be applied to all shake controls before updating the progress bar. </param>
    public void SetValue(float value)
    {
        ShakeAll(); // Apply shake effect to all shake controls.

        // Delay the progress bar based on the transient value.
        // This creates a smooth effect of changing the progress bar and shows difference between the current value and changed value.

        if (TransientValue > value)
        { // When value is smaller.
            ProgressBar.Value = value;
            CurrentTransientProgressBar = TransientProgressBar;
        }
        else // When value is bigger.
        {
            StartTransientColor();
            TransientProgressBar.Value = value;
            CurrentTransientProgressBar = ProgressBar;
        }

        TransientTimer.Stop();
        TweenTransientProgressBar?.Kill();
        TweenTransientProgressBar = null;

        // Start the transient timer, which will delay change progress bar.
        TransientTimer.Start();

        Label.Text = $"{value} {ValueText}";
    }

    private float maxValue = 100f;
    /// <summary> The maximum value of the value bar. When changed it will set the maximum value of the progress bar. </summary>
    public float MaxValue
    {
        get => maxValue; set
        {
            SetMaxValue(value);
        }
    }

    public void SetMaxValue(float value, float currentValue = -1)
    {
        maxValue = value;
        ProgressBar.MaxValue = value;
        TransientProgressBar.MaxValue = value;

        if (currentValue != -1) Value = currentValue;
    }

    /// <summary> The delay of the transient effect in seconds for current delayed progress bar. </summary>
    public float TransientDelay = 1f;
    /// <summary> The duration of the transient effect in seconds for current delayed progress bar. </summary>
    public float TransientDuration = 2f;
    #endregion

    public override void _Ready()
    {
        #region Get nodes
        ProgressBar = GetNode<TextureProgressBar>(ProgressBarNodePath);
        Label = GetNode<Label>(LabelNodePath);
        Texture = GetNode<TextureRect>(TextureNodePath);
        foreach (NodePath shakeControlNodePath in ShakeControlNodePaths) ShakeControls.Add(GetNode<ShakeControl>(shakeControlNodePath));
        #endregion

        // Create transient timer to delay change of the progress bar.
        TransientTimer = new Timer
        {
            WaitTime = TransientDelay,
            OneShot = true
        };
        TransientTimer.Timeout += StartTransientProgressBarEffect;
        AddChild(TransientTimer);

        InitTransientProgressBar();
    }

    /// <summary> Forcefully updates the value of the value bar without any delay. </summary>
    // public void ForceUpdateValue(float value)
    // {
    //     var previous = this.value;
    //     this.value = value;
    //     CurrentTransientProgressBar = previous > this.value ? TransientProgressBar : ProgressBar;

    //     ProgressBar.Value = value;
    //     TransientProgressBar.Value = value;
    //     Label.Text = $"{this.value} {ValueText}";

    //     TransientTimer.Stop();
    // }

    public override void _ExitTree()
    {
        TransientTimer.Timeout -= StartTransientProgressBarEffect;

        // Clear shake controls to avoid memory leaks.
        ShakeControls.Clear();

        // Abort all tweeners for as well.
        TweenTransientProgressBar?.Kill();
        TweenColor?.Kill();
    }

    private void InitTransientProgressBar()
    {
        // Load transient texture.
        TransientTexture = ResourceLoader.Load<Texture2D>(TransientTexturePath);

        // Create transient progress bar by duplicating progress bar.
        TransientProgressBar = ProgressBar.Duplicate() as TextureProgressBar;

        // Set transient progress bar texture and other properties.
        TransientProgressBar.TextureProgress = TransientTexture;
        TransientProgressBar.Name = "TransientProgressBar";
        TransientProgressBar.ZIndex--;

        // Add transient progress bar to the scene.
        AddChild(TransientProgressBar);
    }

    /// <summary> Applies shake effect to all shake controls. </summary>
    public void ShakeAll()
    {
        // Obviously, shake all shake controls by calling their shake method.
        foreach (ShakeControl shakeControl in ShakeControls) shakeControl.Shake();
    }

    public void StartTransientProgressBarEffect()
    {
        // So, when delay is over, set the transient progress bar value.
        TransientValue = Value;

        // Create tween to smoothly change progress bar value.
        TweenTransientProgressBar = GetTree().CreateTween().SetTrans(Tween.TransitionType.Sine);

        // Set initial value to change from.
        var initialValue = CurrentTransientProgressBar.Value;

        // Tween to the new value for 2 seconds.
        TweenTransientProgressBar.TweenMethod(Callable.From<float>((x) => { if (IsInstanceValid(CurrentTransientProgressBar)) CurrentTransientProgressBar.Value = x; }), initialValue, Value, TransientDuration);
    }

    public void StartTransientColor()
    {
        TweenColor = GetTree().CreateTween();
        TweenColor.TweenMethod(Callable.From<Color>((x) => { if (IsInstanceValid(Label)) Label.Modulate = x; }), ChangedColor, NormalColor, TransientDuration);
    }
}
