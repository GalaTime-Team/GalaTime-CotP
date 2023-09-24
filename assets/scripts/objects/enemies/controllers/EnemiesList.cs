using System.Collections.Generic;
using Godot;

/// <summary> List of enemies with ID's and node path. </summary>
public partial class EnemiesList : Node {
    /// <summary> The list of enemies ID's and node path. </summary>
    private static readonly Dictionary<string, string> EnemyPaths = new() {
        { "slime", "res://assets/objects/enemy/Slime.tscn" },
        { "shootingbuddy", "res://assets/objects/enemy/ShootingBuddy.tscn" }
    };

    /// <summary> The loaded scenes of the enemies. </summary>
    public static Dictionary<string, PackedScene> Enemies { get; private set; } = new();

    public override void _Ready() {
        // Adding the loaded scenes of the enemies.
        foreach (var item in EnemyPaths) Enemies.Add(item.Key, ResourceLoader.Load<PackedScene>(item.Value));
    }
}