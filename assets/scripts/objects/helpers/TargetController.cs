using Godot;
using System.Collections.Generic;
using System.Linq;
using System;
using Galatime.Global;

namespace Galatime.Helpers;

/// <summary> Different teams. </summary>
public enum Teams { Allies, Enemies, Neutral };
/// <summary> Determines how the target controller selects a target. </summary>
public enum TargetingMode { Closest, Weakest };
/// <summary> Controls the target controller to select a target. </summary>
public partial class TargetController : Node2D
{
    /// <summary> Dictionary of teams and their strings (names). </summary>
    public static Dictionary<Teams, string> TeamNames = new() 
    {
        { Teams.Allies, "ally" },
        { Teams.Enemies, "enemy" }
    };

    public static Dictionary<Teams, int> CollisionTeamLayers = new() 
    {
        { Teams.Allies, 2 },
        { Teams.Enemies, 3 }
    };

    /// <summary> The target team to deal damage. Don't confuse with friendly fire, because this target to deal damage. Can be "allies" or "enemies". </summary>
    [Export]
    public Teams TargetTeam = Teams.Allies;

    /// <summary> Chooses how the target controller selects a target. </summary>
    /// <remarks> Weakest selects by average stats of all targets. </remarks>
    [Export]
    public TargetingMode TargetingMode = TargetingMode.Closest;

    /// <summary> <see cref="TargetTeamN"/>, but as a string to get team members. </summary>
    public string TargetTeamString => GetTeamNameByTeam(TargetTeam);

    /// <summary> The current target of the dependent entity. </summary>
    public Node2D CurrentTarget = null;

    /// <summary> Returns the current target rotation based on global position. </summary>
    /// <param name="globalPosition"> The current position of the dependent. </param>
    /// <returns> Rotation in radians. </returns>
    public float CurrentTargetRotationTo(Vector2 globalPosition) => CurrentTarget.GlobalPosition.AngleTo(globalPosition);

    /// <summary> Overrides the target of the target. Mostly used for testing. </summary>
    public Node2D TargetOverride = null;

    /// <summary> Gets the team name by enum. </summary>
    public string GetTeamNameByTeam(Teams team) => TeamNames[team];
    /// <summary> Gets the collision layer number by enum. </summary>
    public int GetCollisionLayerByTeam(Teams team) => CollisionTeamLayers[team];
    
    [Export]
    /// <summary> Represents the duration of the switching target. </summary>
    public float SwitchDuration = 0f;
    private float Timer = 0f;

    public override void _Process(double delta)
    {
        // Switch target if enough time has passed.
        Timer += (float)delta;
        if (Timer < SwitchDuration) return;
        Timer = 0f;

        // Override target if set.
        if (TargetOverride != null) 
        {
            CurrentTarget = TargetOverride as Entity;
            return;
        }

        // Get all enemies in the scene of the target team.
        var enemies = LevelManager.Instance.Entities.FindAll(x => x.Team == TargetTeam);

        // Get all enemies and convert to non-typed list.
        List<Node> NonTypedEnemies = enemies.Cast<Node>().ToList();

        // TODO: Implement targeting modes.

        // Sort enemies by distance from closest to farthest.
        var sortedEnemies = NonTypedEnemies.OrderBy(x => x as Entity != null ? GlobalPosition.DistanceTo((x as Entity).GlobalPosition) : 0).ToList();

        // Remove all dead enemies.
        sortedEnemies.RemoveAll(x => x as Entity is not null && (x as Entity).DeathState);

        // Find the closest enemy and set it as the current target.
        if (sortedEnemies.ToList().Count > 0) CurrentTarget = sortedEnemies[0] as Entity; else CurrentTarget = null;
    }
}