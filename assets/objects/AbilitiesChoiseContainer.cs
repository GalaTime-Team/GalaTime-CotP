using Godot;
using System;

namespace Galatime
{
    public partial class AbilitiesChoiseContainer : VBoxContainer
    {
        private PlayerVariables _playerVariables;
        private HBoxContainer _abilitiesContainer;
        private Godot.Collections.Array<Node> abilityContainers;

        public string choiseId = "unknow";

        public override void _Ready()
        {
            _abilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            abilityContainers = _abilitiesContainer.GetChildren();

            for (int i = 0; i < abilityContainers.Count; i++)
            {
                var abilityContainer = abilityContainers[i] as AbilityContainer;
                var id = i;
                abilityContainer.GuiInput += (InputEvent @event) => _abilityInput(@event, id);
                //Connect("gui_input",new Callable(this,"_abilityInput"),binds);
            }

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.Connect("abilities_changed",new Callable(this,"_onAbilitiesChanged"));
        }

        private void _onAbilitiesChanged()
        {
            for (int i = 0; i < PlayerVariables.abilities.Count; i++)
            {
                var ability = (Godot.Collections.Dictionary)PlayerVariables.abilities[i];
                if (ability.ContainsKey("icon"))
                {
                    var icon = GD.Load((string)ability["icon"]) as Texture2D;
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
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    var abilityContainer = abilityContainers[id] as AbilityContainer;
                    for (int i = 0; i < PlayerVariables.abilities.Count; i++)
                    {
                        var ability = (Godot.Collections.Dictionary)PlayerVariables.abilities[i];
                        GD.Print(" reloaded? " + _playerVariables.isAbilityReloaded(i) + " " + _playerVariables.isAbilityReloaded(id));
                        if (ability.ContainsKey("id"))
                        {
                            if ((string)ability["id"] == choiseId)
                            {
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
                    if (_playerVariables.isAbilityReloaded(id))
                    {
                        _playerVariables.setAbility(GalatimeGlobals.getAbilityById(choiseId), id);
                        abilityContainer.click();
                    }
                    else
                    {
                        abilityContainer.no();
                        return;
                    }
                }
            }
        }
    }
}