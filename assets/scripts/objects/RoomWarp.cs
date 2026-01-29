using Godot;
using System;

namespace Galatime;

/// <summary>
/// Handles warping/teleporting between rooms/scenes.
/// Used with InteractiveTrigger to allow player to change scenes.
/// </summary>
public partial class RoomWarp : Node
{
	[Export] public string Scene { get; set; } = "";
	[Export] public int Room { get; set; } = 0;

	public override void _Ready()
	{
		base._Ready();
	}

	/// <summary>
	/// Called by InteractiveTrigger when player interacts.
	/// Changes to the target scene.
	/// </summary>
	public void Interact()
	{
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

		// Print confirmation
		GD.Print($"RoomWarp: Changing to room {Room} (Scene: {Scene})");
		
		// Change scene
		var error = GetTree().ChangeSceneToFile(Scene);
		if (error != Error.Ok)
		{
			GD.PrintErr($"RoomWarp: Failed to change scene. Error: {error}");
		}
	}
}
