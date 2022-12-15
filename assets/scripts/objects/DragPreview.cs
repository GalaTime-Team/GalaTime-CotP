using Godot;
using System;

namespace Galatime {
    public class DragPreview : Control
    {
        TextureRect ItemIcon;
        Label ItemQuantity;

        AnimationPlayer animationPlayerStatus;

        public Godot.Collections.Dictionary draggedItem = null;

        public override void _Ready()
        {
            ItemIcon = GetNode<TextureRect>("Icon");
            ItemQuantity = GetNode<Label>("Quantity");
            animationPlayerStatus = GetNode<AnimationPlayer>("AnimationPlayerStatus");

            GetNode<AnimationPlayer>("AnimationPlayer").Play("idle");
        }

        public override void _Process(float delta)
        {
            if (draggedItem != null && draggedItem.Count != 0)
            {
                Vector2 position = GetGlobalMousePosition();
                position.x -= 16;
                position.y += 2;
                RectPosition = position;
            }
            else
            {
                RectPosition = Vector2.Zero;
                //ItemIcon.Texture = null;
                //ItemQuantity.Text = "";
            }
        }

        public void setDraggedItem(Godot.Collections.Dictionary i)
        {
            draggedItem = i;
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
                }
                else
                {
                    //ItemIcon.Texture = null;
                    //ItemQuantity.Text = "";
                }
            }
            else
            {
                //ItemIcon.Texture = null;
                //ItemQuantity.Text = "";
            }
        }

        public void prevent()
        {
            animationPlayerStatus.Play("error");
        }
    }
}


