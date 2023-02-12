using Godot;
using System;

namespace Galatime {
    public class Tooltip : PanelContainer
    {
        public Label nameNode;
        public RichTextLabel descriptionNode;

        public override void _Ready()
        {
            nameNode = GetNode<Label>("MarginContainer/VBoxContainer/Name");
            descriptionNode = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/Description");
        }

        public void _display(Slot nodeItem)
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

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion)
            {
                Vector2 finalPosition = GetGlobalMousePosition();
                finalPosition.y += 16;
                _setPosition(finalPosition);
            }
        }

        public void _display(Godot.Collections.Dictionary abilityId)
        {
            var ability = abilityId;
            if (ability != null && ability.Count != 0)
            {
                nameNode.Text = ability.Contains("name") ? (string)ability["name"] : "Undefined item";
                descriptionNode.BbcodeText = ability.Contains("description") ? (string)ability["description"] : "What is that?";
                descriptionNode.BbcodeText += ability.Contains("element") ? "\nElement: " + (string)ability["element"] : "Inanus";
                if (ability.Contains("power")) descriptionNode.AppendBbcode("\n\nPower: [color=yellow]" + (Single)ability["power"] + "[/color]");
                if (ability.Contains("reload")) descriptionNode.AppendBbcode("\nReload: [color=yellow]" + (Single)ability["reload"] + "s[/color]");
                if (ability.Contains("costs"))
                {
                    var costs = (Godot.Collections.Dictionary)ability["costs"];
                    
                    if (costs.Contains("mana")) descriptionNode.AppendBbcode("\n\nMana cost: [color=#ff6347]" + (Single)costs["mana"] + "[/color]");
                    if (costs.Contains("stamina")) descriptionNode.AppendBbcode("\nStamina cost: [color=#ff6347]" + (Single)costs["stamina"] + "[/color]");
                }
                Visible = true;
            }
            else
            {
                Visible = false;
            }
            // nameNode.Text = ability.abilityName;
            Visible = true;
        }

        public void _hide()
        {
            Visible = false;
        }

        public override void _Process(float delta)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                GD.Print("hide");
                _hide();
            }
        }

        public void _setPosition(Vector2 position)
        {
            RectSize = new Vector2(RectSize.x, 0);
            SetGlobalPosition(position);
        }
    }
}