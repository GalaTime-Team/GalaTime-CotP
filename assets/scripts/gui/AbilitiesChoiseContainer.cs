using Godot;

using Galatime.Global;

using System.Collections.Generic;

namespace Galatime
{
    public partial class AbilitiesChoiseContainer : VBoxContainer
    {
        private HBoxContainer AbilitiesContainer;
        private List<AbilityContainer> AbilityContainers = new();
        private PlayerVariables PlayerVariables;

        public string ChoiceId = "unknown";

        public override void _Ready()
        {
            AbilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            PlayerVariables = PlayerVariables.Instance;
            
            PlayerVariables.OnAbilitiesChanged += LoadAbilities;

            var abilityContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/AbilityContainer.tscn");
            // Adding ability containers
            for (int i = 0; i < PlayerVariables.AbilitySlots; i++)
            {
                // Instantiate ability container and add it to the abilities container
                var instance = abilityContainerScene.Instantiate<AbilityContainer>();
                AbilitiesContainer.AddChild(instance);
                var id = i; // DON'T REMOVE THIS
                instance.GuiInput += (InputEvent @event) => AbilityInput(@event, id);
                AbilityContainers.Add(instance);
            }
            LoadAbilities();
        }

        public override void _ExitTree()
        {
            PlayerVariables.OnAbilitiesChanged -= LoadAbilities;
        }

        public void LoadAbilities()
        {
            int i = 0;
            foreach (var ability in PlayerVariables.Abilities)
            {
                if (!ability.IsEmpty)
                {
                    var ab = AbilityContainers[i];
                    ab.Load(ability);
                }
                else
                {
                    var ab = AbilityContainers[i];
                    ab.Unload();
                }
                i++;
            }
        }

        private void AbilityInput(InputEvent @event, int id)
        {
            // Check if the event is a mouse button
            if (@event is InputEventMouseButton @mouseEvent)
            {
                // Check if the mouse button is pressed
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    // Get the ability container.
                    var abilityContainer = AbilityContainers[id];
                    for (int i = 0; i < PlayerVariables.Abilities.Length; i++)
                    {
                        var ability = PlayerVariables.Abilities[i];
                        var choiceAbility = PlayerVariables.Abilities[id];
                        if (!ability.IsEmpty && ability.ID == ChoiceId)
                        {
                            if (ability.IsFullyReloaded && choiceAbility.IsFullyReloaded)
                            {
                                var previous = PlayerVariables.Abilities[id];
                                PlayerVariables.SetAbility(GalatimeGlobals.GetAbilityById(ChoiceId), id);
                                PlayerVariables.SetAbility(previous, i);
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
                    if (PlayerVariables.Abilities[id].IsReloaded)
                    {
                        PlayerVariables.SetAbility(GalatimeGlobals.GetAbilityById(ChoiceId), id);
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