using Godot;
using System;

public partial class inventory : Node
{
    // Signals
    [Signal] public delegate void items_changedEventHandler(int items);

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
        if (Godot.FileAccess.FileExists(path))
        {
            var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            var json = new Json();
            json.Parse(file.GetAsText());
            return (Godot.Collections.Dictionary)json.Data;
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
        EmitSignal("items_changed", item);
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
