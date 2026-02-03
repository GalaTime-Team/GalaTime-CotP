using Godot;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Galatime;

/// <summary>
/// Represents a single inventory item in saved data.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SavedInventoryItem
{
    [JsonProperty("id")]
    public string ID { get; set; } = "";
    
    [JsonProperty("quantity")]
    public int Quantity { get; set; } = 0;
    
    [JsonProperty("slot")]
    public int Slot { get; set; } = -1;
}

/// <summary>
/// Represents a single ability slot in saved data.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SavedAbility
{
    [JsonProperty("id")]
    public string ID { get; set; } = "";
    
    [JsonProperty("slot")]
    public int Slot { get; set; } = -1;
}

/// <summary>
/// Represents a saved level object state.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SavedLevelObject
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";
    
    [JsonProperty("data")]
    public object[] Data { get; set; } = System.Array.Empty<object>();
}

/// <summary>
/// Represents saved level state data.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SavedLevelState
{
    [JsonProperty("level_name")]
    public string LevelName { get; set; } = "";
    
    [JsonProperty("objects")]
    public List<SavedLevelObject> Objects { get; set; } = new();
}

/// <summary>
/// Represents the player state in saved data.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SavedPlayerState
{
    [JsonProperty("health")]
    public float Health { get; set; } = 100f;
    
    [JsonProperty("mana")]
    public float Mana { get; set; } = 100f;
    
    [JsonProperty("stamina")]
    public float Stamina { get; set; } = 100f;
    
    [JsonProperty("xp")]
    public int Xp { get; set; } = 0;
}

