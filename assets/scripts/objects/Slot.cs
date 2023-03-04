using Godot;
using System;

namespace Galatime {
    public partial class Slot : TextureRect
    {
        Texture2D defaultTexture = GD.Load<Texture2D>("res://sprites/gui/inventory/slot.png");
        Texture2D weaponTexture = GD.Load<Texture2D>("res://sprites/gui/inventory/slot-weapon.png");
        //Texture2D selectedTexture = GD.Load<Texture2D>("res://images/item_slot_selected_background.png");

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
