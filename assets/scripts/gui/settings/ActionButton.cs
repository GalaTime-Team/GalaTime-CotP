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

            // Remove previous bind
            InputMap.ActionEraseEvents(ActionName);

            // Decode the stored value and create appropriate input event
            if (key > 0)
            {
                // Positive value: Keyboard key
                var @event = new InputEventKey() { PhysicalKeycode = (Key)key };
                InputMap.ActionAddEvent(ActionName, @event);
            }
            else if (key >= -999)
            {
                // Negative value -1 to -999: Mouse button
                var buttonIndex = (MouseButton)(-(int)key);
                var @event = new InputEventMouseButton() { ButtonIndex = buttonIndex };
                InputMap.ActionAddEvent(ActionName, @event);
            }
            else if (key >= -1999)
            {
                // Negative value -1000 to -1999: Joypad button
                var buttonIndex = (JoyButton)(-(int)key - 1000);
                var @event = new InputEventJoypadButton() { ButtonIndex = buttonIndex };
                InputMap.ActionAddEvent(ActionName, @event);
            }
            else
            {
                // Negative value -2000 and below: Joypad axis
                var axis = (JoyAxis)(-(int)key - 2000);
                var @event = new InputEventJoypadMotion() 
                { 
                    Axis = axis,
                    AxisValue = 0.5f // Use standard threshold
                };
                InputMap.ActionAddEvent(ActionName, @event);
            }

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
        // Accept keyboard keys, mouse buttons, and joypad buttons/axes
        // The Key property setter will handle adding to InputMap
        if (@event is InputEventKey keyEvent)
        {
            Key = (long)keyEvent.PhysicalKeycode;
        }
        else if (@event is InputEventMouseButton mouseEvent)
        {
            // Store mouse button as negative value (e.g., -1 for left mouse button)
            Key = -(long)mouseEvent.ButtonIndex;
        }
        else if (@event is InputEventJoypadButton joyEvent)
        {
            // Store joypad button (use values starting from -1000 to distinguish)
            Key = -1000 - (long)joyEvent.ButtonIndex;
        }
        else if (@event is InputEventJoypadMotion joyMotion)
        {
            // Store joypad axis (use values starting from -2000 to distinguish)
            // Note: We don't store the specific AxisValue to allow any threshold to trigger
            Key = -2000 - (long)joyMotion.Axis;
        }
        else
        {
            return; // Unsupported input type
        }
    }

    /// <summary> Displays the key of the bind on button. </summary>
    public void DisplayKey()
    {
        if (InputMap.ActionGetEvents(ActionName).Count > 0)
        {
            Text = InputMap.ActionGetEvents(ActionName)[0].AsText().Replace(" (Physical)", "");
        }
        else
        {
            Text = "None";
        }
    }
}
