using Galatime.Global;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galatime;

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

	public string Id { get; }
	public string Name { get; }
	public string Description { get; }
	/// <summary> If the cheat is active. It will be always inactive if cheat type is button. </summary>
	public bool Active { get; private set; }
	/// <summary> The action input to activate the cheat. </summary>
	public string ActivationAction { get; }
	/// <summary> When the cheat is activated. Returns if the cheat is active. </summary>
	public Action<bool, string> CheatAction { get; }
	public CheatType Type { get; } = CheatType.Button;

	/// <summary> Activates the cheat. If cheat type is toggle, it will be toggled. Button just calls action. </summary>
	public void ActivateCheat(string inputText = "")
	{
		if (Type == CheatType.Separator) return;
		if (Type == CheatType.Toggle) Active = !Active;
		CheatAction?.Invoke(Active, inputText);
	}

	public Cheat(string id = "", string name = null, string description = null, string activationAction = null, Action<bool, string> cheatAction = null, CheatType type = CheatType.Button, bool active = false)
	{
		(Id, Name, Description, ActivationAction, CheatAction, Type, Active) = (id, name, description, activationAction, cheatAction, type, active);
		if (Type == CheatType.Separator) Name = $"[center][color=gray]-- {Name} --[/color][/center]";
		if (Description?.Length == 0) Description = "This cheat has no description.";
	}
}

/// <summary> Represents the cheats menu in the game. Used for debugging and testing. Also, it can be activated by the player. </summary>
public partial class CheatsMenu : Control
{
	public GameLogger Logger { get; private set; } = new("CheatsMenu", GameLogger.ConsoleColor.Lime);

	public enum LogLevel
	{
		Result,
		Warning,
		Error
	}

	#region Nodes
	public Button CheatButton;
	public VBoxContainer ListContainer;
	public LineEdit InputText;
	public LineEdit InputSearch;
	public RichTextLabel LogLabel;
	public Button MinimizeButton;
	public Control Window;
	public Label InfoLabel;

	public Tooltip Tooltip;
	#endregion

	#region Variables
	/// <summary> The list of cheats that registered. </summary>
	public List<Cheat> Cheats { get; private set; } = new();
	public List<Control> CheatButtons { get; private set; } = new();

	private int SameLogsCount = 0;
	private string PreviousLogText = "";
	private int PreviousFocusIndex = 0;

	private double TimerLog = 0;

	private bool shown;
	/// <summary> If cheats menu is shown. </summary>
	public bool Shown
	{
		get => shown;
		set
		{
			if (!Activated) return; // Don't show cheats menu if cheats menu is not even activated.
			if (!WindowManager.Instance.ToggleWindow("cheats", Shown, () => { Shown = false; }, canOverlay: true)) return;

			shown = value;
			Window.Visible = shown;

			// Pause only when this cheat is active.
			GetTree().Paused = shown && GetCheat("pause_when_cheats_active").Active;

			if (GetCheat("clear_search_when_toggled").Active) InputSearch.Clear(); // To clear previous search results.
			// Focus on the first cheat.
			CheatButtons[0].GrabFocus();
		}
	}

	private bool activated;
	/// <summary> If the cheats menu is activated. Don't be confused with <see cref="Shown"/>, because activated means that cheats menu is functional and can be enabled. </summary>
	public bool Activated
	{
		get => activated;
		set
		{
			var enabled = GalatimeGlobals.CMDArgs.ContainsKey("cheats"); // Activate cheats only if cheats are defined in the command line.
			activated = value && enabled;

			Visible = activated;
		}
	}
	#endregion

	/// <summary> Returns the string representation of the given cheat. </summary>
	public static string CheatString(Cheat cheat)
	{
		var actionKey = GetActionKey(cheat.ActivationAction);
		var activationAction = actionKey != "N/A" ? $"[[color=cyan]{actionKey}[/color]] " : "";
		var toggleStatus = cheat.Active ? "[color=green]ON[/color]" : "[color=red]OFF[/color]";
		var toggle = cheat.Type == Cheat.CheatType.Toggle ? $"({toggleStatus}) " : "";
		var input = cheat.Type == Cheat.CheatType.Input ? "[color=gray](Input)[/color]" : "";

		return $"{activationAction}{cheat.Name} {toggle}{input}";
	}
	/// <summary> Returns the key of the given action. </summary>
	public static string GetActionKey(string action) => InputMap.ActionGetEvents(action).Count > 0 ? InputMap.ActionGetEvents(action)[0].AsText().Replace(" (Physical)", "") : "N/A";

