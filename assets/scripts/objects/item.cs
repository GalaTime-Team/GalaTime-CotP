using Godot;
using System;

public class item : Control
{
    // Nodes
    TextureRect ItemIcon;
    Label ItemQuantity;

    public override void _Ready()
    {
        ItemIcon = GetNode<TextureRect>("icon");
        ItemQuantity = GetNode<Label>("quantity");
    }

    public void DisplayItem(Godot.Collections.Dictionary i)
    {
        GD.Print("working!!!@!!");
        Godot.Collections.Dictionary ItemAssets = (Godot.Collections.Dictionary)i["assets"];
        string icon = (string)ItemAssets["icon"];
        if (i != null)
        {
            ItemIcon.Texture = GD.Load<Texture>("res://assets/sprites/" + icon);
            ItemQuantity.Text = i["quantity"].ToString();
            if (!(bool)i["stackable"]) ItemQuantity.Text = "";
        }
        else
        {
            ItemIcon.Texture = null;
            ItemQuantity.Text = "";
        }
    }
}
