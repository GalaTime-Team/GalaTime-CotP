using System.Collections.Generic;
using System.Linq;
using Galatime;
using Godot;
using Newtonsoft.Json;
using Galatime.Dialogue;
using Newtonsoft.Json.Linq;
using System;

public sealed partial class GalatimeGlobals : Node
{
    /// <summary> List of every single item data that is registered in the game. </summary>
    public static List<Item> ItemList = new();
    /// <summary> List of every single ability data that is registered in the game. </summary>
    public static List<AbilityData> AbilitiesList = new();
    /// <summary> List of every single dialog data that is registered in the game. </summary>
    public static List<DialogData> DialogsList = new();
    /// <summary> List of every single dialog character data that is registered in the game. </summary>
    public static List<Character> CharactersList = new();
    public static Godot.Collections.Array TipsList = new();

    public static string PathListItems = "res://assets/data/json/items.json";
    public static string PathListAbilities = "res://assets/data/json/abilities.json";
    public static string PathListTips = "res://assets/data/json/tips.json";
    public static string PathListDialogs = "res://assets/data/json/dialogs.json";
    public static string PathListCharacters = "res://assets/data/json/talking_characters.json";
    public static string PathListAllies = "res://assets/data/json/allies.json";

    /// <summary> The maximum number of saves that can be stored. </summary>
    public const int MaxSaves = 5;

    public static PackedScene LoadingScene;
    public static PackedScene SaveProcessScene;

    public PlayerVariables PlayerVariables;

    /// <summary> Returns the command line arguments passed to the game. </summary>
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
        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        LoadingScene = ResourceLoader.Load<PackedScene>("res://assets/scenes/Loading.tscn");
        SaveProcessScene = ResourceLoader.Load<PackedScene>("res://assets/scenes/SavingProcess.tscn");

        ItemList = GetFromJson<Item>(PathListItems, "items");
        AbilitiesList = GetFromJson<AbilityData>(PathListAbilities, "abilities");
        TipsList = GetTipsFromJson();
        DialogsList = GetDataFromJson<DialogsData>(PathListDialogs).Dialogs;
        CharactersList = GetDataFromJson<CharactersData>(PathListCharacters).Characters;

