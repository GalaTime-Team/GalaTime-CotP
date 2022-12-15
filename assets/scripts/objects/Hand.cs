using Godot;
using System;


namespace Galatime
{
    public class Hand : Position2D
    {
        private Node _item;
        public void takeItem(Godot.Collections.Dictionary item)
        {
            _removeItem();
            if (item.Contains("assets"))
            {
                var objAssets = (Godot.Collections.Dictionary)item["assets"];
                if (objAssets.Contains("object"))
                {
                    var objNodePath = (string)objAssets["object"];
                    var objScene = (PackedScene)ResourceLoader.Load(objNodePath);
                    var objNode = objScene.Instance();
                    AddChild(objNode);
                    _item = objNode;
                }
                else
                {
                    GD.PushWarning("Unable to get the item from " + item);
                }
            }
            else
            {
                GD.PushWarning("Item doesn't have assets " + item);
            }
        }

        private void _removeItem()
        {
            if (GetChildCount() != 0)
            {
                for (int i = 0; i < GetChildCount(); i++)
                {
                    var child = GetChild(i);
                    RemoveChild(child);
                }
            }
            else
            {
                GD.Print("no");
            }
        }

        public void attack()
        {
            if (GetChildCount() != 0) _item.Call("attack");
        }
    }
}