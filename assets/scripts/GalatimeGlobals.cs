using Godot;
using System;
using Discord;
using Galatime;
using System.IO;
using YamlDotNet;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed partial class GalatimeGlobals : Node
{
    public static Godot.Collections.Dictionary itemList = new Godot.Collections.Dictionary();
    public static Godot.Collections.Dictionary ablitiesList = new Godot.Collections.Dictionary();
    public static Godot.Collections.Array tipsList = new Godot.Collections.Array();
    public static string pathListItems = "res://assets/data/json/items.json";
    public static string pathListAbilities = "res://assets/data/json/abilities.json";
    public static string pathListTips = "res://assets/data/json/tips.json";

    public static PackedScene loadingScene;
    public static PackedScene saveProcessScene;

    //itemList = _getItemsFromJson();
    //ablitiesList = _getAbilitiesFromJson();
    //1071756821158699068

    public static Discord.Discord discord;
    public static Discord.ActivityManager activityManager;
    public static Activity currentActivity;

    public PlayerVariables playerVariables;

    /// <summary>
    /// Slot type for inventory
    /// </summary>
    public enum slotType
    {
        slotDefault,
        slotSword
    }

    //private static bool discordActivityDisabled;
    //public static bool DiscordActivityDisabled
    //{
    //    get
    //    {
    //        return discordActivityDisabled;
    //    }
    //    set
    //    {
    //        discordActivityDisabled = value;
    //        if (discordActivityDisabled)
    //        {
    //            activityManager.ClearActivity((result) =>
    //            {
    //                if (result == Discord.Result.Ok)
    //                {

    //                    Console.WriteLine("Success!");
    //                }
    //                else
    //                {
    //                    Console.WriteLine("Failed");
    //                }
    //            });
    //        }
    //        else
    //        {
    //            activityManager.UpdateActivity(currentActivity, (res) =>
    //            {
    //                if (res == Discord.Result.Ok)
    //                {
    //                    GD.Print("Everything is fine!");
    //                }
    //            });
    //        }
    //    }
    //}

    // public Node CurrentScene { get; set; }

    public override async void _Ready()
    {
        playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        loadingScene = ResourceLoader.Load<PackedScene>("res://assets/scenes/Loading.tscn");
        saveProcessScene = ResourceLoader.Load<PackedScene>("res://assets/scenes/SavingProcess.tscn");

        itemList = _getItemsFromJson();
        ablitiesList = _getAbilitiesFromJson();
        tipsList = _getTipsFromJson();
        
        // Viewport root = GetTree().Root;
        // GetTree().CurrentScene = root.GetChild(root.GetChildCount() - 1);

        //discord = new Discord.Discord(1071756821158699068, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        //activityManager = discord.GetActivityManager();

        //if (!DiscordActivityDisabled)
        //{
        //    currentActivity = new Activity()
        //    {
        //        Assets =
        //        {
        //            LargeImage = "default",
        //            LargeText = "GalaTime " + GalatimeConstants.version,
        //            SmallImage = "",
        //            SmallText = ""
        //        },
        //        Timestamps =
        //        {
        //            Start = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
        //        },
        //        State = "In main menu",
        //        Details = ""
        //    };

        //    activityManager.UpdateActivity(currentActivity, (res) =>
        //    {
        //        if (res == Discord.Result.Ok)
        //        {
        //            GD.Print("Everything is fine!");
        //        }
        //    });
        //}
    }

    public override void _Process(double delta)
    {
        // if (discord != null) discord.RunCallbacks();
    }

    public void loadScene(string nextScene = "res://assets/scenes/MainMenu.tscn")
    {
        // var loadingSceneInstance = loadingScene.Instantiate<Loading>();
        //loadingSceneInstance.sceneName = nextScene;
        // currentScene.GetTree().Root.AddChild(loadingSceneInstance);
        CallDeferred(nameof(deferredLoadScene), nextScene);
    }


    public void deferredLoadScene(string path)
    {
        var loadingSceneInstance = loadingScene.Instantiate<Loading>();
        loadingSceneInstance.sceneName = path;
        GetTree().Root.AddChild(loadingSceneInstance);

        // Old method

        // It is now safe to remove the current scene
        // CurrentScene.Free();

        // Load a new scene.
        // var nextScene = (PackedScene)GD.Load(path);

        // Instance the new scene.
        // CurrentScene = nextScene.Instantiate();

        // Add it to the active scene, as child of root.
        // GetTree().Root.AddChild(CurrentScene);

        // Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
        // GetTree().CurrentScene = CurrentScene;
    }

    public static ResourceLoader.ThreadLoadStatus sceneLoadingStatus(string nextScene = "res://assets/scenes/MainMenu.tscn")
    {
        return ResourceLoader.LoadThreadedGetStatus("res://assets/scenes/MainMenu.tscn");
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

    public void save(int saveId, Node? currentScene)
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
        var saveData = new Godot.Collections.Dictionary();
        saveData.Add("DO_NOT_MODIFY_THIS_FILE_ONLY_MODIFY_IF_YOU_KNOW_WHAT_YOURE_DOING", 0);
        saveData.Add("id", saveId);
        saveData.Add("chapter", 1);
        saveData.Add("day", 1);
        saveData.Add("playtime", 0);
        saveData.Add("learned_abilities", playerVariables.learnedAbilities);
        var inventory = new Godot.Collections.Dictionary();
        for (int i = 0; i < playerVariables.inventory.Count; i++)
        {
            var item = (Godot.Collections.Dictionary)playerVariables.inventory[i];
            if (!item.ContainsKey("id"))
            {
                inventory.Add(i, new Godot.Collections.Dictionary());
            }
            else
            {
                inventory.Add(i, new Godot.Collections.Dictionary {
                    { "id", item["id"] },
                    { "quantity", item.ContainsKey("quantity") ? item["quantity"] : 1 }
                });
            }
        }
        saveData.Add("inventory", inventory);
        var abilities = new Godot.Collections.Dictionary();
        for (int i = 0; i < playerVariables.abilities.Count; i++)
        {
            var ability = (Godot.Collections.Dictionary)playerVariables.abilities[i];
            abilities.Add(i, new Godot.Collections.Dictionary());
            if (ability.ContainsKey("id"))
            {
                ((Godot.Collections.Dictionary)abilities[i]).Add("id", ability["id"]);
            }
        }
        GD.Print("player state is " + playerVariables.Player == null + " playerVariables " + playerVariables == null);
        saveData.Add("equiped_abilities", abilities);
        var stats = new Godot.Collections.Dictionary();
        for (int i = 0; i < playerVariables.Player.Stats.Count; i++)
        {
            var stat = playerVariables.Player.Stats[i];
            stats.Add(stat.id, stat.Value);
        }
        saveData.Add("stats", stats);
        saveData.Add("xp", playerVariables.Player.Xp);
        return saveData;
    }

    private static Godot.Collections.Dictionary getBlankSaveData(int saveId)
    {
        var saveData = new Godot.Collections.Dictionary();
        saveData.Add("DO_NOT_MODIFY_THIS_FILE_ONLY_MODIFY_IF_YOU_KNOW_WHAT_YOURE_DOING", 0);
        saveData.Add("id", saveId);
        saveData.Add("chapter", 1);
        saveData.Add("day", 1);
        saveData.Add("playtime", 0);
        saveData.Add("learned_abilities", new Godot.Collections.Dictionary());
        saveData.Add("inventory", new Godot.Collections.Dictionary());
        saveData.Add("equiped_abilities", new Godot.Collections.Dictionary());

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

    private static Godot.Collections.Dictionary _getAbilitiesFromJson()
    {
        if (Godot.FileAccess.FileExists(pathListItems))
        {
            var file = Godot.FileAccess.Open(pathListAbilities, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            return (Godot.Collections.Dictionary)json.Data;
        }
        else
        {
            GD.PrintErr("GLOBALS: Invalid path for abilities");
            return new Godot.Collections.Dictionary();
        }
    }

    private static Godot.Collections.Dictionary _getItemsFromJson()
    {
        if (Godot.FileAccess.FileExists(pathListItems))
        {
            var file = Godot.FileAccess.Open(pathListItems, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            return (Godot.Collections.Dictionary)json.Data;
        }
        else
        {
            GD.PrintErr("GLOBALS: Invalid path for inventory");
            return new Godot.Collections.Dictionary();
        }
    }

    public static Godot.Collections.Dictionary getItemById(string id)
    {
        if (itemList.Count >= 0)
        {
            if (itemList.ContainsKey(id))
            {
                return (Godot.Collections.Dictionary)itemList[id];
            }
            else
            {
                GD.PrintErr("GLOBALS: Item ID is invalid");
                return new Godot.Collections.Dictionary();
            }
        }
        else
        {
            GD.PrintErr("GLOBALS: Item list is empty");
            return new Godot.Collections.Dictionary();
        }
    }

    public static Godot.Collections.Dictionary getAbilityById(string id)
    {
        if (ablitiesList.Count >= 0)
        {
            if (ablitiesList.ContainsKey(id))
            {
                return (Godot.Collections.Dictionary)ablitiesList[id];
            }
            else
            {
                GD.PrintErr("GLOBALS: Ability ID is invalid");
                return new Godot.Collections.Dictionary();
            }
        }
        else
        {
            GD.PrintErr("GLOBALS: Ability list is empty");
            return new Godot.Collections.Dictionary();
        }
    }
}
