using Galatime;
using Godot;

public partial class Inventory : GridContainer
{
    #region Nodes
    public Tooltip Tooltip;
    public DragPreview DragPreview;
    public PlayerVariables PlayerVariables;
    #endregion

    /// <summary> The previous item id in the inventory. </summary>
    private int PreviousItemId;

    public override void _Ready()
    {
        PlayerGui gui = GetNode<PlayerGui>("../../");

        Tooltip = GetNode<Tooltip>("../Tooltip");
        DragPreview = GetNode<DragPreview>("../DragPreview");

        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        gui.OnItemsChanged += () => _on_inventory_items_changed();
        gui.OnPause += (bool visible) => _onPause();
        PackedScene slot = GD.Load<PackedScene>("res://assets/objects/Slot.tscn");
        for (int i = 0; i < PlayerVariables.slots; i++)
        {
            Slot ItemSlot = (Slot)slot.Instantiate();
            ItemSlot.slotType = Slot.InventorySlotType.INVENTORY;
            if (i == 0) ItemSlot.slotType = Slot.InventorySlotType.WEAPON;

            ItemSlot.MouseEntered += () => _mouseEnterSlot(ItemSlot);
            ItemSlot.MouseExited += () => _mouseExitSlot();
            ItemSlot.GuiInput += (InputEvent @event) => _guiInputSlot(@event, ItemSlot.GetIndex());

            AddChild(ItemSlot);
        }
    }

    public void _onPause()
    {
        // var draggedItem = (Godot.Collections.Dictionary)dragPreview.Get("draggedItem");
        // if (draggedItem != null) if (draggedItem.Count >= 0) playerVariables.setItem(draggedItem, _previousItemId);
        // dragPreview.draggedItem = null;
    }

    void _on_inventory_items_changed()
    {
        for (int i = 0; i < PlayerVariables.inventory.Count; i++)
        {
            var ItemSlot = GetChild(i) as Slot;
            ItemSlot.Data = !ItemSlot.Data.IsEmpty ? PlayerVariables.inventory[i].Clone() : new Item();

            var Item = ItemSlot.GetChild<ItemContainer>(0);
            if (PlayerVariables.currentItem == i)
            {
                Item.DisplayItem(PlayerVariables.inventory[i], true);
                PlayerVariables.currentItem = -1;
            }
            else
            {
                Item.DisplayItem(PlayerVariables.inventory[i]);
            }
        }
    }

    public void _mouseEnterSlot(Slot item)
    {
        Tooltip.Display(item);
    }

    public void _mouseExitSlot()
    {
        Tooltip.Call("_hide");
    }

    public void _guiInputSlot(InputEvent @event, int slot)
    {
        if (@event is InputEventMouseButton)
        {
            var @mouseEvent = @event as InputEventMouseButton;
            if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                dragItem(slot);
                GD.Print("get input");
            }
        }
    }
    public void dragItem(int slot)
    {
        var inventoryItem = PlayerVariables.inventory[slot];
        var draggedItem = DragPreview.DraggedItem;
        Tooltip.Hide();
        GD.Print($"CURRENT PRESSED INDEX: {slot}. Dragged item is empty: {draggedItem.IsEmpty}, Inventory item is empty: {inventoryItem.IsEmpty} (Quantity: {inventoryItem.Quantity}, ID: {inventoryItem.ID})");
        if (draggedItem.IsEmpty && !inventoryItem.IsEmpty)
        {
            DragPreview.DraggedItem = PlayerVariables.removeItem(slot);
            PreviousItemId = slot;
        }
        else if (!draggedItem.IsEmpty && inventoryItem.IsEmpty)
        {
            if (slot == 0 && draggedItem.Type != SlotType.WEAPON)
            {
                DragPreview.Prevent();
                return;
            }   
            DragPreview.DraggedItem = new Item();
            PlayerVariables.setItem(draggedItem, slot);
        }
        else if (!draggedItem.IsEmpty && !inventoryItem.IsEmpty)
        {
            if (slot == 0 && draggedItem.Type != SlotType.WEAPON)
            {
                DragPreview.Prevent();
                return;
            }
            DragPreview.DraggedItem = PlayerVariables.setItem(draggedItem, slot);
            PreviousItemId = slot;
        }
    }
}


