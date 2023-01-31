using Godot;
using System;

public class item : Control
{
    // Nodes
    TextureRect ItemIcon;
    Label ItemQuantity;
    AnimationPlayer animationPlayer;

    public Godot.Collections.Dictionary data;

    public override void _Ready()
    {
        ItemIcon = GetNode<TextureRect>("icon");
        ItemQuantity = GetNode<Label>("quantity");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void DisplayItem(Godot.Collections.Dictionary i, bool _playAnimation = false)
    {
        data = i;
        if (i.Count != 0)
        {
            Godot.Collections.Dictionary ItemAssets = (Godot.Collections.Dictionary)i["assets"];
            string icon = (string)ItemAssets["icon"];
            if (i != null)
            {
                ItemIcon.Texture = GD.Load<Texture>("res://sprites/" + icon);
                if (i.Contains("quantity"))
                {
                    ItemQuantity.Text = ((int)i["quantity"]).ToString();
                }
                else
                {
                    ItemQuantity.Text = "";
                }
                if (!(bool)i["stackable"]) ItemQuantity.Text = "";
                if (_playAnimation) animationPlayer.Play("pop");
            }
            else
            {
                ItemIcon.Texture = null;
                ItemQuantity.Text = "";
            }
        }
        else
        {
            ItemIcon.Texture = null;
            ItemQuantity.Text = "";
        }
    }
}