        // TODO: Move this to another file that handling the behavior of items.
        GetItemById("heal_potion", false).OnUse += () =>
        {
            PlayerVariables.Player.PlayDrinkingSound();
            GetTree().CreateTimer(1f).Timeout += () => PlayerVariables.Player.Heal(PlayerVariables.Player.Stats[EntityStatType.Health].Value * 0.5f);
        };
        GetItemById("mana_potion", false).OnUse += () =>
        {
            PlayerVariables.Player.PlayDrinkingSound();
            GetTree().CreateTimer(1f).Timeout += () => PlayerVariables.Player.Mana += PlayerVariables.Player.Stats[EntityStatType.Mana].Value * 0.5f;
        };
        // TODO: Move this to another file that handling the behavior of items.
    }

    public void LoadScene(string nextScene = "res://assets/scenes/MainMenu.tscn") => CallDeferred(nameof(DeferredLoadScene), nextScene);
    private void DeferredLoadScene(string path)
    {
        var loadingSceneInstance = LoadingScene.Instantiate<Loading>();
        loadingSceneInstance.sceneName = path;
        GetTree().Root.AddChild(loadingSceneInstance);
    }

    /// <summary>  Checks for the presence of saves and also creates them if they are absent </summary>
    public static void CheckSaves()
    {
        if (!DirAccess.DirExistsAbsolute(GalatimeConstants.SavesPath)) DirAccess.MakeDirAbsolute(GalatimeConstants.SavesPath);
    }

    public static Godot.Collections.Array<Godot.Collections.Dictionary> GetSaves()
    {
        var saves = DirAccess.Open(GalatimeConstants.SavesPath);
        var results = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        if (saves != null)
        {
            saves.ListDirBegin();
            var fileName = saves.GetNext();
            while (fileName != "")
            {
                var file = FileAccess.Open($"{GalatimeConstants.SavesPath}{fileName}", Godot.FileAccess.ModeFlags.Read);
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

    public static Godot.Collections.Dictionary LoadSave(int saveId)
    {
        string SAVE_FILE_PATH = $"{GalatimeConstants.SavesPath}save{saveId}.json";
        var file = FileAccess.Open(SAVE_FILE_PATH, FileAccess.ModeFlags.Read);
        var saveData = (Godot.Collections.Dictionary)Json.ParseString(file.GetAsText());
        file.Close();
        return saveData;
    }

    public void Save(int saveId, Node currentScene)
    {
        var saveProcessSceneInstance = SaveProcessScene.Instantiate<SavingProcess>();
        currentScene?.GetTree().Root.AddChild(saveProcessSceneInstance);

        var savePath = $"{GalatimeConstants.SavesPath}save{saveId}.json";
        var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

        if (FileAccess.GetOpenError() != Error.Ok)
        {
            if (currentScene != null) saveProcessSceneInstance.PlayFailedAnimation();
            GD.Print("Error when saving a config: " + FileAccess.GetOpenError().ToString());
        }
        else
        {
            var saveData = GetSaveData(saveId);
            var saveJson = Json.Stringify(saveData, "\t");
            file.StoreString(saveJson);
            file.Close();
        }
    }

    private Godot.Collections.Dictionary GetSaveData(int saveId)
    {
        var saveData = new Godot.Collections.Dictionary
        {
            { "DO_NOT_MODIFY_THIS_FILE_ONLY_MODIFY_IF_YOU_KNOW_WHAT_YOURE_DOING", 0 },
            { "id", saveId },
            { "chapter", 1 },
            { "day", 1 },
            { "playtime", 0 },
            { "learned_abilities", PlayerVariables.LearnedAbilities }
        };
        var inventory = new Godot.Collections.Dictionary();
        for (int i = 0; i < PlayerVariables.Inventory.Length; i++)
        {
            var item = PlayerVariables.Inventory[i];
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
        for (int i = 0; i < PlayerVariables.Abilities.Length; i++)
        {
            var ability = PlayerVariables.Abilities[i];
            abilities.Add(i, new Godot.Collections.Dictionary());
            ((Godot.Collections.Dictionary)abilities[i]).Add("id", ability.ID);
        }
        saveData.Add("equipped_abilities", abilities);
        if (PlayerVariables.Player is not null) saveData.Add("xp", PlayerVariables.Player.Xp);
        return saveData;
    }

    private static Godot.Collections.Array GetTipsFromJson()
    {
        if (FileAccess.FileExists(PathListTips))
        {
            var file = FileAccess.Open(PathListTips, Godot.FileAccess.ModeFlags.Read);
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

    /// <summary> Loads and parses a json file into an object of type T. </summary>
    /// <typeparam name="T"> The type of object to parse. </typeparam>
    /// <param name="path"> The path to the json file. </param>
    /// <returns> An object of type T with its properties set based on the data in the json file. </returns>
    public static T GetDataFromJson<T>(string path)
    {
        if (FileAccess.FileExists(path))
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var text = file.GetAsText();
            return JsonConvert.DeserializeObject<T>(text);
        }

        return default;
    }

    /// <summary> Loads and parses a json file into an object of type T. </summary>
    public static List<T> GetFromJson<T>(string path, string name)
    {
        // Check if the file exists
        if (FileAccess.FileExists(path))
        {
            // Open the file in read mode
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

            JObject json = JObject.Parse(file.GetAsText());
            var itemListData = json[name];
            var itemList = new List<T>();
            foreach (var i in itemListData)
            {
                // Create a new instance of the Item class
                var item = i.ToObject<T>();
                // Add the item to the list
                itemList.Add(item);
            }

            // Return the created list
            return itemList;
        }
        else
        {
            // If the file doesn't exist, print an error message and return a new instance of the Item class
            GD.PrintErr($"GLOBALS: Invalid path for {name}. Path: {path}");
            return new();
        }
    }

    public static Item GetItemById(string id, bool newItem = true)
    {
        if (ItemList.Count >= 0)
        {
            foreach (var item in ItemList)
            {
                if (item.ID == id)
                {
                    Item i;
                    if (newItem) i = item.Clone(); else i = item;
                    return i;
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

    public static DialogData GetDialogById(string id) => DialogsList.Find(x => x.ID == id);
    public static Character GetCharacterById(string id) => CharactersList.Find(x => x.ID == id);

    public static AbilityData GetAbilityById(string id)
    {
        if (AbilitiesList.Count >= 0)
        {
            var ability = AbilitiesList.FirstOrDefault(x => x.ID == id);
            if (ability is null) GD.PrintErr($"GLOBALS: Ability ID is invalid. Ability ID is {(ability is null ? "null" : ability.Name)}");
            return ability is null ? new() : ability.Clone();
        }
        else
        {
            GD.PrintErr("GLOBALS: Ability list is empty");
            return new();
        }
    }
}
