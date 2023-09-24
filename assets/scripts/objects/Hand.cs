using Godot;

namespace Galatime
{
    public partial class Hand : Marker2D
    {
        public Node Item = null;
        public Item ItemData;

        public void TakeItem(Item item)
        {
            RemoveItem();
            if (item.ItemScene != null)
            {
                this.ItemData = item;
                var objScene = item.ItemScene;
                var objNode = objScene.Instantiate();
                AddChild(objNode); 
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

        public Godot.Collections.Array GetOverlappingBodies()
        {
            if (Item is Area2D a)
            {
                var result = (Godot.Collections.Array)Item.Call("get_overlapping_bodies");
                return result;
            }
            else
            {
                return new Godot.Collections.Array();
            }
        }

        public void Attack(float physicalAttack, float magicalAttack)
        {
            if (GetChildCount() != 0) Item.Call("attack", physicalAttack, magicalAttack);
        }
    }
}