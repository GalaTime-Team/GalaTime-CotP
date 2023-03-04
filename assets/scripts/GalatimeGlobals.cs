using Godot;
using System;
using Discord;
using Galatime;
using System.IO;

public sealed partial class GalatimeGlobals : Node
{
    public static Godot.Collections.Dictionary itemList = new Godot.Collections.Dictionary();
    public static Godot.Collections.Dictionary ablitiesList = new Godot.Collections.Dictionary();
    public static string pathListItems = "res://assets/data/json/items.json";
    public static string pathListAbilities = "res://assets/data/json/abilities.json";

    //itemList = _getItemsFromJson();
    //ablitiesList = _getAbilitiesFromJson();
    //1071756821158699068

    public Discord.Discord discord;

    /// <summary>
    /// Slot type for inventory
    /// </summary>
    public enum slotType
    {
        slotDefault,
        slotSword
    }

    public override async void _Ready()
    {
        itemList = _getItemsFromJson();
        ablitiesList = _getAbilitiesFromJson();

        discord = new Discord.Discord(1071756821158699068, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        var activityManager = discord.GetActivityManager();

        var activity = new Activity()
        {
            Assets =
            {
                LargeImage = "default",
                LargeText = "GalaTime " + GalatimeConstants.version,
                SmallImage = "day_1",
                SmallText = "1st day"
            },
            Timestamps =
            {
                Start = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
            },
            State = "In Play",
            Details = ""
        };

        activityManager.UpdateActivity(activity, (res) =>
        {
            if (res == Discord.Result.Ok)
            {
                GD.Print("Everything is fine!");
            }
        });
    }

    public override void _Process(double delta)
    {
        if (discord != null) discord.RunCallbacks();
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
