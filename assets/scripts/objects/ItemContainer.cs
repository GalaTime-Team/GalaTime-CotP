using Godot;
using Galatime;

namespace Galatime;
public partial class ItemContainer : Control
{
    #region Nodes
    public TextureRect IconTexture;
    public Label CountLabel;
    public AnimationPlayer AnimationPlayer;
    #endregion

    /// <summary>
    /// The current item of the item container.
    /// </summary>
    public Item Data;

    public override void _Ready()
    {
        #region Get node
        IconTexture = GetNode<TextureRect>("IconTexture");
        CountLabel = GetNode<Label>("CountLabel");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        #endregion

        DisplayItem(Data);
    }

    /// <summary>
    /// Setting the item to display.
    /// </summary>
    /// <param name="i">The item to display</param>
    /// <param name="_playAnimation">If the animation should be played. Current animation is popping</param>
    public void DisplayItem(Item i, bool _playAnimation = false)
    {
        // Check if the item is empty, if so, do nothing (don't display anything).
        if (i == null || i.IsEmpty) {
            IconTexture.Texture = null;
            CountLabel.Text = "";
            return;
        }

        // Set the item data to the local variable.
        Data = i;

        // Setting the item icon, name and count.
        IconTexture.Texture = i.Icon;
        CountLabel.Text = i.Quantity <= 1 ? "" : i.Quantity.ToString();
        if (!i.Stackable) CountLabel.Text = "";
        if (_playAnimation) PopAnimation();
    }

    public void PopAnimation() 
    { 
        AnimationPlayer.Stop();
        AnimationPlayer.Play("pop");
    }
}