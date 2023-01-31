using Godot;
using System;

public sealed class GalatimeGlobals : Node
{
    public static Godot.Collections.Dictionary itemList = new Godot.Collections.Dictionary();
    public static Godot.Collections.Dictionary ablitiesList = new Godot.Collections.Dictionary();
    public static string pathListItems = "res://assets/data/json/items.json";
    public static string pathListAbilities = "res://assets/data/json/abilities.json";

    /// <summary>
    /// Slot type for inventory
    /// </summary>
    public enum slotType
    {
        slotDefault,
        slotSword
    }


    public override void _Ready()
    {
        itemList = _getItemsFromJson();
        ablitiesList = _getAbilitiesFromJson();
    }

    private static Godot.Collections.Dictionary _getAbilitiesFromJson()
    {
        File file = new File();
        if (file.FileExists(pathListItems))
        {
            file.Open(pathListAbilities, File.ModeFlags.Read);
            Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
            return data;
        }
        else
        {
            GD.PrintErr("GLOBALS: Invalid path for abilities");
            return new Godot.Collections.Dictionary();
        }
    }

    private static Godot.Collections.Dictionary _getItemsFromJson()
    {
        File file = new File();
        if (file.FileExists(pathListItems))
        {
            file.Open(pathListItems, File.ModeFlags.Read);
            Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
            return data;
        }
        else
        {
            GD.PrintErr("GLOBALS: Invalid path for inventory");
            return new Godot.Collections.Dictionary();
        }
    }

    public static Godot.Collections.Dictionary getItemById(string id)
    {
        File file = new File();
        if (itemList.Count >= 0)
        {
            if (itemList.Contains(id))
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
        File file = new File();
        if (ablitiesList.Count >= 0)
        {
            if (ablitiesList.Contains(id))
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
