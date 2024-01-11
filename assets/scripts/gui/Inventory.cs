using System.Collections.Generic;
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
        Tooltip = GetNode<Tooltip>("../Tooltip");
        DragPreview = GetNode<DragPreview>("../DragPreview");
        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        var gui = GetNode<PlayerGui>("../../");
        gui.OnItemsChanged += OnInventoryChanged;
        gui.OnPause += OnPause;

        var slotScene = GD.Load<PackedScene>("res://assets/objects/Slot.tscn");
        for (int i = 0; i < PlayerVariables.InventorySlots; i++)
        {
            Slot itemSlot = (Slot)slotScene.Instantiate();
            itemSlot.slotType = Slot.InventorySlotType.INVENTORY;
            if (i == 0) itemSlot.slotType = Slot.InventorySlotType.WEAPON;

            itemSlot.MouseEntered += () => Tooltip.Display(itemSlot);
            itemSlot.MouseExited += () => Tooltip.Hide();
            itemSlot.GuiInput += (InputEvent @event) => GuiInputSlot(@event, itemSlot.GetIndex());

            AddChild(itemSlot);
        }
    }

    public void OnPause(bool paused)
    {
        // var draggedItem = (Godot.Collections.Dictionary)dragPreview.Get("draggedItem");
        // if (draggedItem != null) if (draggedItem.Count >= 0) playerVariables.setItem(draggedItem, _previousItemId);
        // dragPreview.draggedItem = null;
    }

    void OnInventoryChanged()
    {
        for (int i = 0; i < PlayerVariables.Inventory.Length; i++)
        {
            var ItemSlot = GetChild(i) as Slot;
            ItemSlot.Data = PlayerVariables.Inventory[i].Clone();

            var Item = ItemSlot.GetChild<ItemContainer>(0);
            if (PlayerVariables.CurrentInventoryItem == i)
            {
                Item.DisplayItem(PlayerVariables.Inventory[i], true);
                PlayerVariables.CurrentInventoryItem = -1;
            }
            else Item.DisplayItem(PlayerVariables.Inventory[i]);
        }
    }

    public void GuiInputSlot(InputEvent @event, int slot)
    {
        if (@event is InputEventMouseButton)
        {
            var @mouseEvent = @event as InputEventMouseButton;
            if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed) DragItem(slot);
            if (@mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed) TakeFromStack(slot);
        }
    }

    /// <summary> Returns item from the slot and dragged item. </summary>
    public (Item existed, Item dragged) GetBoth(int slot) => (PlayerVariables.Inventory[slot], DragPreview.DraggedItem);

    /// <summary> Takes item from the stack. </summary>
    public void TakeFromStack(int slot)
    {
        var (existed, dragged) = GetBoth(slot);
        // Checking if item is stackable and not empty and if dragged item is the same as the item in the slot.
        // Then we can take item from the stack.
        if (!existed.IsEmpty && !dragged.IsEmpty && existed.Stackable && existed.ID == dragged.ID && !dragged.StackIsFull)
        {
            dragged.Quantity++;
            existed.Quantity--;
            DragPreview.ItemContainer.PopAnimation();
        }
        else DragPreview.Prevent();
    }

    public void DragItem(int slot)
    {
        var inventoryItem = PlayerVariables.Inventory[slot];
        var draggedItem = DragPreview.DraggedItem;
        Tooltip.Hide();
        // GD.Print($"CURRENT PRESSED INDEX: {slot}. Dragged item is empty: {draggedItem.IsEmpty}, Inventory item is empty: {inventoryItem.IsEmpty} (Quantity: {inventoryItem.Quantity}, ID: {inventoryItem.ID})");
        if (draggedItem.IsEmpty && !inventoryItem.IsEmpty)
        {
            DragPreview.DraggedItem = PlayerVariables.RemoveItem(slot);
            PreviousItemId = slot;
        }
        else if (!draggedItem.IsEmpty && inventoryItem.IsEmpty)
        {
            if (slot == 0 && draggedItem.Type != ItemType.WEAPON)
            {
                DragPreview.Prevent();
                return;
            }
            DragPreview.DraggedItem = new Item();
            PlayerVariables.SetItem(draggedItem, slot);
        }
        else if (!draggedItem.IsEmpty && !inventoryItem.IsEmpty)
        {
            if (slot == 0 && draggedItem.Type != ItemType.WEAPON)
            {
                DragPreview.Prevent();
                return;
            }
            DragPreview.DraggedItem = PlayerVariables.SetItem(draggedItem, slot);
            PreviousItemId = slot;
        }
    }
}