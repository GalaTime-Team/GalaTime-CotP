using Godot;
using System;
using Galatime.Interfaces;

using System.Collections.Generic;

/// <summary> Represents a puzzle activator that keeps track of the puzzle and activates target object. </summary>
public partial class PuzzleActivator : Node
{
    GameLogger Logger = new("PuzzleActivator", GameLogger.ConsoleColor.Magenta);
    /// <summary> Set of conditions that must be met for the puzzle to be activated. </summary>
    public List<Func<bool>> Conditions = new();
    /// <summary> Callback when the puzzle is activated and conditions are met. </summary>
    public Action WhenActivated;
    [Export] public Node Activatable;
    [Export] public bool InverseActivation;

    /// <summary> Activates the puzzle if all conditions are met. </summary>
    public void Activate()
    {
        // Check if conditions are met and activate the puzzle if so.
        foreach (var condition in Conditions) if (!condition()) return;
        WhenActivated?.Invoke();

        Logger.Log("Puzzle activated", GameLogger.LogType.Success);

        if (Activatable is IActivatable activatable) activatable.Active = false ^ InverseActivation;
        else Logger.Log("Activatable object is not an IActivatable", GameLogger.LogType.Error);
    }

    public override void _ExitTree()
    {
        // Clear node after exit tree to prevent memory leaks
        WhenActivated = null;
        Activatable = null;
        Conditions.Clear();
    }
}