/// <summary>
/// Represents the complete save data structure for the game.
/// Contains all information needed to restore the game state from a save file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SaveData
{
    [JsonProperty("version")]
    public int Version { get; set; } = 1;
    
    [JsonProperty("id")]
    public int ID { get; set; } = 0;
    
    [JsonProperty("chapter")]
    public int Chapter { get; set; } = 1;
    
    [JsonProperty("day")]
    public int Day { get; set; } = 1;
    
    [JsonProperty("playtime")]
    public float Playtime { get; set; } = 0f;
    
    [JsonProperty("current_scene")]
    public string CurrentScene { get; set; } = "res://assets/scenes/Lobby.tscn";
    
    [JsonProperty("spawn_point_index")]
    public int SpawnPointIndex { get; set; } = 0;
    
    [JsonProperty("player_state")]
    public SavedPlayerState PlayerState { get; set; } = new();
    
    [JsonProperty("learned_abilities")]
    public List<string> LearnedAbilities { get; set; } = new();
    
    [JsonProperty("equipped_abilities")]
    public List<SavedAbility> EquippedAbilities { get; set; } = new();
    
    [JsonProperty("inventory")]
    public List<SavedInventoryItem> Inventory { get; set; } = new();
    
    [JsonProperty("allies")]
    public List<string> Allies { get; set; } = new();
    
    [JsonProperty("discovered_enemies")]
    public List<int> DiscoveredEnemies { get; set; } = new();
    
    [JsonProperty("level_states")]
    public List<SavedLevelState> LevelStates { get; set; } = new();
    
    /// <summary>
    /// Indicates whether this is an empty/new save slot with no actual data.
    /// </summary>
    public bool IsEmpty => Version == 1 && Chapter == 1 && Day == 1 && 
                          Playtime == 0 && LearnedAbilities.Count == 0 && 
                          Inventory.Count == 0 && string.IsNullOrEmpty(CurrentScene) ||
                          CurrentScene == "res://assets/scenes/Lobby.tscn" && Playtime == 0;
    
    /// <summary>
    /// Creates a new SaveData instance with default values.
    /// </summary>
    public SaveData() { }
    
    /// <summary>
    /// Converts the save data to a JSON string.
    /// </summary>
    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
    
    /// <summary>
    /// Creates a SaveData instance from a JSON string.
    /// </summary>
    public static SaveData FromJson(string json)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<SaveData>(json);
            return data ?? new SaveData();
        }
        catch (JsonException e)
        {
            GD.PrintErr($"Error parsing save data: {e.Message}");
            return new SaveData();
        }
    }
    
    /// <summary>
    /// Creates a SaveData instance from a Godot Dictionary (for backwards compatibility).
    /// </summary>
    public static SaveData FromDictionary(Godot.Collections.Dictionary dict)
    {
        var data = new SaveData();
        
        if (dict == null || dict.Count == 0) return data;
        
        // Basic info - use Variant API for safe type conversion
        if (dict.ContainsKey("id")) data.ID = dict["id"].AsInt32();
        if (dict.ContainsKey("chapter")) data.Chapter = dict["chapter"].AsInt32();
        if (dict.ContainsKey("day")) data.Day = dict["day"].AsInt32();
        if (dict.ContainsKey("playtime")) data.Playtime = dict["playtime"].AsSingle();
        if (dict.ContainsKey("current_scene")) data.CurrentScene = dict["current_scene"].AsString();
        if (dict.ContainsKey("spawn_point_index")) data.SpawnPointIndex = dict["spawn_point_index"].AsInt32();
        
        // Player state
        if (dict.ContainsKey("player_state") && dict["player_state"].VariantType == Variant.Type.Dictionary)
        {
            var playerState = dict["player_state"].AsGodotDictionary();
            if (playerState.ContainsKey("health")) data.PlayerState.Health = playerState["health"].AsSingle();
            if (playerState.ContainsKey("mana")) data.PlayerState.Mana = playerState["mana"].AsSingle();
            if (playerState.ContainsKey("stamina")) data.PlayerState.Stamina = playerState["stamina"].AsSingle();
            if (playerState.ContainsKey("xp")) data.PlayerState.Xp = playerState["xp"].AsInt32();
        }
        else if (dict.ContainsKey("xp"))
        {
            // Legacy support: XP was stored at root level
            data.PlayerState.Xp = dict["xp"].AsInt32();
        }
        
        // Learned abilities
        if (dict.ContainsKey("learned_abilities") && dict["learned_abilities"].VariantType == Variant.Type.Array)
        {
            var abilities = dict["learned_abilities"].AsGodotArray();
            foreach (var ab in abilities)
            {
                var str = ab.AsString();
                if (!string.IsNullOrEmpty(str)) data.LearnedAbilities.Add(str);
            }
        }
        
        // Equipped abilities
        if (dict.ContainsKey("equipped_abilities") && dict["equipped_abilities"].VariantType == Variant.Type.Dictionary)
        {
            var equippedDict = dict["equipped_abilities"].AsGodotDictionary();
            foreach (var key in equippedDict.Keys)
            {
                int slot = key.AsInt32();
                if (equippedDict[key].VariantType == Variant.Type.Dictionary)
                {
                    var abilityDict = equippedDict[key].AsGodotDictionary();
                    if (abilityDict.ContainsKey("id"))
                    {
                        data.EquippedAbilities.Add(new SavedAbility
                        {
                            ID = abilityDict["id"].AsString(),
                            Slot = slot
                        });
                    }
                }
            }
        }
        
        // Inventory
        if (dict.ContainsKey("inventory") && dict["inventory"].VariantType == Variant.Type.Dictionary)
        {
            var inventoryDict = dict["inventory"].AsGodotDictionary();
            foreach (var key in inventoryDict.Keys)
            {
                int slot = key.AsInt32();
                if (inventoryDict[key].VariantType == Variant.Type.Dictionary)
                {
                    var itemDict = inventoryDict[key].AsGodotDictionary();
                    if (itemDict.ContainsKey("id"))
                    {
                        data.Inventory.Add(new SavedInventoryItem
                        {
                            ID = itemDict["id"].AsString(),
                            Quantity = itemDict.ContainsKey("quantity") ? itemDict["quantity"].AsInt32() : 1,
                            Slot = slot
                        });
                    }
                }
            }
        }
        
        // Allies
        if (dict.ContainsKey("allies") && dict["allies"].VariantType == Variant.Type.Array)
        {
            var alliesArray = dict["allies"].AsGodotArray();
            foreach (var ally in alliesArray)
            {
                var str = ally.AsString();
                if (!string.IsNullOrEmpty(str)) data.Allies.Add(str);
            }
        }
        
        // Discovered enemies
        if (dict.ContainsKey("discovered_enemies") && dict["discovered_enemies"].VariantType == Variant.Type.Array)
        {
            var enemiesArray = dict["discovered_enemies"].AsGodotArray();
            foreach (var enemy in enemiesArray)
            {
                data.DiscoveredEnemies.Add(enemy.AsInt32());
            }
        }
        
        // Level states
        if (dict.ContainsKey("level_states") && dict["level_states"].VariantType == Variant.Type.Array)
        {
            var statesArray = dict["level_states"].AsGodotArray();
            foreach (var state in statesArray)
            {
                if (state.VariantType == Variant.Type.Dictionary)
                {
                    var stateDict = state.AsGodotDictionary();
                    var levelState = new SavedLevelState();
                    if (stateDict.ContainsKey("level_name")) levelState.LevelName = stateDict["level_name"].AsString();
                    if (stateDict.ContainsKey("objects") && stateDict["objects"].VariantType == Variant.Type.Array)
                    {
                        var objectsArray = stateDict["objects"].AsGodotArray();
                        foreach (var obj in objectsArray)
                        {
                            if (obj.VariantType == Variant.Type.Dictionary)
                            {
                                var objDict = obj.AsGodotDictionary();
                                var savedObj = new SavedLevelObject();
                                if (objDict.ContainsKey("name")) savedObj.Name = objDict["name"].AsString();
                                if (objDict.ContainsKey("data") && objDict["data"].VariantType == Variant.Type.Array)
                                {
                                    var dataArray = objDict["data"].AsGodotArray();
                                    var dataList = new List<object>();
                                    foreach (var d in dataArray) dataList.Add(d.Obj);
                                    savedObj.Data = dataList.ToArray();
                                }
                                levelState.Objects.Add(savedObj);
                            }
                        }
                    }
                    data.LevelStates.Add(levelState);
                }
            }
        }
        
        return data;
    }
}