	public override void _Ready()
	{
		#region Get nodes
		CheatButton = GetNode<Button>("Window/Button");
		ListContainer = GetNode<VBoxContainer>("Window/VSplitContainer/ScrollContainer/ListContainer");
		InputText = GetNode<LineEdit>("Window/InputText");
		InputSearch = GetNode<LineEdit>("Window/InputSearch");
		LogLabel = GetNode<RichTextLabel>("Window/VSplitContainer/LogLabel");
		MinimizeButton = GetNode<Button>("MinimizeButton");
		Window = GetNode<Control>("Window");
		InfoLabel = GetNode<Label>("InfoLabel");

		Tooltip = WindowManager.Instance.Tooltip;
		#endregion

		Activated = true; // Try to activate cheats.

		MinimizeButton.Pressed += () => Shown = !Shown;
		InputSearch.TextChanged += (string s) => CheatSearchUpdate(s);

		// Register basic cheats.
		RegisterCheat(
			new Cheat(name: "[color=yellow]Cheats menu cheats[/color]", type: Cheat.CheatType.Separator),
			new Cheat("pause_when_cheats_active", "Pause when cheats are active", "Pauses the game when cheats menu are shown.", "", (active, _) =>
			{
				GetTree().Paused = Shown && active;
			}, Cheat.CheatType.Toggle, true),
			new Cheat("clear_search_when_toggled", "Clear search when toggled", "Clears search input text box when toggled.", "", null, Cheat.CheatType.Toggle, false),
			new Cheat("help_cheat", "Help for cheat", "Show help for this cheat.", "", (_, input) =>
			{
				var inputArguments = ParseCheatArguments(input, 1);

				var args = inputArguments.args;
				if (!inputArguments.result) return;

				var cheat = GetCheat(args[0]);
				if (cheat == null)
				{
					Log($"Cheat {args[0]} not found", LogLevel.Warning);
					return;
				}

				Log(GetCheatHelpString(cheat), LogLevel.Result);
			}, Cheat.CheatType.Input),
			new Cheat("show_cheats", "Show cheats", "Show help for cheats.", "", (_, _) =>
			{
				var cheats = Cheats;
				var str = new StringBuilder();

				str.Append("\n\n[center][color=yellow]Help for cheats[/color][/center]\nIf cheat has in his name \"[color=gray](Input)[/color]\", it means that it can be used to input textbox.\n\n");
				for (var i = 0; i < cheats.Count; i++)
				{
					var cheat = cheats[i];
					// No need to show separators.
					if (cheat.Type == Cheat.CheatType.Separator) continue;

					// Add name and description.
					str.AppendLine(GetCheatHelpString(cheat));
				}
				// Remove the last three lines to make it look nice.
				str.Remove(str.Length - 3, 3);

				// Show the cheats in the log.
				Log(str.ToString(), LogLevel.Result);
			}),
			new Cheat("clear_logs", "Clear logs", "Resets the log.", "", (_, _) => LogLabel.Clear())
		);
	}

	/// <summary> Returns the help string for the given cheat. </summary>
	public static string GetCheatHelpString(Cheat cheat) => $"[color=white]{cheat.Name} ({cheat.Id})\n[color=dark_slate_gray]{cheat.Description} [color=dark_gray]Bind: {GetActionKey(cheat.ActivationAction)}";

