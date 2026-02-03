namespace Galatime;

using Galatime.Global;
using Godot;
using NodeExtensionMethods;


/// <summary>
/// Represents a trigger, which transitions to a new room (Scene)
/// </summary>
/// <remarks> Don't confuse with <see cref="GalatimeGlobals.LoadScene(string)"/>, because it's loads a scene, but that node is trigger for the room transition </remarks>
[Tool] public partial class Roomwrap : Node2D
{
	private string scene = "";
	[Export(PropertyHint.File, "*.tscn")] public string Scene
	{
		get => scene;
		set
		{
			scene = value;
			UpdateConfigurationWarnings();
		}
	}
	[Export] public float AnimationDuration = 0.5f;
	/// <summary> Determines the spawn point of the player in the next room. </summary>
	[Export(PropertyHint.Range, "0,255,1")] public byte PlayerSpawnPoint = 0;
	private Area2D TriggerArea;

	public override void _Ready()
	{
		if (Engine.IsEditorHint()) return;
		TriggerArea = GetNode<Area2D>("TriggerArea");
		TriggerArea.BodyEntered += OnEnter;
	}

	public override void _ExitTree() 
	{
		if (Engine.IsEditorHint()) return;
		TriggerArea.BodyEntered -= OnEnter;
	}

	private void OnEnter(Node node)
	{
		// Check if the node is a possessed character (player-controlled)
		if (!node.IsPossessed())
		{
			return;
		}
		
		GD.Print($"Roomwrap: Player entered portal trigger, initiating transition to: {Scene}");
		
		// Cast to TestCharacter (HumanoidCharacter base class)
		var character = node as TestCharacter;
		if (character == null)
		{
			GD.PrintErr("Roomwrap: Node is possessed but not TestCharacter, cannot transition");
			return;
		}
		
		// Verify we have a valid scene to load
		if (string.IsNullOrEmpty(Scene))
		{
			GD.PrintErr("Roomwrap: Cannot transition - Scene path is not set");
			return;
		}
		
		// Verify PlayerGui exists for fade animation
		if (PlayerVariables.Instance?.Player?.PlayerGui == null)
		{
			GD.PrintErr("Roomwrap: Cannot transition - PlayerGui not available");
			return;
		}
		
		// Disable character movement during transition
		character.CanMove = false;
		
		// Start fade animation
		GD.Print($"Roomwrap: Starting fade animation (duration: {AnimationDuration}s)");
		PlayerVariables.Instance.Player.PlayerGui.OnFade(true, AnimationDuration, OnFadeEnded);
	}
	private void OnFadeEnded()
	{
		GD.Print($"Roomwrap: Fade completed, loading scene: {Scene}");
		GD.Print($"Roomwrap: Setting spawn point index to: {PlayerSpawnPoint}");
		
		// Set spawn point for next room
		LevelManager.Instance.PlayerSpawnPointIndex = PlayerSpawnPoint;
		
		// Disable position restore from save during room transitions (use spawn point instead)
		PlayerVariables.Instance.ShouldRestorePosition = false;
		
		// Load the new scene
		var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
		globals.LoadScene(Scene);
		
		GD.Print("Roomwrap: Scene load initiated");
	}

	public override string[] _GetConfigurationWarnings()
	{
		if (Scene.Length == 0)
			return new string[] { "Please specify a scene or it will not be loaded" };
		else
			return System.Array.Empty<string>();
	}
}
