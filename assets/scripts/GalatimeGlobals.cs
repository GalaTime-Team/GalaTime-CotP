using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Galatime;
using Godot;
using YamlDotNet;

public sealed partial class GalatimeGlobals : Node
{
    public static List<Item> itemList = new();
    public static List<AbilityData> ablitiesList = new();
    public static Godot.Collections.Array tipsList = new Godot.Collections.Array();
    public static string pathListItems = "res://assets/data/json/items.json";
    public static string pathListAbilities = "res://assets/data/json/abilities.json";
    public static string pathListTips = "res://assets/data/json/tips.json";

    public static PackedScene loadingScene;
    public static PackedScene saveProcessScene;

    //itemList = _getItemsFromJson();
    //ablitiesList = _getAbilitiesFromJson();
    //1071756821158699068

    public PlayerVariables playerVariables;

    /// <summary>
    /// Slot type for inventory
    /// </summary>
    public enum slotType
    {
        slotDefault,
        slotSword
    }

    public static Dictionary<string, string> CMDArgs
    {
        get
        {
            var arguments = new Dictionary<string, string>();
            foreach (var argument in OS.GetCmdlineArgs())
            {

                if (argument.Find("=") > -1)
                {
                    string[] keyValue = argument.Split("=");
                    arguments[keyValue[0].TrimPrefix("--")] = keyValue[1];
                }
                else
                {
                    string[] keyValue = argument.Split("=");
                    // Options without an argument will be present in the dictionary,
                    // with the value set to an empty string.
                    arguments[keyValue[0].TrimPrefix("--")] = "";
                }
            }
            return arguments;
        }
    }

    public override void _Ready()
    {
        playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        loadingScene = ResourceLoader.Load<PackedScene>("res://assets/scenes/Loading.tscn");
        saveProcessScene = ResourceLoader.Load<PackedScene>("res://assets/scenes/SavingProcess.tscn");

        itemList = _getItemsFromJson();
        ablitiesList = _getAbilitiesFromJson();
        tipsList = _getTipsFromJson();
    }

    public void LoadScene(string nextScene = "res://assets/scenes/MainMenu.tscn")
    {
        CallDeferred(nameof(deferredLoadScene), nextScene);
    }

    private void deferredLoadScene(string path)
    {
        var loadingSceneInstance = loadingScene.Instantiate<Loading>();
        loadingSceneInstance.sceneName = path;
        GetTree().Root.AddChild(loadingSceneInstance);
    }

    /// <summary>
    /// Checks for the presence of saves and also creates them if they are absent
    /// </summary>
    public static void checkSaves()
    {
        if (!DirAccess.DirExistsAbsolute(GalatimeConstants.savesPath))
        {
            DirAccess.MakeDirAbsolute(GalatimeConstants.savesPath);
        }
        var savesCount = getSaves().Count;
        if (savesCount <= 5)
        {
            for (int i = 1; i <= 5 - savesCount; i++)
            {
                createBlankSave(i);
            }
        }
    }

    public static System.Collections.Generic.Dictionary<string, string> loadSettingsConfig()
    {
        string SETTINGS_FILE_PATH = "user://settings.yml";

        GD.Print($"Settings path: {SETTINGS_FILE_PATH}");

        var file = Godot.FileAccess.Open(SETTINGS_FILE_PATH, Godot.FileAccess.ModeFlags.Read);

        var deserializer = new YamlDotNet.Serialization.Deserializer();
        var data = deserializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(file.GetAsText());

        file.Close();

        return data;
    }

