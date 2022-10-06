using Godot;
using System;

public class slot_container : GridContainer
{
    // Exports
    [Export] public PackedScene ItemSlot;

    public override void _Ready()
    {
        inventory inv = GetNode<inventory>("/root/Inventory");
        inv.Connect("items_changed", this, "_on_inventory_items_changed");
    }

    void _on_inventory_items_changed(int items)
    {
        inventory inv = GetNode<inventory>("/root/Inventory");
        GD.Print("working!!!");
        var item_slot = GetChild(items).GetChild(0);
        GD.Print(item_slot);
        item_slot.Call("DisplayItem", inv.items[items]);
    }
}


