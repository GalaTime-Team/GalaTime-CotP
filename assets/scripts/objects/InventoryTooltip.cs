using Godot;
using System;

namespace Galatime {
    public class InventoryTooltip : PanelContainer
    {
        public Label nameNode;
        public RichTextLabel descriptionNode;

        public override void _Ready()
        {
            nameNode = GetNode<Label>("MarginContainer/VBoxContainer/Name");
            descriptionNode = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/Description");
        }

        public void _display(TextureRect nodeItem)
        {
            var Item = nodeItem.GetChild(0);
            var data = (Godot.Collections.Dictionary)Item.Get("data");
            if (data != null && data.Count != 0)
            {
                nameNode.Text = data.Contains("name") ? (string)data["name"] : "Undefined item";
                descriptionNode.BbcodeText = data.Contains("description") ? (string)data["description"] : "What is that?";
                if (data.Contains("stats"))
                {
                    var stats = (Godot.Collections.Dictionary)data["stats"];
                    descriptionNode.AppendBbcode("\n\nDamage: [color=yellow]" + (Single)stats["damage"] + "[/color]");
                    descriptionNode.AppendBbcode("\nSwing speed: [color=yellow]" + (Single)stats["swing_speed"] / 1000 + "s[/color]");
                }
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }

        public void _hide()
        {
            Visible = false;
        }

        public void _setPosition(Vector2 position)
        {
            RectSize = new Vector2(RectSize.x, 0);
            SetGlobalPosition(position);
        }
    }
}