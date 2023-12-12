using System.Collections.Generic;
using Godot;

namespace Galatime
{
    public partial class AbilitiesChoiseContainer : VBoxContainer
    {
        private PlayerVariables _playerVariables;
        private HBoxContainer _abilitiesContainer;
        private List<AbilityContainer> abilityContainers = new();
        private PlayerVariables playerVariables;

        public string ChoiceId = "unknown";

        public override void _Ready()
        {
            _abilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            var abilityContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/AbilityContainer.tscn");
            // Adding ability containers
            for (int i = 0; i < PlayerVariables.AbilitySlots; i++)
            {
                // Instantiate ability container and add it to the abilities container
                var instance = abilityContainerScene.Instantiate<AbilityContainer>();
                _abilitiesContainer.AddChild(instance);
                var id = i;
                instance.GuiInput += (InputEvent @event) => _abilityInput(@event, id);
                abilityContainers.Add(instance);
            }

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.OnAbilitiesChanged += _onAbilitiesChanged;
        }

        public override void _ExitTree()
        {
            _playerVariables.OnAbilitiesChanged -= _onAbilitiesChanged;
        }

        private void _onAbilitiesChanged()
        {
            int i = 0;
            foreach (var ability in playerVariables.Abilities)
            {
                if (!ability.IsEmpty)
                {
                    var ab = abilityContainers[i];
                    ab.Load(ability);
                }
                else
                {
                    var ab = abilityContainers[i];
                    ab.Unload();
                }
                i++;
            }
        }

        private void _abilityInput(InputEvent @event, int id)
        {
            // Check if the event is a mouse button
            if (@event is InputEventMouseButton @mouseEvent)
            {
                // Check if the mouse button is pressed
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    // Get the ability container.
                    var abilityContainer = abilityContainers[id];
                    for (int i = 0; i < playerVariables.Abilities.Length; i++)
                    {
                        var ability = playerVariables.Abilities[i];
                        var choiceAbility = playerVariables.Abilities[id];
                        GD.Print(" reloaded? " + ability.IsEmpty + " " + choiceAbility.IsEmpty);
                        if (!ability.IsEmpty)
                        {
                            if (ability.ID == ChoiceId)
                            {
                                if (ability.IsFullyReloaded && choiceAbility.IsFullyReloaded)
                                {
                                    var previous = playerVariables.Abilities[id];
                                    _playerVariables.SetAbility(GalatimeGlobals.GetAbilityById(ChoiceId), id);
                                    _playerVariables.SetAbility(previous, i);
                                    abilityContainer.Click();
                                    return;
                                }
                                else
                                {
                                    abilityContainer.No();
                                    return;
                                }
                            }
                        }
                    }
                    if (playerVariables.Abilities[id].IsReloaded)
                    {
                        _playerVariables.SetAbility(GalatimeGlobals.GetAbilityById(ChoiceId), id);
                        abilityContainer.Click();
                    }
                    else
                    {
                        abilityContainer.No();
                        return;
                    }
                }
            }
        }
    }
}