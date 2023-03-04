using Galatime;
using Godot;
using System;

public partial class AbilitiesContainer : ScrollContainer
{
    private Godot.Collections.Array<Node> _abilityItemsContainers;
    private Panel _abilitiesPanel;

    private PlayerVariables _playerVariables;

    public override void _Ready()
    {
        _abilitiesPanel = GetNode<Panel>("Panel");
        _abilityItemsContainers = GetTree().GetNodesInGroup("abilityItem");

        _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        _playerVariables.Connect("abilities_changed",new Callable(this,"_onAbilitiesChanged"));
    }

    public void _onAbilitiesChanged()
    {
        
    }
}

