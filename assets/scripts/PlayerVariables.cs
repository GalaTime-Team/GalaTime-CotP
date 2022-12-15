using Godot;
using System;

public class PlayerVariables : Node
{
    public int slots = 16;

    public static Godot.Collections.Dictionary inventory = new Godot.Collections.Dictionary();

    [Signal] public delegate void items_changed();

    public override void _Ready()
    {
        for (int i = 0; i < slots; i++)
        {
            inventory.Add(i, new Godot.Collections.Dictionary());
        }
        EmitSignal("items_changed", inventory);
    }


    public int getFreeSlot()
    {
        for (var i = 0; i < inventory.Count; i++)
        {
            if ((inventory[i] as Godot.Collections.Dictionary).Count == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public bool isStackable(Godot.Collections.Dictionary item)
    {
        return item.Contains("stackable") && (bool)item["stackable"];
    }

    public string getItemType(Godot.Collections.Dictionary item)
    {
        if (item.Contains("type"))
        {
            return (string)item["type"];
        }
        else
        {
            return "default";
        }
    }

    /// <summary> Add item to free slot in inventory </summary>
    public void addItem(Godot.Collections.Dictionary item)
    {
        // Go through all items
        for (var i = 0; i < inventory.Count; i++)
        {
            // Getting an existing Item
            var existedItem = (Godot.Collections.Dictionary)inventory[i];
            if (isStackable(item))
            {
                // Check if there is a similar stackable item
                if (existedItem.Count != 0 && (string)existedItem["name"] == (string)item["name"])
                {
                    // Add quantity if find a similar item
                    setQuantity(i, 1);
                    return;
                }
            }
        }

        // If there is no item for stackability, then add it to any free slot

        // Go through all items
        for (int i = 0; i < inventory.Count; i++)
        {
            // Getting an existing Item
            var existedItem = (Godot.Collections.Dictionary)inventory[i];
            // Checking of existence
            if (existedItem.Count == 0)
            {
                // Prevent an item from being added to a weapon slot
                if (getItemType(item) == "default" && i == 0) continue;
                setItem(item, i);
                if (isStackable(item))
                {
                    setQuantity(i, 1);
                }
                return;
            }
        }
    }

    /// <summary> Set inventory item to slot </summary>
    public Godot.Collections.Dictionary setItem(Godot.Collections.Dictionary item, int slot)
    {
        // Get pervious item to return
        Godot.Collections.Dictionary previousItem = new Godot.Collections.Dictionary();
        if (inventory.Contains(slot)) previousItem = (Godot.Collections.Dictionary)inventory[slot];
        // Remove item
        inventory.Remove(slot);
        // Set item
        inventory[slot] = item;
        // Send item_changed signal to GUI
        EmitSignal("items_changed");
        return previousItem;
    }

    /// <summary> Remove inventory item from slot </summary>
    public Godot.Collections.Dictionary removeItem(int slot)
    {
        // Get pervious item to return
        Godot.Collections.Dictionary previousItem = new Godot.Collections.Dictionary();
        if (inventory.Contains(slot)) previousItem = (Godot.Collections.Dictionary)inventory[slot];
        // Remove item
        inventory[slot] = new Godot.Collections.Dictionary();
        // Send item_changed signal to GUI
        EmitSignal("items_changed");
        return previousItem;
    }

    public void setQuantity(int slot, int amount)
    {
        Godot.Collections.Dictionary item = (Godot.Collections.Dictionary)inventory[slot];
        if (item.Contains("quantity")) item["quantity"] = amount + (int)item["quantity"];
        else item.Add("quantity", amount);

        GD.Print("ITEM QUANTITY: " + item["quantity"]);
        inventory[slot] = item;
        EmitSignal("items_changed");
    }
}
