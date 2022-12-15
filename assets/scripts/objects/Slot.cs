using Godot;
using System;

namespace Galatime {
    public class Slot : TextureRect
    {
        Texture defaultTexture = GD.Load<Texture>("res://sprites/gui/inventory/slot.png");
        Texture weaponTexture = GD.Load<Texture>("res://sprites/gui/inventory/slot-weapon.png");
        //Texture selectedTexture = GD.Load<Texture>("res://images/item_slot_selected_background.png");

        public enum InventorySlotType
        {
            INVENTORY,
            WEAPON
        }

        public InventorySlotType slotType;

        public override void _Ready()
        {
            refreshStyle();
        }

        public void refreshStyle() {
            switch (slotType)
            {
                case InventorySlotType.INVENTORY:
                    this.Texture = defaultTexture;
                    break;
                case InventorySlotType.WEAPON:
                    this.Texture = weaponTexture;
                    break;
                default:
                    break;
            }
        }
    }
}
