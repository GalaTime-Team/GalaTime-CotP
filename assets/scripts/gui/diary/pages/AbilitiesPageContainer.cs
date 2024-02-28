using Galatime;
using Galatime.UI;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AbilitiesPageContainer : Control
{
    public VBoxContainer AbilitiesList;
    public List<AbilityDescriptionContainer> AbilityContainers = new();

    private PlayerVariables PlayerVariables;

    public override void _Ready()
    {
        #region Get nodes
        AbilitiesList = GetNode<VBoxContainer>("AbilitiesList");
        #endregion

        PlayerVariables = PlayerVariables.Instance;
        PlayerVariables.OnAbilitiesChanged += OnAbilitiesChanged;

        AbilityContainers = AbilitiesList.GetChildren().Cast<AbilityDescriptionContainer>().ToList();

        OnAbilitiesChanged();
    }

    public void OnAbilitiesChanged()
    {
        for (int i = 0; i < PlayerVariables.Abilities.Length; i++)
        {
            var abCon = AbilityContainers[i];
            var ab = PlayerVariables.Abilities[i];
            abCon.Load(ab, i + 1);
        }
    }
}
