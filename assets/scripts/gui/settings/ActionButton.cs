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
        // Remove previous bind
        InputMap.ActionEraseEvents(ActionName);
        
        // Accept keyboard keys, mouse buttons, and joypad buttons/axes
        if (@event is InputEventKey keyEvent)
        {
            Key = (long)keyEvent.PhysicalKeycode;
            var newEvent = new InputEventKey() { PhysicalKeycode = keyEvent.PhysicalKeycode };
            InputMap.ActionAddEvent(ActionName, newEvent);
        }
        else if (@event is InputEventMouseButton mouseEvent)
        {
            // Store mouse button as a special key value (use negative values to distinguish from keyboard)
            Key = -(long)mouseEvent.ButtonIndex;
            var newEvent = new InputEventMouseButton() { ButtonIndex = mouseEvent.ButtonIndex };
            InputMap.ActionAddEvent(ActionName, newEvent);
        }
        else if (@event is InputEventJoypadButton joyEvent)
        {
            // Store joypad button (use values starting from -1000 to distinguish)
            Key = -1000 - (long)joyEvent.ButtonIndex;
            var newEvent = new InputEventJoypadButton() { ButtonIndex = joyEvent.ButtonIndex };
            InputMap.ActionAddEvent(ActionName, newEvent);
        }
        else if (@event is InputEventJoypadMotion joyMotion)
        {
            // Store joypad axis (use values starting from -2000 to distinguish)
            Key = -2000 - (long)joyMotion.Axis;
            var newEvent = new InputEventJoypadMotion() 
            { 
                Axis = joyMotion.Axis,
                AxisValue = joyMotion.AxisValue
            };
            InputMap.ActionAddEvent(ActionName, newEvent);
        }
        else
        {
            return; // Unsupported input type
        }
        
        DisplayKey();
        OnBound?.Invoke(Key);
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
