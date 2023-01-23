using Godot;
using System;
using System.Reflection;

using Galatime;

public class slot_container : GridContainer
{
    public InventoryTooltip tooltip;
    public DragPreview dragPreview;

    public PlayerVariables playerVariables;

    [Signal] public delegate void guiChanged();

    public override void _Ready()
    {
        Control gui = GetNode<Control>("../../");

        tooltip = GetNode<InventoryTooltip>("../Tooltip");
        dragPreview = GetNode<DragPreview>("../DragPreview");

        playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        gui.Connect("items_changed", this, "_on_inventory_items_changed");
        PackedScene slot = GD.Load<PackedScene>("res://assets/objects/Slot.tscn");
        for (int i = 0; i < playerVariables.slots; i++)
        {
            Slot ItemSlot = (Slot)slot.Instance();
            ItemSlot.slotType = Slot.InventorySlotType.INVENTORY;
            if (i == 0) ItemSlot.slotType = Slot.InventorySlotType.WEAPON;

            Godot.Collections.Array binds = new Godot.Collections.Array();
            binds.Add(ItemSlot);
            ItemSlot.Connect("mouse_entered", this, "_mouseEnterSlot", binds);
            ItemSlot.Connect("mouse_exited", this, "_mouseExitSlot");
            ItemSlot.Connect("gui_input", this, "_guiInputSlot", binds);

            AddChild(ItemSlot);
        }
    }

    void _on_inventory_items_changed()
    {
        for (int i = 0; i < PlayerVariables.inventory.Count; i++)
        {
            var ItemSlot = GetChild(i);
            var Item = ItemSlot.GetChild<item>(0);
            Item.DisplayItem((Godot.Collections.Dictionary)PlayerVariables.inventory[i]); 
        }
    }

    public void _mouseEnterSlot(TextureRect item)
    {
        tooltip.Call("_display", item);
    }

    public void _mouseExitSlot()
    {
        tooltip.Call("_hide");
    }

    public void _guiInputSlot(InputEvent @event, TextureRect item)
    {
        if (@event is InputEventMouseMotion)
        {
            Vector2 finalPosition = GetGlobalMousePosition();
            finalPosition.y += 16;
            tooltip.Call("_setPosition", finalPosition);
        }
        if (@event is InputEventMouseButton)
        {
            var @mouseEvent = @event as InputEventMouseButton;
            if (@mouseEvent.ButtonIndex == 1 && mouseEvent.Pressed)
            {
                dragItem(item);
            }
        }
    }
    public void dragItem(TextureRect nodeItem)
    {
        var inventoryItem = (Godot.Collections.Dictionary)PlayerVariables.inventory[nodeItem.GetIndex()];
        var draggedItem = (Godot.Collections.Dictionary)dragPreview.Get("draggedItem");
        tooltip.Call("_hide");
        if (inventoryItem != null && draggedItem == null)
        {
            GD.Print("pick");
            dragPreview.Call("setDraggedItem", playerVariables.removeItem(nodeItem.GetIndex()));
        }
        else if (inventoryItem.Count <= 0 && draggedItem.Count >= 0)
        {
            GD.Print("drop");
            if (nodeItem.GetIndex() == 0 && playerVariables.getItemType(draggedItem) == "default")
            {
                dragPreview.prevent();
                return;
            }
            dragPreview.draggedItem = null;
        }
        else if (inventoryItem.Count >= 0 && draggedItem.Count >= 0)
        {
            GD.Print("swap");
            GD.Print(draggedItem.Count);
            if (nodeItem.GetIndex() == 0 && playerVariables.getItemType(inventoryItem) == "weapon")
            {
                dragPreview.prevent();
                return;
            }
            dragPreview.Call("setDraggedItem", playerVariables.setItem(draggedItem, nodeItem.GetIndex()));
        }
    }
}

    
