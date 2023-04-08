using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DictionaryExtension;

namespace Galatime
{
    public partial class Hand : Marker2D
    {
        public Node _item = null;

        public void takeItem(Godot.Collections.Dictionary item)
        {
            _removeItem();
            if (item.ContainsKey("assets"))
            {
                var objAssets = (Godot.Collections.Dictionary)item["assets"];
                if (objAssets.ContainsKey("object"))
                {
                    var objNodePath = (string)objAssets["object"];
                    var objScene = (PackedScene)ResourceLoader.Load(objNodePath);
                    var objNode = objScene.Instantiate();
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
                _item = null;
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

        public Godot.Collections.Array GetOverlappingBodies()
        {
            if (_item is Area2D a)
            {
                var result = (Godot.Collections.Array)_item.Call("get_overlapping_bodies");
                return result;
              }
            else
            {
                return new Godot.Collections.Array();
            }
        }

        public void attack(float physicalAttack, float magicalAttack)
        {
            if (GetChildCount() != 0) _item.Call("attack", physicalAttack, magicalAttack);
        }
    }
}