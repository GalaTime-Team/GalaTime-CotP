using Galatime.UI;
using Godot;
using System;
using System.Collections.Generic;

public partial class SelectWheel : Control
{
    public const int WheelSegmentMaxCount = 8;

    #region Nodes
    /// <summary> Contains all the UI items that in the wheel in order. </summary>
    public List<WheelSegment> WheelSegments = new();
    public Label SegmentName;
    public PackedScene WheelSegmentScene;
    public Tween Tween;
    #endregion

    #region Variables
    private int selected;
    /// <summary> Which item is selected in the wheel. </summary>
    public int Selected
    {
        get => selected;
        set
        {
            selected = value;
        }
    }
    private int itemCount = 8;
    [Export(PropertyHint.Range, "1,8")]
    /// <summary> Number of items in the wheel. </summary>
    public int ItemCount
    {
        get => itemCount;
        // Don't allow less than 1 item and more than 8 because sprite is too big.
        set 
            {
            itemCount = Mathf.Clamp(value, 1, WheelSegmentMaxCount);
        }
    }
    /// <summary> The ID of the currently opened wheel. </summary>
    public string CurrentWheelId { get; private set; } = "";
    private Action<int> SelectedCallback;
    #endregion

    public override void _Ready()
    {
        SegmentName = GetNode<Label>("SegmentName");
    }

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

        WheelSegmentScene = GD.Load<PackedScene>("res://assets/objects/gui/WheelSegment.tscn");
        for (var i = 0; i < ItemCount; i++)
        {
            var segment = WheelSegmentScene.Instantiate<WheelSegment>();

            // Adding the segment to the wheel by rotating it.
            segment.RotationDegrees = i * (360 / ItemCount);

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

            // Adding the segment to the list and to the wheel.
            AddChild(segment);
            WheelSegments.Add(segment);

            // Play the animation of appearing.
            segment.Shown = true;
        }
    }

    private void CloseWheel()
    {
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
    public void CallWheel(string id, int size, Control[] placeholders, string[] names, Action<int> callback)
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

        CurrentWheelId = id;
        Visible = true;
        // Rebuild the wheel segments.
        Rebuild();

        // Add the placeholders and names to the segments.
        for (var i = 0; i < WheelSegments.Count; i++)
        {
            var segment = WheelSegments[i];
            segment.SegmentName = names[i];
            segment.AddChild(placeholders[i]);
        }

        // Set the callback to be called when the wheel is closed.
        SelectedCallback = callback;

        // Play the animation of appearing.
        SetTween();
        TweenSegmentNameColor(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1));
    }

    // Check if the number of elements is less than the number of wheel items to prevent errors.
    private bool CheckSize(params object[][] a) 
    {
        foreach (var array in a) {
            if (array.Length < ItemCount) {
                GD.PushWarning($"Can't call wheel, because number of placeholders ({a.Length}) is less than number of items ({ItemCount})");
                return true;
            }
        }
        
        return false;
    }

    public void SetName(string text, bool show = true) => SegmentName.Text = show ? text : "";
}