	public override void _Process(double delta)
	{
		if (TimerLog < .5) TimerLog += delta;
		if (TimerLog >= .5)
		{
			var quests = QuestManager.Instance.CurrentQuests;

			var str = new StringBuilder();
			str.Append("FPS: ").Append(Engine.GetFramesPerSecond()).Append('\n')
			.Append("Current quests: ").Append(quests.Count != 0 ? quests.Select(x => x.Value.Name).Aggregate((x, y) => $"{x}, {y}") : "None");

			InfoLabel.Text = str.ToString();

			TimerLog = 0;
		}

		// Show/hide the cheats menu by pressing the key.
		if (Input.IsActionJustPressed("cheats_menu")) Shown = !Shown;

		// Toggles the cheats by pressing the activation action.
		foreach (var cheat in Cheats)
		{
			if (InputMap.HasAction(cheat.ActivationAction) && Input.IsActionJustPressed(cheat.ActivationAction) && Activated)
				ActivateCheat(cheat);
		}
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
		var formattedText = $"[color={color}][{level}] [color=white]{text} {(SameLogsCount > 0 ? $"({SameLogsCount + 1}x)" : "")}";
		LogLabel.AppendText(formattedText);
		Logger.Log(formattedText);
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

	public Button CreateCheatLabel(string text, Cheat cheat = null, bool interactable = true)
	{
		if (string.IsNullOrEmpty(text)) throw new ArgumentException($"'{nameof(text)}' cannot be null or empty.", nameof(text));

		// Duplicating base button.
		var cheatButton = CheatButton.Duplicate() as Button;

		ListContainer.AddChild(cheatButton);
		CheatButtons.Add(cheatButton);

		cheatButton.Visible = true;

		if (!interactable)
		{
			// No need to press the button if it's not interactable.
			cheatButton.Disabled = true;
		}
		else
		{
			cheatButton.MouseEntered += () => Tooltip.Display(cheat);
			cheatButton.MouseExited += () => Tooltip.Hide();
			cheatButton.Pressed += () => ActivateCheat(cheat);
		}

		return cheatButton;
	}

	public void UpdateCheatLabel(int index, string text)
	{
		var cheatButton = CheatButtons[index];

		// Getting the label.
		var cheatLabel = cheatButton.GetChild(0) as RichTextLabel;
		cheatLabel.Clear();
		cheatLabel.AppendText(text);
	}

	private void ActivateCheat(Cheat cheat)
	{
		var text = InputText.Text;

		// Clear any previous log.
		LogLabel.Clear();

		cheat.ActivateCheat(text);
		UpdateCheatList();
	}

	public void UpdateCheatList()
	{
		for (int i = 0; i < Cheats.Count; i++)
		{
			var cheat = Cheats[i];
			if (CheatButtons.Count <= i) CreateCheatLabel(CheatString(cheat), cheat, cheat.Type != Cheat.CheatType.Separator);
			UpdateCheatLabel(i, CheatString(cheat));
		}
	}

	/// <summary> Performs a cheat search update based on the specified keyword. </summary>
	/// <param name="keyword"> The keyword to search for. </param>
	public void CheatSearchUpdate(string keyword)
	{
		// Counting the number of matches.
		var listOfMatches = new List<string>();

		for (int i = 0; i < Cheats.Count; i++)
		{
			// Since these lists have the same length, we can use the same index for both.
			var cheat = Cheats[i];
			var button = CheatButtons[i];

			button.Visible = true;

			// Search for the keyword by checking if it contains all characters of the id.
			var normalizedKeyword = keyword.Replace(" ", "").ToLower();
			var containsAllCharacters = normalizedKeyword.All(c => cheat.Id.ToLower().Contains(c));

			// Hide button if it doesn't match with the requirements.
			if (!containsAllCharacters) button.Visible = false;
			else listOfMatches.Add(cheat.Id);
		}

		listOfMatches.RemoveAll(x => string.IsNullOrWhiteSpace(x)); // Remove empty strings.

		// No matches found case.
		if (listOfMatches.Count == 0)
		{
			Log("No matches found", LogLevel.Warning);
			return;
		}

		Log($"Cheat search: {string.Join(", ", listOfMatches)}", LogLevel.Result);
	}

	/// <summary> Parse cheat arguments. If failed it shows an error in cheats menu log. </summary>
	/// <param name="requiredArgs"> The number of required arguments. If the number of arguments is not equal to this value, the cheat is not executed. </param>
	/// <returns> The parsed arguments and a boolean indicating if the parsing was successful. </returns>
	public (string[] args, bool result) ParseCheatArguments(string args, int requiredArgs = 0)
	{
		if (args is null || args.Length == 0)
		{
			Log("No arguments", LogLevel.Error);
			return (Array.Empty<string>(), false);
		}

		var inputArguments = new List<string>();
		var currentArgument = "";
		var inQuotes = false;

		for (var i = 0; i < args.Length; i++)
		{
			var c = args[i];

			if (c == '"')
			{
				inQuotes = !inQuotes;
				continue;
			}

			if (c == ' ' && !inQuotes)
			{
				inputArguments.Add(currentArgument);
				currentArgument = "";
				continue;
			}

			currentArgument += c;
		}

		inputArguments.Add(currentArgument);

		if (inputArguments.Count < requiredArgs)
		{
			Log("Invalid number of arguments", LogLevel.Error);
			return (Array.Empty<string>(), false);
		}

		return (inputArguments.ToArray(), true);
	}



	/// <summary> Returns the player from the PlayerVariables. If the player is invalid, it shows an error in cheats menu log. </summary>
	public Player GetPlayer()
	{
		var pv = PlayerVariables.Instance;
		var player = pv.Player;

		if (player == null || !IsInstanceValid(player))
		{
			Log("Player is invalid", LogLevel.Error);
			return null;
		}

		return player;
	}

}
