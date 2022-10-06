using Godot;
using System;

public class inventory : Node
{
    // Signals
    [Signal] delegate void items_changed(int items);

    // Variables
    public int slots = 16;
    public Godot.Collections.Dictionary items;
    public Godot.Collections.Dictionary dataItems;

    public override void _Ready()
    {
        items = new Godot.Collections.Dictionary();
        dataItems = ReadFromJson("res://assets/data/json/items.json");
        SetItem((Godot.Collections.Dictionary)dataItems["golden_holder_sword"], 0);
    }

    public Godot.Collections.Dictionary ReadFromJson(string path)
    {
        File file = new File();
        if (file.FileExists(path))
        {
            file.Open(path, File.ModeFlags.Read);
            Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
            return data;
        }
        else
        {
            GD.PrintErr("INVENTORY: Invalid path");
            return new Godot.Collections.Dictionary();
        }
    }

    public void SetItem(Godot.Collections.Dictionary item, int i)
    {
        items[i] = item;
        EmitSignal("items_changed", i);
    }

    public void RemoveItem(int i)
    {
        items.Remove(i);
        EmitSignal("items_changed", i);
    }

    public void SetQuantity(int i, int amount)
    {
        Godot.Collections.Dictionary item = (Godot.Collections.Dictionary)items[i];
        int quantity = (int)item["quantity"];
        quantity += amount;
        item["quantity"] = quantity;
        items[i] = item;
        if (quantity <= 0)
        {
            RemoveItem(i);
        }
        else 
        {
            EmitSignal("items_changed", i);
        }

    }
}
