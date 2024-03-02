using Godot;
using System;
using System.Text;

namespace Galatime;

/// <summary> Represents the tooltip for displaying information. </summary>
public partial class Tooltip : PanelContainer
{
    public Label NameNode;
    public RichTextLabel DescriptionNode;

    public override void _Ready()
    {
        #region Get nodes
        NameNode = GetNode<Label>("MarginContainer/VBoxContainer/Name");
        DescriptionNode = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/Description");
        #endregion

        Visible = false;
    }

    #region Display
    [Obsolete("Use Display(Item) instead")]
    public void Display(Slot nodeItem)
    {
        var Item = nodeItem.GetChild(0) as ItemContainer;
        var data = Item.Data;
        if (data is not null && !data.IsEmpty)
        {
            NameNode.Text = data.Name;
            DescriptionNode.Text = data.Description;
            Visible = true;
        }
        else
        {
            Visible = false;
        }
    }

    /// <summary> Display the tooltip with an item. </summary>
    public void Display(Item item)
    {
        if (item is not null && !item.IsEmpty)
        {
            NameNode.Text = item.Name;
            WriteDescription(item.Description);
        }
        else
        {
            Visible = false;
        }
    }

    /// <summary> Display the tooltip with an information about ability. </summary>
    public void Display(AbilityData ability)
    {
        NameNode.Text = ability.Name;

        // Add description
        var sb = new StringBuilder();
        sb.Append(ability.Description);
        sb.AppendLine($"Element: {ability.Element.Name}");
        sb.AppendLine(ability.Reload > 0 ? $"Cooldown: [color=yellow]{ability.Reload}s[/color]" : "");
        sb.Append(" \n");
        sb.AppendLine(ability.Charges > 1 ? $"[color=green]+ Has {ability.Charges - 1} more charges[/color] ({ability.Charges})" : "");
        sb.AppendLine(ability.Costs.Mana > 0 ? $"[color=orangered]- Consumes {ability.Costs.Mana} mana[/color]" : "");
        sb.AppendLine(ability.Costs.Stamina > 0 ? $"[color=orangered]- Consumes {ability.Costs.Stamina} stamina[/color]" : "");

        WriteDescription(sb.ToString());
    }

    public void Display(Cheat cheat)
    {
        NameNode.Text = cheat.Name;
        WriteDescription(cheat.Description);
    }
    #endregion

    public override void _Input(InputEvent @event)
    {
        // Tooltip always follows the mouse.
        if (@event is InputEventMouseMotion && Visible) // No need to move the tooltip if it's not visible.
        {
            Vector2 finalPosition = GetGlobalMousePosition();
            
            // Don't go outside the viewport.
            if (finalPosition.X + Size.X * Scale.X >= GetViewportRect().Size.X)
                finalPosition.X -= Size.X * Scale.X;

            finalPosition.Y += 16; // Looks nicer if the tooltip is above the mouse.
            GlobalPosition = finalPosition;
        }
    }

    public void WriteDescription(string description)
    {
        Visible = true;
        DescriptionNode.Clear();
        Size = new Vector2(Size.X, 0); // Make 0 size to force the size to be calculated.

        // AppendText, because to parse BBCode.
        // Don't ask me why space is added at the end, it just needs to be there.
        DescriptionNode.AppendText(description + " ");
    }

    public void HideTooltip()
    {
        // Don't ask me why, someday I could make an animation to hide the tooltip.
        Visible = false;
    }
}