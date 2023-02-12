using Godot;
using System;

namespace Galatime
{
    public class AbilitiesChoiseContainer : VBoxContainer
    {
        private PlayerVariables _playerVariables;
        private HBoxContainer _abilitiesContainer;
        private Godot.Collections.Array abilityContainers;

        public string choiseId = "unknow";

        public override void _Ready()
        {
            _abilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            abilityContainers = _abilitiesContainer.GetChildren();

            for (int i = 0; i < abilityContainers.Count; i++)
            {
                var abilityContainer = abilityContainers[i] as AbilityContainer;
                var binds = new Godot.Collections.Array();
                binds.Add(i);
                abilityContainer.Connect("gui_input", this, "_abilityInput", binds);
            }

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.Connect("abilities_changed", this, "_onAbilitiesChanged");
        }

        private void _onAbilitiesChanged()
        {
            for (int i = 0; i < PlayerVariables.abilities.Count; i++)
            {
                var ability = (Godot.Collections.Dictionary)PlayerVariables.abilities[i];
                if (ability.Contains("icon"))
                {
                    var icon = GD.Load((string)ability["icon"]) as Texture;
                    var ab = abilityContainers[i] as AbilityContainer;
                    ab.load(icon, 2);
                }
                else
                {
                    var ab = abilityContainers[i] as AbilityContainer;
                    ab.unload();
                }
            }
        }

        private void _abilityInput(InputEvent @event, int id)
        {
            if (@event is InputEventMouseButton)
            {
                var @mouseEvent = @event as InputEventMouseButton;
                if (@mouseEvent.ButtonIndex == 1 && mouseEvent.Pressed)
                {
                    var abilityContainer = abilityContainers[id] as AbilityContainer;
                    for (int i = 0; i < PlayerVariables.abilities.Count; i++)
                    {
                        var ability = (Godot.Collections.Dictionary)PlayerVariables.abilities[i];
                        if (ability.Contains("id"))
                        {
                            if ((string)ability["id"] == choiseId)
                            {
                                GD.Print(i + " id " + _playerVariables.isAbilityReloaded(i));
                                if (_playerVariables.isAbilityReloaded(i) && _playerVariables.isAbilityReloaded(id))
                                {
                                    var previous = (Godot.Collections.Dictionary)PlayerVariables.abilities[id];
                                    _playerVariables.setAbility(GalatimeGlobals.getAbilityById(choiseId), id);
                                    _playerVariables.setAbility(previous, i);
                                    abilityContainer.click();
                                    return;
                                }
                                else
                                {
                                    abilityContainer.no();
                                    return;
                                }
                            }
                        }
                    }
                    _playerVariables.setAbility(GalatimeGlobals.getAbilityById(choiseId), id);
                    abilityContainer.click();
                }
            }
        }
    }
}