using Godot;
using System;
using NodeExtensionMethods;
using Galatime.Global;

namespace Galatime;

/// <summary>
/// Handles warping/teleporting between rooms/scenes.
/// Automatically triggers when player enters the Area2D.
/// </summary>
public partial class RoomWarp : Node2D
{
	[Export] public string Scene { get; set; } = "";
	[Export] public int Room { get; set; } = 0;
	[Export] public float AnimationDuration = 0.5f;
	/// <summary> Determines the spawn point of the player in the next room. </summary>
	[Export(PropertyHint.Range, "0,255,1")] public byte PlayerSpawnPoint = 0;
	
	private Area2D TriggerArea;

	public override void _Ready()
	{
		base._Ready();
		
		// Get or create the trigger area
		TriggerArea = GetNodeOrNull<Area2D>("TriggerArea");
		if (TriggerArea != null)
		{
			TriggerArea.BodyEntered += OnEnter;
		}
		else
		{
			GD.PushWarning("RoomWarp: No TriggerArea child node found. Add an Area2D named 'TriggerArea' as a child.");
		}
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		if (TriggerArea != null)
		{
			TriggerArea.BodyEntered -= OnEnter;
		}
	}
	
	/// <summary>
	/// Called when a body enters the trigger area.
	/// Initiates room transition if the body is a possessed player character.
	/// </summary>
	private void OnEnter(Node node)
	{
		// Check if the node is a possessed character (player-controlled)
		if (!node.IsPossessed())
		{
			return;
		}
		
		GD.Print($"RoomWarp: Player entered, initiating transition to: {Scene}");
		
		// Cast to TestCharacter
		var character = node as TestCharacter;
		if (character == null)
		{
			GD.PrintErr("RoomWarp: Node is possessed but not TestCharacter, cannot transition");
			return;
		}
		
		// Validate that we have a scene to warp to
		if (string.IsNullOrEmpty(Scene))
		{
			GD.PrintErr("RoomWarp: Cannot warp - Scene path is not set");
			return;
		}

		// Verify scene file exists
		if (!ResourceLoader.Exists(Scene))
		{
			GD.PrintErr($"RoomWarp: Cannot warp - Scene file does not exist: {Scene}");
			return;
		}
		
		// Verify PlayerGui exists for fade animation
		if (PlayerVariables.Instance?.Player?.PlayerGui == null)
		{
			GD.PrintErr("RoomWarp: Cannot transition - PlayerGui not available");
			return;
		}
		
		// Disable character movement during transition
		character.CanMove = false;
		
		// Start fade animation
		GD.Print($"RoomWarp: Starting fade animation (duration: {AnimationDuration}s)");
		PlayerVariables.Instance.Player.PlayerGui.OnFade(true, AnimationDuration, OnFadeEnded);
	}
	
	private void OnFadeEnded()
	{
		GD.Print($"RoomWarp: Fade completed, loading scene: {Scene}");
		GD.Print($"RoomWarp: Setting spawn point index to: {PlayerSpawnPoint}");
		
		// Set spawn point for next room
		LevelManager.Instance.PlayerSpawnPointIndex = PlayerSpawnPoint;
		
		// Load the new scene
		var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
		globals.LoadScene(Scene);
		
		GD.Print("RoomWarp: Scene load initiated");
	}
}
