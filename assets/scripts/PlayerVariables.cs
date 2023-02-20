using Galatime;
using Godot;
using System;

public class PlayerVariables : Node
{
    public int slots = 16;
    public int abilitySlots = 3;
    public int currentItem = -1;

    public static Godot.Collections.Dictionary inventory = new Godot.Collections.Dictionary();
    public static Godot.Collections.Dictionary abilities = new Godot.Collections.Dictionary();

    [Signal] public delegate void items_changed();
    [Signal] public delegate void abilities_changed();

    public static Player player;

    public override void _Ready()
    {
        for (int i = 0; i < slots; i++)
        {
            inventory.Add(i, new Godot.Collections.Dictionary());
        }
        EmitSignal("items_changed", inventory);

        for (int i = 0; i < abilitySlots; i++)
        {
            abilities.Add(i, new Godot.Collections.Dictionary());
        }
        EmitSignal("abilities_changed", abilities);

        player = GetTree().GetNodesInGroup("player")[0] as Player;
    }

    public bool isAbilityReloaded(int id)
    {
        if (player == null)
        {
            GD.PrintErr("Сouldn't find a player, return false"); return false;
        }
    
        if (player._abiltiesReloadTimes[id] <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
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
    public void addItem(Godot.Collections.Dictionary item, int quantity)
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
                    setQuantity(i, quantity);
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
                    setQuantity(i, quantity);
                }
                return;
            }
        }
    }

    /// <summary>
    /// Sets ability to new slot
    /// </summary>
    /// <param name="ability">JSON ability data</param>
    /// <param name="slot">Up to three slots</param>
    public void setAbility(Godot.Collections.Dictionary ability, int slot)
    {
        if (abilities.Count >= 4)
        {
            GD.PushWarning("Can't set ability up to " + abilities.Count); return;
        }
        abilities[slot] = ability;
        EmitSignal("abilities_changed");
    }

    /// <summary>
    /// Removes ability item from slot
    /// </summary>
    /// <param name="slot">Item slot to delete</param>
    /// <returns>Previous ability</returns>
    public Godot.Collections.Dictionary removeAbility(int slot)
    {
        // Get pervious item to return
        Godot.Collections.Dictionary previousItem = new Godot.Collections.Dictionary();
        if (abilities.Contains(slot)) previousItem = (Godot.Collections.Dictionary)abilities[slot];
        // Remove item
        abilities[slot] = new Godot.Collections.Dictionary();
        // Send item_changed signal to GUI
        EmitSignal("abilities_changed");
        return previousItem;
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
        currentItem = slot;
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
