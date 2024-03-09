using Godot;
using System;

namespace Galatime.UI;

/// <summary> Represents a container for the ability description. </summary>
public partial class AbilityDescriptionContainer : HBoxContainer
{
    public TextureRect AbilityIcon;
    public Label AbilityName;
    public RichTextLabel AbilityDescription;

    public Texture2D DefaultIcon;

    private AbilityData abilityData;
    public AbilityData AbilityData
    {
        get => abilityData;
        set
        {
            abilityData = value;

            AbilityIcon.Texture = value.Icon;
            AbilityName.Text = value.Name;
            AbilityDescription.Text = value.Description;

            if (value.IsEmpty)
                AbilityIcon.Texture = DefaultIcon;
        }
    }

    public void Load(AbilityData ab, int index = -1)
    {
        AbilityData = ab;
        
        if (index > 0) AbilityName.Text = $"{index}. {AbilityName.Text}";
    }

    public override void _ExitTree()
    {
        AbilityIcon = null;
        AbilityName = null;
        AbilityDescription = null;
    }

    public override void _Ready()
    {
        #region Get nodes
        AbilityIcon = GetNode<TextureRect>("AbilityIcon");
        AbilityName = GetNode<Label>("DescriptionContainer/NameLabel");
        AbilityDescription = GetNode<RichTextLabel>("DescriptionContainer/DescriptionLabel");
        #endregion

        // By default, scene depends on the default ability icon and name.
        DefaultIcon = AbilityIcon.Texture;
    }
}
