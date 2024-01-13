using Galatime.Global;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galatime.UI;

/// <summary> Represents a select wheel in the UI. Selects an item in the wheel with the mouse or keyboard. </summary>
public partial class SelectWheel : Control
{
    /// <summary> The maximum number of items in the wheel. Sprite is too big to fit more then 8. </summary>
    public const int WheelSegmentMaxCount = 8;

    #region Nodes
    /// <summary> Contains all the UI items that in the wheel in order. </summary>
    public List<WheelSegment> WheelSegments = new();
    public Label SegmentName;
    public PackedScene WheelSegmentScene;
    public Tween Tween;

    public Control[] Placeholders;
    public string[] Names;
    public bool[] Disabled;
    #endregion

    #region Variables
    public int Selected;
    /// <summary> Which item is selected in the wheel. </summary>
    private int itemCount = 8;
    [Export(PropertyHint.Range, "1,8")]
    /// <summary> Number of items in the wheel. </summary>
    public int ItemCount
    {
        get => itemCount;
        // Don't allow less than 1 item and more than 8 because sprite is too big.
        set => itemCount = Mathf.Clamp(value, 1, WheelSegmentMaxCount);
    }
    /// <summary> The ID of the currently opened wheel. </summary>
    public string CurrentWheelId { get; private set; } = "";
    private Action<int> SelectedCallback;
    #endregion

    public override void _Ready()
    {
        SegmentName = GetNode<Label>("SegmentName");
    }

    // TODO: Move all the tweening stuff to the separate class.
    private void SetTween()
    {
        if (Tween != null && !Tween.IsRunning()) Tween?.Kill();
        Tween = GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);
    }

    public void TweenSegmentNameColor(Color from, Color to) => Tween.TweenMethod(Callable.From<Color>(x => SegmentName.Modulate = x), from, to, .1f);

    /// <summary> Rebuilds the wheel by instantiating and adding the wheel segments. </summary>
    public void Rebuild()
    {
        // Remove all the old wheel segments.
        foreach (var segment in WheelSegments)
        {
            RemoveChild(segment);
            segment.QueueFree();
        }
        WheelSegments.Clear();

        // GD.Print($"Rebuilding wheel with {ItemCount} items. Names count: {Names.Length}, placeholders count: {Placeholders.Length}");

        WheelSegmentScene = GD.Load<PackedScene>("res://assets/objects/gui/WheelSegment.tscn");
        for (var i = 0; i < ItemCount; i++)
        {
            var segment = WheelSegmentScene.Instantiate<WheelSegment>();

            var rotation = i * (360 / ItemCount);
            // Adding the segment to the wheel by rotating it.
            segment.RotationDegrees = rotation;

            // Adding the segment to the list.

            // Setting the index of the segment.
            segment.Index = i;

            // Adding pressed event to the segment.
            segment.Pressed += () =>
            {
                Selected = segment.Index;
                // Call the callback with the selected index.
                SelectedCallback?.Invoke(Selected);

                // Close the wheel, because the user can't select anything else.
                CloseWheel();
            };
            // Set the name of the segment when segment hovered.
            segment.Hover += (bool entered) => SetName(segment.SegmentName, entered);

            var disabled = false;
            if (Disabled != null && Disabled.Length > 0) disabled = Disabled[i];
            if (disabled) segment.Disabled = true;

            // Adding the segment to the list and to the wheel.
            AddChild(segment);
            WheelSegments.Add(segment);

            // Play the animation of appearing.
            segment.Shown = true;

            // Add the placeholders and names to the segments.
            var s = WheelSegments[i];
            segment.SegmentName = Names[i];

            var placeholder = Placeholders[i];
            placeholder.PivotOffset = placeholder.Size / 2;
            placeholder.RotationDegrees = -rotation;

            segment.AddChild(Placeholders[i]);
        }
    }

    private void CloseWheel()
    {
        WindowManager.Instance.ToggleWindow("wheel", false);

        SelectedCallback = null;
        CurrentWheelId = "";
        WheelSegments.ForEach(x => x.Shown = false);
        GetTree().CreateTimer(0.15f).Timeout += () => Visible = false;

        SetTween();
        TweenSegmentNameColor(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0));
    }

    /// <summary> Calls the wheel with the given parameters. </summary>
    /// <param name="id"> The ID of the wheel. </param>
    /// <param name="size"> The number of items in the wheel. </param>
    /// <param name="placeholders"> The control placeholders of the items in the wheel. It must be in the same order as the names. </param>
    /// <param name="names"> The names of the items in the wheel. </param>
    /// <param name="callback"> The callback to be called when the wheel is closed. </param>
    /// <param name="disabled"></param>
    public void CallWheel(string id, int size, Control[] placeholders, string[] names, Action<int> callback, bool[] disabled)
    {
        ItemCount = size;
        if (CheckSize(placeholders, names)) return;
        // If the wheel is already opened, close it.
        if (CurrentWheelId == id)
        {
            GD.PushWarning($"Wheel {id} is already opened, closing...");
            CloseWheel();
            return;
        }
        // Don't allow opening the wheel if it's already opened by another wheel.
        if (CurrentWheelId != "")
        {
            GD.PushWarning($"Wheel {CurrentWheelId} is already opened.");
            return;
        }

        if (!WindowManager.Instance.ToggleWindow("wheel", false, () => CloseWheel())) return;

        // Set the callback to be called when the wheel is closed.
        SelectedCallback = callback;

        Placeholders = placeholders;
        Names = names;
        Disabled = disabled;
        CurrentWheelId = id;

        // Rebuild the wheel segments.
        Rebuild();

        Visible = true;

        // Play the animation of appearing.
        SetTween();
        TweenSegmentNameColor(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1));
    }

    // Check if the number of elements is less than the number of wheel items to prevent errors.
    private bool CheckSize(params object[][] a)
    {
        foreach (var array in a)
        {
            if (array.Length < ItemCount)
            {
                GD.PushWarning($"Can't call wheel, because number of placeholders ({a.Length}) is less than number of items ({ItemCount})");
                return true;
            }
        }

        return false;
    }

    public void SetName(string text, bool show = true) => SegmentName.Text = show ? text : "";
}
