using Galatime;
using Godot;

public partial class SlotContainer : GridContainer
{
    public Tooltip tooltip;
    public DragPreview dragPreview;

    public PlayerVariables PlayerVariables;

    private int _previousItemId;

    [Signal] public delegate void guiChangedEventHandler();

    public override void _Ready()
    {
        PlayerGui gui = GetNode<PlayerGui>("../../");

        tooltip = GetNode<Tooltip>("../Tooltip");
        dragPreview = GetNode<DragPreview>("../DragPreview");

        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        gui.OnItemsChanged += () => _on_inventory_items_changed();
        gui.OnPause += (bool visible) => _onPause();
        PackedScene slot = GD.Load<PackedScene>("res://assets/objects/Slot.tscn");
        for (int i = 0; i < PlayerVariables.InventorySlots; i++)
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
        for (int i = 0; i < PlayerVariables.Inventory.Length; i++)
        {
            var ItemSlot = GetChild(i) as Slot;
            ItemSlot.Data = !ItemSlot.Data.IsEmpty ? PlayerVariables.Inventory[i].Clone() : new Item();

            var Item = ItemSlot.GetChild<ItemContainer>(0);
            if (PlayerVariables.CurrentInventoryItem == i)
            {
                Item.DisplayItem(PlayerVariables.Inventory[i], true);
                PlayerVariables.CurrentInventoryItem = -1;
            }
            else
            {
                Item.DisplayItem(PlayerVariables.Inventory[i]);
            }
        }
    }

    public void _mouseEnterSlot(Slot item)
    {
        tooltip.Display(item);
    }

    public void _mouseExitSlot()
    {
        tooltip.Call("_hide");
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
        var inventoryItem = PlayerVariables.Inventory[slot];
        var draggedItem = dragPreview.DraggedItem;
        tooltip.Hide();
        GD.Print($"CURRENT PRESSED INDEX: {slot}. Dragged item is empty: {draggedItem.IsEmpty}, Inventory item is empty: {inventoryItem.IsEmpty} (Quantity: {inventoryItem.Quantity}, ID: {inventoryItem.ID})");
        if (draggedItem.IsEmpty && !inventoryItem.IsEmpty)
        {
            dragPreview.DraggedItem = PlayerVariables.RemoveItem(slot);
            _previousItemId = slot;
        }
        else if (!draggedItem.IsEmpty && inventoryItem.IsEmpty)
        {
            if (slot == 0 && draggedItem.Type != ItemType.WEAPON)
            {
                dragPreview.Prevent();
                return;
            }   
            dragPreview.DraggedItem = new Item();
            PlayerVariables.SetItem(draggedItem, slot);
        }
        else if (!draggedItem.IsEmpty && !inventoryItem.IsEmpty)
        {
            if (slot == 0 && draggedItem.Type != ItemType.WEAPON)
            {
                dragPreview.Prevent();
                return;
            }
            dragPreview.DraggedItem = PlayerVariables.SetItem(draggedItem, slot);
            _previousItemId = slot;
        }
    }
}


