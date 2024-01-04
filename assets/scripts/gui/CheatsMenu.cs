using ExtensionMethods;
using Galatime;
using Godot;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary> Represents a cheat in the cheats menu. </summary>
/// <remarks> If cheat type is button, this cheat will always inactive. See <see cref="CheatType"/> and <see cref="Active"/>. </remarks>
public class Cheat
{
    public enum CheatType
    {
        /// <summary> The cheat is switched on and off. </summary>
        Toggle,
        /// <summary> The cheat is invoked by pressing the button. </summary>
        Button,
        /// <summary> The cheat is invoked by text input and by pressing the button. </summary>
        Input,
        /// <summary> Just appearance thing. Separates cheats in the list and doesn't do anything. </summary>
        Separator
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    /// <summary> If the cheat is active. It will be always inactive if cheat type is button. </summary>
    public bool Active { get; private set; }
    /// <summary> The action input to activate the cheat. </summary>
    public string ActivationAction { get; private set; }
    /// <summary> When the cheat is activated. Returns if the cheat is active. </summary>
    public Action<bool, string> CheatAction { get; private set; }
    public CheatType Type { get; private set; } = CheatType.Button;

    /// <summary> Activates the cheat. If cheat type is toggle, it will be toggled. Button just calls action. </summary>
    public void ActivateCheat(string inputText = "")
    {
        if (Type == CheatType.Separator) return;
        if (Type == CheatType.Toggle) Active = !Active;
        CheatAction?.Invoke(Active, inputText);
    }

    public Cheat(string id = null, string name = null, string description = null, string activationAction = null, Action<bool, string> cheatAction = null, CheatType type = CheatType.Button) 
    {
        (Id, Name, Description, ActivationAction, CheatAction, Type) = (id, name, description, activationAction, cheatAction, type);
        if (Type == CheatType.Separator) Name = $"[center][color=gray]-- {Name} --[/color][/center]";
        if (Description == "") Description = "This cheat has no description.";
    }
}   


/// <summary> Represents the cheats menu in the game. Used for debugging and testing. Also, it can be activated by the player. </summary>
public partial class CheatsMenu : Control
{
    public enum LogLevel
    {
        Result,
        Warning,
        Error
    }

    private const string CHEATS_HEADER_STRING = "[center][color=green]Cheats is active :)[/color][/center]";

    #region Nodes
    public Button CheatButton;
    public VBoxContainer ListContainer;
    public LineEdit InputText;
    public RichTextLabel LogLabel;
    public Button MinimizeButton;
    public Control Window;
    #endregion

    #region Variables
    /// <summary> The list of cheats that registered. </summary>
    public List<Cheat> Cheats { get; private set; } = new();
    public List<Control> CheatsControls { get; private set; } = new();

    private int SameLogsCount = 0;
    private string PreviousLogText = "";
    #endregion

    /// <summary> Returns the string representation of the given cheat. </summary>
    public string CheatString(Cheat cheat)
    {
        var actionKey = GetActionKey(cheat.ActivationAction);
        var activationAction = actionKey != "N/A" ? $"[[color=cyan]{actionKey}[/color]] " : "";
        var toggleStatus = cheat.Active ? "[color=green]ON[/color]" : "[color=red]OFF[/color]";
        var toggle = cheat.Type == Cheat.CheatType.Toggle ? $"({toggleStatus}) " : "";
        var input = cheat.Type == Cheat.CheatType.Input ? "[color=gray](Input)[/color]" : "";

        return $"{activationAction}{cheat.Name} {toggle}{input}";
    }
    /// <summary> Returns the key of the given action. </summary>
    public string GetActionKey(string action) => InputMap.ActionGetEvents(action).Count > 0 ? InputMap.ActionGetEvents(action)[0].AsText().Replace(" (Physical)", "") : "N/A";

    public override void _Ready()
    {
        #region Get nodes
        CheatButton = GetNode<Button>("Window/Button");
        ListContainer = GetNode<VBoxContainer>("Window/VSplitContainer/ScrollContainer/ListContainer");
        InputText = GetNode<LineEdit>("Window/InputText");
        LogLabel = GetNode<RichTextLabel>("Window/VSplitContainer/LogLabel");
        MinimizeButton = GetNode<Button>("MinimizeButton");
        Window = GetNode<Control>("Window");
        #endregion

        MinimizeButton.Pressed += () => Window.Visible = !Window.Visible;

        // Register basic cheats.
        RegisterCheat(
            new Cheat(name: "[color=yellow]Cheats menu cheats[/color]", type: Cheat.CheatType.Separator),
            new Cheat("clear_logs", "Clear logs", "Resets the log.", "", (_, _) => LogLabel.Clear()),
            new Cheat("show_cheats", "Show cheats", "Show help for cheats.", "", (_, _) => {
                var cheats = Cheats;
                var str = new StringBuilder();

                str.Append("\n\n[center][color=yellow]Help for cheats[/color][/center]\nIf cheat has in his name \"[color=gray](Input)[/color]\", it means that it can be used to input textbox.\n\n");
                for (var i = 0; i < cheats.Count; i++) 
                {
                    var cheat = cheats[i];
                    // No need to show separators.
                    if (cheat.Type == Cheat.CheatType.Separator) continue;

                    // Add name and description.
                    str.AppendLine($"[color=white]{cheat.Name} ({cheat.Id})");
                    str.AppendLine($"[color=dark_slate_gray]{cheat.Description} [color=dark_gray]Bind: {GetActionKey(cheat.ActivationAction)}\n");
                }
                // Remove the last three lines to make it look nice.
                str.Remove(str.Length - 3, 3);

                // Show the cheats in the log.
                Log(str.ToString(), LogLevel.Result);
            })
        );

        UpdateCheatList();
    }

