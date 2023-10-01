using Galatime.Interfaces;
using Godot;

namespace Galatime
{
    public partial class Hand : Marker2D
    {
        public IWeapon Item = null;
        public Item ItemData;

        public void TakeItem(Item item)
        {
            RemoveItem();
            if (item.ItemScene != null)
            {
                this.ItemData = item;
                var objScene = item.ItemScene;
                var objNode = objScene.Instantiate<IWeapon>();
                AddChild(objNode as Node); 
                Item = objNode;
            }
        }

        private void RemoveItem()
        {
            if (GetChildCount() != 0)
            {
                Item = null;
                for (int i = 0; i < GetChildCount(); i++)
                {
                    var child = GetChild(i);
                    RemoveChild(child);
                }
            }
        }

        public Godot.Collections.Array<Node2D> GetOverlappingBodies()
        {
            if (Item is Area2D a)
            {
                var result = a.GetOverlappingBodies();
                return result;
            }
            else
            {
                return new();
            }
        }

        public void Attack(HumanoidCharacter p)
        {
            if (GetChildCount() != 0) Item.Attack(p);
        }
    }
}