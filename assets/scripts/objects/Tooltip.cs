using Godot;
using System;
using System.Text;

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

        public void Display(Slot nodeItem)
        {
            var Item = nodeItem.GetChild(0) as ItemContainer;
            var data = Item.Data;
            if (data is not null && !data.IsEmpty)
            {
                nameNode.Text = data.Name;
                descriptionNode.Text = data.Description;
                // var stats = (Godot.Collections.Dictionary)data["stats"];
                // descriptionNode.AppendText($"\n \nDamage: [color=yellow]{(Single)stats["damage"]}[/color]\nSwing speed: [color=yellow]{(Single)stats["swing_speed"] / 1000}s[/color]");
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

        public void Display(AbilityData abilityId)
        {
            var ability = abilityId;
            nameNode.Text = ability.Name;
            descriptionNode.Text = ability.Description;
            descriptionNode.Text += $"\nElement: {abilityId.Element.Name}";
            // if (ability.ContainsKey("power")) descriptionNode.AppendText("\n \nPower: [color=yellow]" + (Single)ability["power"] + "[/color]");
            var sb = new StringBuilder();
            sb.Append(ability.Reload > 0 ? $"\nCooldown: [color=yellow]{ability.Reload}s[/color]" : "");
            sb.Append(" \n \n");
            sb.Append(ability.Charges > 1 ? $"\n[color=green]+ Has {ability.Charges - 1} more charges[/color] ({ability.Charges})" : "");
            sb.Append(ability.Costs.Mana > 0 ? $"\n[color=orangered]- Consumes {ability.Costs.Mana} mana[/color]" : "");
            sb.Append(ability.Costs.Stamina > 0 ? $"\n[color=orangered]- Consumes {ability.Costs.Stamina} stamina[/color]" : "");
            descriptionNode.AppendText(sb.ToString());
            Visible = true;
            // nameNode.Text = ability.abilityName;
        }

        public void Hide()
        {
            Visible = false;
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                Hide();
            }
        }

        public void _setPosition(Vector2 position)
        {
            Size = new Vector2(Size.X, 0);
            SetGlobalPosition(position);
        }
    }
}