    /// <summary> Registers multiple cheats to the list. </summary>
    public void RegisterCheat(params Cheat[] cheats)
    {
        Cheats.AddRange(cheats);
        UpdateCheatList();
    }

    /// <summary> Logs the given text with the given log level in the log label. </summary>
    public void Log(string text, LogLevel level = LogLevel.Result)
    {
        // Count the same logs in a row to show it in the log label and not be confusing.
        if (text == PreviousLogText) SameLogsCount++;
        else SameLogsCount = 0;

        PreviousLogText = text;

        LogLabel.Clear();
        var color = level switch
        {
            LogLevel.Result => "green",
            LogLevel.Warning => "yellow",
            LogLevel.Error => "red",
            _ => "white"
        };
        LogLabel.AppendText($"[color={color}][{level}] [color=white]{text} {(SameLogsCount > 0 ? $"({SameLogsCount + 1}x)" : "")}");
    }

    /// <summary> Returns the cheat with the given id. Returns null if not found. </summary>
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
        // Toggles the cheats by pressing the activation action.
        foreach (var cheat in Cheats)
        {
            if (InputMap.HasAction(cheat.ActivationAction) && Input.IsActionJustPressed(cheat.ActivationAction)) ActivateCheat(cheat);
        }
    }

    public RichTextLabel CreateCheatLabel(string text, Cheat cheat = null, bool interactable = true)
    {
        // Duplicating base button.
        var cheatButton = CheatButton.Duplicate() as Button;
        // Getting the label.
        var cheatLabel = cheatButton.GetChild(0) as RichTextLabel;

        ListContainer.AddChild(cheatButton);
        CheatsControls.Add(cheatButton);
        cheatLabel.Clear();
        cheatLabel.AppendText(text);
        cheatButton.Visible = true;

        if (!interactable)
        {
            // No need to press the button if it's not interactable.
            cheatButton.Disabled = true;
        }
        else
        {
            cheatButton.Pressed += () => ActivateCheat(cheat);
        }

        return cheatLabel;
    }

    private void ActivateCheat(Cheat cheat)
    {
        var text = InputText.Text;

        // Clear any previous log.
        LogLabel.Clear();

        cheat.ActivateCheat(text);
        UpdateCheatList();
    }

    /// <summary> Updates the list of cheats in the UI. </summary>
    public void UpdateCheatList()
    {
        CheatsControls.ForEach(cheat => cheat.QueueFree());
        CheatsControls.Clear();

        // Add cheats to the list. Disable interaction with separators.
        Cheats.ForEach(cheat => CreateCheatLabel(CheatString(cheat), cheat, cheat.Type != Cheat.CheatType.Separator));
    }

    /// <summary> Parse cheat arguments. If failed it shows an error in cheats menu log. </summary>
    /// <param name="requiredArgs"> The number of required arguments. If the number of arguments is not equal to this value, the cheat is not executed. </param>
    /// <returns> The parsed arguments and a boolean indicating if the parsing was successful. </returns>
    public (string[] args, bool result) ParseCheatArguments(string args, int requiredArgs = 0)
    {
        if (args == "")
        {
            Log("No arguments", LogLevel.Error);
            return (new string[0], false);
        }
        var inputArguments = args.Split(' ');
        if (inputArguments.Length != requiredArgs)
        {
            Log($"Invalid number of arguments", LogLevel.Error);
            return (new string[0], false);
        }
        return (inputArguments, true);
    }

    /// <summary> Returns the player from the PlayerVariables. If the player is invalid, it shows an error in cheats menu log. </summary>
    public Player GetPlayer()
    {
        var pv = PlayerVariables.Instance;
        var player = pv.Player;

        if (player == null || !IsInstanceValid(player))
        {
            Log($"Player is invalid", LogLevel.Error);
            return null;
        }

        return player;
    }
}
