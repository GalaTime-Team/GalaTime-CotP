using Godot;
using System;

namespace Galatime
{
    public partial class Tooltip : PanelContainer
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
                nameNode.Text = data.ContainsKey("name") ? (string)data["name"] : "Undefined item";
                descriptionNode.Text = "";
                descriptionNode.Text = data.ContainsKey("description") ? (string)data["description"] : "What is that?";
                if (data.ContainsKey("stats"))
                {
                    var stats = (Godot.Collections.Dictionary)data["stats"];
                    descriptionNode.AppendText($"\n \nDamage: [color=yellow]{(Single)stats["damage"]}[/color]\nSwing speed: [color=yellow]{(Single)stats["swing_speed"] / 1000}s[/color]");
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
                finalPosition.Y += 16;
                _setPosition(finalPosition);
            }
        }

        public void _display(Godot.Collections.Dictionary abilityId)
        {
            var ability = abilityId;
            if (ability != null && ability.Count != 0)
            {
                nameNode.Text = ability.ContainsKey("name") ? (string)ability["name"] : "Undefined item";
                descriptionNode.Text = ability.ContainsKey("description") ? (string)ability["description"] : "What is that?";
                descriptionNode.Text += ability.ContainsKey("element") ? "\nElement: " + (string)ability["element"] : "Inanus";
                if (ability.ContainsKey("power")) descriptionNode.AppendText("\n \nPower: [color=yellow]" + (Single)ability["power"] + "[/color]");
                if (ability.ContainsKey("reload")) descriptionNode.AppendText("\nReload: [color=yellow]" + (Single)ability["reload"] + "s[/color]");
                if (ability.ContainsKey("costs"))
                {
                    var costs = (Godot.Collections.Dictionary)ability["costs"];

                    if (costs.ContainsKey("mana")) descriptionNode.AppendText("\n \nMana cost: [color=#ff6347]" + (Single)costs["mana"] + "[/color]");
                    if (costs.ContainsKey("stamina")) descriptionNode.AppendText("\nStamina cost: [color=#ff6347]" + (Single)costs["stamina"] + "[/color]");
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

        public void _display(EntityStat stat)
        {
            nameNode.Text = stat.name;
            descriptionNode.ParseBbcode($"{stat.description}" +
                $"\n \n" +
                $"After the upgrade you will get [color=yellow]{stat.value + 5}[/color]" +
                $"\n" +
                $"Required [rainbow]XP[/rainbow] for upgrade the stat: [color=yellow]100[/color]");
            
            Visible = true;
        }

        public void _hide()
        {
            Visible = false;
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                GD.Print("hide");
                _hide();
            }
        }

        public void _setPosition(Vector2 position)
        {
            Size = new Vector2(Size.X, 0);
            SetGlobalPosition(position);
        }
    }
}