using Godot;
using System;
using System.Collections.Generic;

/// <summary> Represents a cheat in the cheats menu. </summary>
/// <remarks> If cheat type is button, this cheat will always inactive. See <see cref="CheatType"/> and <see cref="Active"/>. </remarks>
public class Cheat 
{
    public enum CheatType
    {
        /// <summary> The cheat is switched on and off. </summary>
        Toggle,
        /// <summary> The cheat is invoked by pressing the button. </summary>
        Button
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    /// <summary> If the cheat is active. It will be always inactive if cheat type is button. </summary>
    public bool Active { get; private set; }
    /// <summary> The action input to activate the cheat. </summary>
    public string ActivationAction { get; private set; }
    /// <summary> When the cheat is activated. Returns if the cheat is active. </summary>
    public Action<bool> CheatAction { get; private set; }
    public CheatType Type { get; private set; } = CheatType.Toggle;

    /// <summary> Activates the cheat. If cheat type is toggle, it will be toggled. Button just calls action. </summary>
    public void ActivateCheat()
    {
        if (Type == CheatType.Toggle) Active = !Active;
        CheatAction?.Invoke(Active);
    }

    public Cheat(string id, string name, string activationAction, Action<bool> cheatAction, CheatType type = CheatType.Toggle) =>
        (Id, Name, ActivationAction, CheatAction, Type) = (id, name, activationAction, cheatAction, type);
}


/// <summary> Represents the cheats menu in the game. Used for debugging and testing. Also, it can be activated by the player. </summary>
public partial class CheatsMenu : Control
{
    private const string CHEATS_HEADER_STRING = "[center][color=green]Cheats is active :)[/color][/center]";

    public Button CheatButton;
    public VBoxContainer ListContainer;
    /// <summary> The list of cheats that registered. </summary>
    public List<Cheat> Cheats = new();
    public List<Control> CheatsControls = new();

    /// <summary> Returns the string representation of the given cheat with BBCode. </summary>
    public string CheatString(Cheat cheat) => $"{(GetActionKey(cheat.ActivationAction) != "N/A" ? $"[[color=cyan]{GetActionKey(cheat.ActivationAction)}[/color]] " : "")}{cheat.Name} {(cheat.Type != Cheat.CheatType.Toggle ? "" : $"({(cheat.Active ? "[color=green]ON[/color]" : "[color=red]OFF[/color]")})")}";
    /// <summary> Returns the key of the given action. </summary>
    public string GetActionKey(string action) => InputMap.ActionGetEvents(action).Count > 0 ? InputMap.ActionGetEvents(action)[0].AsText().Replace(" (Physical)", "") : "N/A";

    public override void _Ready() 
    {
        CheatButton = GetNode<Button>("Button");
        ListContainer = GetNode<VBoxContainer>("ListContainer");

        UpdateCheatList();
    }

    /// <summary> Registers multiple cheats to the list. </summary>
    public void RegisterCheat(params Cheat[] cheats)
    {
        Cheats.AddRange(cheats);
        UpdateCheatList();
    }

    /// <summary> Returns the cheat with the given name. Returns null if not found. </summary>
    public Cheat GetCheat(string id)
    {
        var cheat = Cheats.Find(c => c.Id == id);
        if (cheat == null) 
        {
            GD.PushWarning($"Cheat {id} was not found.");
            return null;
        }
        return cheat;
    }

    public override void _Process(double delta) 
    {
        // Toggles the cheats.
        foreach (var cheat in Cheats) 
        {
            if (InputMap.HasAction(cheat.ActivationAction) && Input.IsActionJustPressed(cheat.ActivationAction)) 
            {
                cheat.ActivateCheat();
                UpdateCheatList();
            }
        }
    }

    public RichTextLabel CreateCheatLabel(string text, Cheat cheat = null, bool interactable = false) 
    {
        var cheatButton = CheatButton.Duplicate() as Button;
        var cheatLabel = cheatButton.GetChild(0) as RichTextLabel;

        ListContainer.AddChild(cheatButton);
        CheatsControls.Add(cheatButton);
        cheatLabel.Clear();
        cheatLabel.AppendText(text);
        cheatButton.Visible = true;

        if (!interactable) 
        {
            cheatButton.Disabled = true;
        }
        else
        {
            cheatButton.Pressed += () => {
                cheat?.ActivateCheat();
                UpdateCheatList();
            };
        }

        return cheatLabel;
    }

    /// <summary> Updates the list of cheats in the UI. </summary>
    public void UpdateCheatList() 
    {
        CheatsControls.ForEach(cheat => cheat.QueueFree());
        CheatsControls.Clear();

        CreateCheatLabel(CHEATS_HEADER_STRING);
        CreateCheatLabel("");
        Cheats.ForEach(cheat => { 
            CreateCheatLabel(CheatString(cheat), cheat, true);
        });
    }
}
