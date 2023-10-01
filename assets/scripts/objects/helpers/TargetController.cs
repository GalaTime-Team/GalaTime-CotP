using Godot;
using Galatime;
using System.Collections.Generic;
using System.Linq;

namespace Galatime.Helpers;

public enum Teams { allies, enemies };

/// <summary> Controls the target controller to select a target. </summary>
public partial class TargetController : Node2D {
    /// <summary> Dictionary of teams and their strings (names). </summary>
    public static Dictionary<Teams, string> TeamNames = new() {
        { Teams.allies, "ally" },
        { Teams.enemies, "enemy" }
    };

    public static Dictionary<Teams, int> CollisionTeamLayers = new() {
        { Teams.allies, 2 },
        { Teams.enemies, 3 }
    };

    /// <summary> The target team to deal damage. Don't confuse with friendly fire, because this target to deal damage. Can be "allies" or "enemies". </summary>
    public Teams TargetTeam = Teams.enemies; 

    /// <summary> <see cref="TargetTeam"/>, but as a string to get team members. </summary>
    public string TargetTeamString => GetTeamNameByEnum(TargetTeam);

    public Node2D CurrentTarget = null;

    /// <summary> Overrides the target of the target. Mostly used for testing. </summary>
    public Node2D TargetOverride = null;

    public string GetTeamNameByEnum(Teams team) {
        return TeamNames[team];
    }

    public int GetCollisionLayerByEnum(Teams team) {
        return CollisionTeamLayers[team];
    }

    public override void _Process(double delta) {
        Entity enemy = null;

        if (TargetOverride != null) { 
            enemy = TargetOverride as Entity;
            CurrentTarget = enemy;
            return;
        }

        // Get all enemies in the scene of the target team.
        var enemies = GetTree().GetNodesInGroup(TargetTeamString);

        // Get all enemies and convert to non-typed list.
        List<Node> NonTypedEnemies = enemies.Cast<Node>().ToList();

        // Sort enemies by distance from closest to farthest.
        var sortedEnemies = NonTypedEnemies.OrderBy(x => x as Entity != null ? GlobalPosition.DistanceTo((x as Entity).GlobalPosition) : 0).ToList();
        
        // Remove all dead enemies.
        sortedEnemies.RemoveAll(x => x as Entity is not null && (x as Entity).DeathState);
        
        // Find the closest enemy and set it as the current target.
        if (sortedEnemies.ToList().Count > 0) enemy = sortedEnemies[0] as Entity; else enemy = null; 
        CurrentTarget = enemy;
    }
}