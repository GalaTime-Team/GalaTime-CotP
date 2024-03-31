using System.Collections.Generic;
using Godot;

namespace Galatime.Global;

/// <summary> Data of an enemy that registered in the game. </summary>
public class EnemyData
{
    /// <summary> Registered numeric ID of the enemy that uses to index and in save. </summary>
    public int NumID = 0;
    /// <summary> The standard name of the enemy. </summary>
    public string Name = "N/A";
    /// <summary> The path to the enemy that will be spawned. </summary>
    public string EnemyPath = null;
    /// <summary> The path to the icon that will be displayed in the UI. </summary>
    public string IconPath = null;
    /// <summary> The entry that will be displayed in the UI. </summary>
    public string Entry = "This enemy has no entry.";

    public EnemyData(int numID = 0, string name = "N/A", string enemyPath = null, string iconPath = null) => 
        (NumID, Name, EnemyPath, IconPath) = (numID, name, enemyPath, iconPath); 

    /// <summary> Retrieves the enemy's string representation of the ID. </summary>
    public string GetEnemyNumID() => NumID.ToString("000");
}

/// <summary> List of enemies with ID's and node path. </summary>
public partial class EnemiesList : Node 
{
    public const string ENEMIES_ENTRIES_PATH = "res://assets/data/enemyentries/";

    /// <summary> The list of enemies ID's and node path. </summary>
    public static readonly Dictionary<string, EnemyData> EnemiesData = new() 
    {
        { "slime", new(1, "Slime", "res://assets/objects/enemy/Slime.tscn", "res://assets/sprites/gui/enemyentries/slime_diary_icon.png") },
        { "rockant", new(2, "Rock Ant", "res://assets/objects/enemy/RockAnt.tscn", "res://assets/sprites/gui/enemyentries/rockant_diary_icon.png") },
        { "firecloak", new(3, "Firecloak", "res://assets/objects/enemy/Firecloak.tscn", "res://assets/sprites/gui/enemyentries/firecloak_diary_icon.png") }
    };

    public static int EnemiesCount => EnemiesData.Count;

    /// <summary> The loaded scenes of the enemies. </summary>
    public static Dictionary<string, PackedScene> Enemies { get; private set; } = new();

    public override void _Ready() 
    {
        // Adding the loaded scenes of the enemies.
        foreach (var item in EnemiesData)
        {
            Enemies.Add(item.Key, ResourceLoader.Load<PackedScene>(item.Value.EnemyPath));
            var entry = AssetsManager.Instance.GetTextFromFile(ENEMIES_ENTRIES_PATH + item.Key + ".txt");
            if (!string.IsNullOrEmpty(entry)) item.Value.Entry = entry;
        }
    }
}