    public static void updateSettingsConfig(Node currentScene, double musicVolume = 1, double soundsVolume = 1, bool discordActivityDisabled = false)
    {
        var saveProcessSceneInstance = saveProcessScene.Instantiate<SavingProcess>();
        currentScene.GetTree().Root.AddChild(saveProcessSceneInstance);

        string SETTINGS_FILE_PATH = "user://settings.yml";

        GD.Print($"Settings path: {SETTINGS_FILE_PATH}");

        var file = Godot.FileAccess.Open(SETTINGS_FILE_PATH, Godot.FileAccess.ModeFlags.Write);
        if (Godot.FileAccess.GetOpenError() != Error.Ok)
        {
            saveProcessSceneInstance._playFailedAnimation();
            GD.Print("Error when saving a config: " + Godot.FileAccess.GetOpenError().ToString());
        }
        else
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            var saveData = getSettingsData(musicVolume, soundsVolume, discordActivityDisabled);
            var saveYaml = serializer.Serialize(saveData);

            file.StoreString(saveYaml);

            file.Close();
        }
    }

    private static System.Collections.Generic.Dictionary<string, object> getSettingsData(double musicVolume = 1, double soundsVolume = 1, bool discordActivityDisable = false)
    {
        var saveData = new System.Collections.Generic.Dictionary<string, object>();
        saveData.Add("music_volume", musicVolume);
        saveData.Add("sounds_volume", soundsVolume);
        // saveData.Add("discord_presence_disabled", discordActivityDisable);
        saveData.Add("config_version", 1);

        return saveData;
    }

    public static Godot.Collections.Array<Godot.Collections.Dictionary> getSaves()
    {
        var saves = DirAccess.Open(GalatimeConstants.savesPath);
        var results = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        if (saves != null)
        {
            saves.ListDirBegin();
            var fileName = saves.GetNext();
            while (fileName != "")
            {
                // GD.Print($"user://saves/{fileName}");
                var file = Godot.FileAccess.Open($"{GalatimeConstants.savesPath}{fileName}", Godot.FileAccess.ModeFlags.Read);
                var json = new Json();
                var parsedJson = json.Parse(file.GetAsText());
                if (parsedJson == Error.Ok)
                {
                    results.Add((Godot.Collections.Dictionary)json.Data);
                }
                else
                {
                    GD.Print(json.Data + " " + json.Data.GetType() + " " + parsedJson);
                }
                fileName = saves.GetNext();
            }
        }

        return results;
    }

    public static void createBlankSave(int saveId)
    {
        string SAVE_FILE_PATH = GalatimeConstants.savesPath;
        SAVE_FILE_PATH += "save" + saveId + ".json";

        GD.Print($"Save path: {SAVE_FILE_PATH}");

        var file = Godot.FileAccess.Open(SAVE_FILE_PATH, Godot.FileAccess.ModeFlags.Write);

        var saveData = getBlankSaveData(saveId);
        var saveJson = Json.Stringify(saveData, "\t");

        file.StoreString(saveJson);

        file.Close();
    }

    public static Godot.Collections.Dictionary loadSave(int saveId)
    {
        string SAVE_FILE_PATH = GalatimeConstants.savesPath;
        SAVE_FILE_PATH += "save" + saveId + ".json";

        var file = Godot.FileAccess.Open(SAVE_FILE_PATH, Godot.FileAccess.ModeFlags.Read);

        var saveData = (Godot.Collections.Dictionary)Json.ParseString(file.GetAsText());

        file.Close();

        return saveData;
    }

    public void save(int saveId, Node currentScene)
    {
        var saveProcessSceneInstance = saveProcessScene.Instantiate<SavingProcess>();
        if (currentScene != null)
        {
            currentScene.GetTree().Root.AddChild(saveProcessSceneInstance);
        }

        string SAVE_FILE_PATH = GalatimeConstants.savesPath;
        SAVE_FILE_PATH += "save" + saveId + ".json";

        var file = Godot.FileAccess.Open(SAVE_FILE_PATH, Godot.FileAccess.ModeFlags.Write);

        if (Godot.FileAccess.GetOpenError() != Error.Ok)
        {
            if (currentScene != null) saveProcessSceneInstance._playFailedAnimation();
            GD.Print("Error when saving a config: " + Godot.FileAccess.GetOpenError().ToString());
        }
        else
        {
            var saveData = getSaveData(saveId);
            var saveJson = Json.Stringify(saveData, "\t");

            file.StoreString(saveJson);

            file.Close();
        }
    }

    private Godot.Collections.Dictionary getSaveData(int saveId)
    {
        var saveData = new Godot.Collections.Dictionary
        {
            { "DO_NOT_MODIFY_THIS_FILE_ONLY_MODIFY_IF_YOU_KNOW_WHAT_YOURE_DOING", 0 },
            { "id", saveId },
            { "chapter", 1 },
            { "day", 1 },
            { "playtime", 0 },
            { "learned_abilities", playerVariables.learnedAbilities }
        };
        var inventory = new Godot.Collections.Dictionary();
        for (int i = 0; i < playerVariables.inventory.Count; i++)
        {
            var item = playerVariables.inventory[i];
            if (!item.IsEmpty)
            {
                inventory.Add(i, new Godot.Collections.Dictionary {
                    { "id", item.ID },
                    { "quantity", item.Quantity }
                });
            }
        }
        saveData.Add("inventory", inventory);
        var abilities = new Godot.Collections.Dictionary();
        for (int i = 0; i < playerVariables.abilities.Count; i++)
        {
            var ability = playerVariables.abilities[i];
            abilities.Add(i, new Godot.Collections.Dictionary());
            ((Godot.Collections.Dictionary)abilities[i]).Add("id", ability.ID);
        }
        saveData.Add("equiped_abilities", abilities);
        var stats = new Godot.Collections.Dictionary();
        for (int i = 0; i < playerVariables.Player.Stats.Count; i++)
        {
            var stat = playerVariables.Player.Stats[i];
            stats.Add(stat.ID, stat.Value);
        }
        saveData.Add("stats", stats);
        saveData.Add("xp", playerVariables.Player.Xp);
        return saveData;
    }

    private static Godot.Collections.Dictionary getBlankSaveData(int saveId)
    {
        var saveData = new Godot.Collections.Dictionary
        {
            { "DO_NOT_MODIFY_THIS_FILE_ONLY_MODIFY_IF_YOU_KNOW_WHAT_YOURE_DOING", 0 },
            { "id", saveId },
            { "chapter", 1 },
            { "day", 1 },
            { "playtime", 0 },
            { "learned_abilities", new Godot.Collections.Dictionary() },
            { "inventory", new Godot.Collections.Dictionary() },
            { "equiped_abilities", new Godot.Collections.Dictionary() }
        };

        return saveData;
    }

    private static Godot.Collections.Array _getTipsFromJson()
    {
        if (Godot.FileAccess.FileExists(pathListTips))
        {
            var file = Godot.FileAccess.Open(pathListTips, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            return (Godot.Collections.Array)((Godot.Collections.Dictionary)json.Data)["tips"];
        }
        else
        {
            GD.PrintErr("GLOBALS: Invalid path for tips");
            return new Godot.Collections.Array();
        }
    }

    /// <summary>
    /// Parses a dictionary of data into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of object to create.</typeparam>
    /// <param name="data">The dictionary of data to parse.</param>
    /// <returns>An object of type T with its properties set based on the data in the dictionary.</returns>
    public static T ParseJson<T>(Godot.Collections.Dictionary data)
    {
        if (typeof(T) == typeof(AbilityData))
        {
            var ab = new AbilityData()
            {
                // Set the all properties of the ability based on the parsed data
                ID = data.ContainsKey("id") ? (string)data["id"] : "",
                Name = data.ContainsKey("name") ? (string)data["name"] : "",
                Description = data.ContainsKey("description") ? (string)data["description"] : "",
                IconPath = data.ContainsKey("icon") ? (string)data["icon"] : "",
                ScenePath = data.ContainsKey("scene") ? (string)data["scene"] : "",
                Reload = data.ContainsKey("reload") ? (float)data["reload"] : 0,
                Duration = data.ContainsKey("duration") ? (float)data["duration"] : 0,
                Element = data.ContainsKey("element") ? GalatimeElement.GetByName((string)data["element"]) : new(),
                RequiredIDs = data.ContainsKey("required") ? data["required"].AsStringArray() : new string[0],
                CostXP = data.ContainsKey("cost") ? (int)data["cost"] : 0,
                // Set the costs of the ability
                Costs = data.ContainsKey("costs") ? new Costs
                {
                    Mana = ((Godot.Collections.Dictionary)data["costs"]).ContainsKey("mana") ? (int)((Godot.Collections.Dictionary)data["costs"])["mana"] : 0,
                    Stamina = ((Godot.Collections.Dictionary)data["costs"]).ContainsKey("stamina") ? (int)((Godot.Collections.Dictionary)data["costs"])["stamina"] : 0
                } : new(),
                Charges = data.ContainsKey("charges") ? (int)data["charges"] : 1,
                MaxCharges = data.ContainsKey("charges") ? (int)data["charges"] : 1
            };
            return (T)(object)ab;
        }
        else if (typeof(T) == typeof(Item))
        {
            var item = new Item()
            {
                // Set the all properties of the item based on the parsed data
                ID = data.ContainsKey("id") ? (string)data["id"] : "",
                Name = data.ContainsKey("name") ? (string)data["name"] : "",
                Description = data.ContainsKey("description") ? (string)data["description"] : "",
                IconPath = data.ContainsKey("icon") ? (string)data["icon"] : "",
                ScenePath = data.ContainsKey("scene") ? (string)data["scene"] : "",
                StackSize = data.ContainsKey("stack_size") ? (int)data["stack_size"] : 1,
                Stackable = data.ContainsKey("stackable") && (bool)data["stackable"],
                Type = data.ContainsKey("type") ? (string)data["type"] switch
                {
                    // If the "type" value is "weapon", set the Type property to WEAPON
                    "weapon" => SlotType.WEAPON,
                    // For any other value, set the Type property as common item.
                    _ => SlotType.COMMON
                } : SlotType.COMMON,
                Quantity = 1
            };
            return (T)(object)item;
        }
        else
        {
            GD.Print($"GLOBALS: Invalid type passed to ParseJson, not supported type: {typeof(T)}");
            return default;
        }
    }

    private static List<Item> _getItemsFromJson()
    {
        // Check if the file exists
        if (Godot.FileAccess.FileExists(pathListItems))
        {
            // Open the file in read mode
            var file = Godot.FileAccess.Open(pathListItems, Godot.FileAccess.ModeFlags.Read);
            // Create a new instance of the Json parser
            var json = new Json();
            // Parse the file content as text
            json.Parse(file.GetAsText());
            // Get the parsed data as a dictionary
            var data = (Godot.Collections.Dictionary)json.Data;

            // Get the data of the json file
            var itemListData = (Godot.Collections.Array)data["items"];
            var itemList = new List<Item>();

            // Iterate over the json data
            foreach (var i in itemListData)
            {
                // Get the parsed data as a dictionary
                var itemData = (Godot.Collections.Dictionary)i;
                // Create a new instance of the Item class
                var item = ParseJson<Item>(itemData);
                // Add the item to the list
                itemList.Add(item);
            }

            // Return the created list
            return itemList;
        }
        else
        {
            // If the file doesn't exist, print an error message and return a new instance of the Item class
            GD.PrintErr($"GLOBALS: Invalid path for inventory. Path: {pathListItems}");
            return new();
        }
    }


    private static List<AbilityData> _getAbilitiesFromJson()
    {
        if (FileAccess.FileExists(pathListItems))
        {
            var file = FileAccess.Open(pathListAbilities, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            // Get the parsed data as a dictionary
            var data = (Godot.Collections.Dictionary)json.Data;

            // Get the data of the json file
            var abilityListData = (Godot.Collections.Array)data["abilities"];
            var abilityList = new List<AbilityData>();

            // Iterate over the json data
            foreach (var i in abilityListData)
            {
                // Get the parsed data as a dictionary
                var abilityData = (Godot.Collections.Dictionary)i;
                // Create a new instance of the AbilityData class
                AbilityData ability = ParseJson<AbilityData>(abilityData);
                abilityList.Add(ability);
            }

            // Printing all abilities of the list to make sure it works
            foreach (var item in abilityList)
            {
                item.PrintAll();
            };

            return abilityList;
        }
        else
        {
            GD.PrintErr($"GLOBALS: Invalid path for abilities. Path: {pathListAbilities}");
            return new();
        }
    }

    public static Item getItemById(string id)
    {
        if (itemList.Count >= 0)
        {
            foreach (var item in itemList)
            {
                if (item.ID == id)
                {
                    return item.Clone();
                }
            }
            GD.PrintErr($"GLOBALS: Item ID is invalid. Item ID: {id}");
            return new();
        }
        else
        {
            GD.PrintErr("GLOBALS: Item list is empty");
            return new();
        }
    }

    public static AbilityData getAbilityById(string id)
    {
        if (ablitiesList.Count >= 0)
        {
            foreach (var item in ablitiesList)
            {
                if (item.ID == id)
                {
                    return item.Clone();
                }
            }
            GD.PrintErr("GLOBALS: Ability ID is invalid");
            return new();
        }
        else
        {
            GD.PrintErr("GLOBALS: Ability list is empty");
            return new();
        }
    }
}
