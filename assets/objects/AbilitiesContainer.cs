using Galatime;
using Godot;
using System;

public class AbilitiesContainer : ScrollContainer
{
    private Godot.Collections.Array _abilityItemsContainers;
    private Panel _abilitiesPanel;

    private PlayerVariables _playerVariables;

    public override void _Ready()
    {
        _abilitiesPanel = GetNode<Panel>("Panel");
        _abilityItemsContainers = GetTree().GetNodesInGroup("abilityItem");

        _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        _playerVariables.Connect("abilities_changed", this, "_onAbilitiesChanged");
    }

    public void _onAbilitiesChanged()
    {
        
    }
}

