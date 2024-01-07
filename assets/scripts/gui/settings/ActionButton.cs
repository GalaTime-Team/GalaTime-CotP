using Godot;
using System;

namespace Galatime.UI;

/// <summary> Represents an action button that help to bind an action. </summary>
public partial class ActionButton : Button
{
    private string actionName;
    /// <summary> Action to bind. </summary>
    [Export]
    public string ActionName
    {
        get => actionName;
        set
        {
            actionName = value;
            DisplayKey();
        }
    }

    private long key = -1;
    /// <summary> Represents the key used </summary>
    [Export] public long Key
    {
        get => key;
        set
        {
            key = value;

            // Create key event based on key.
            var @event = new InputEventKey() { PhysicalKeycode = (Key)key };

            // Remove previous bind.
            InputMap.ActionEraseEvents(ActionName);
            InputMap.ActionAddEvent(ActionName, @event);

            DisplayKey();

            OnBound?.Invoke(key);
        }
    }

    /// <summary> When action is bound. Returns long representation of bind. </summary>
    public Action<long> OnBound;

    public override void _Ready()
    {
        SetProcessUnhandledInput(false); // I don't know why, but it works.
        DisplayKey();

        Toggled += OnToggled;
    }

    private void OnToggled(bool toggled)
    {
        SetProcessUnhandledInput(toggled); // I don't even why I just wrote this.

        // When toggled wait for keybind to be set.
        if (toggled) Text = "...";
        else DisplayKey();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        BindKey(@event);
        ButtonPressed = false;
    }

    public void BindKey(InputEvent @event)
    {
        if (@event is not InputEventKey key) return; // Don't bind non key events.
        Key = (long)key.PhysicalKeycode;
    }

    /// <summary> Displays the key of the bind on button. </summary>
    public void DisplayKey() => Text = InputMap.ActionGetEvents(ActionName)[0].AsText().Replace(" (Physical)", "");
}
