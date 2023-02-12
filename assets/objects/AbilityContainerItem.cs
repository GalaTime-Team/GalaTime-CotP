using Godot;
using System;

namespace Galatime
{
    public class AbilityContainerItem : TextureRect
    {
        private Tooltip _tooltip;
        private AbilitiesChoiseContainer abilityChoiseContainer;
        private AnimationPlayer _animationPlayer;

        [Export] public string abilityName = "unknown";

        private Godot.Collections.Dictionary abilityData;

        private PlayerVariables _playerVariables;

        public override void _Ready()
        {
            _tooltip = GetNode<Tooltip>("../../../Tooltip");
            abilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("../AbilitiesChoiseContainer");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            abilityData = GalatimeGlobals.getAbilityById(abilityName);

            if (abilityData.Contains("icon")) Texture = GD.Load<Texture>((string)abilityData["icon"]);

            Connect("mouse_entered", this, "_mouseEnter");
            Connect("gui_input", this, "_guiInput");
            Connect("mouse_exited", this, "_mouseExit");

            _animationPlayer.Play("idle");

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.Connect("abilities_changed", this, "_onAbilitiesChanged");
        }

        public void _onAbilitiesChanged()
        {
            for (int i = 0; i < PlayerVariables.abilities.Count; i++)
            {
                var ability = PlayerVariables.abilities[i] as Godot.Collections.Dictionary;
                if (ability.Contains("id"))
                {
                    if ((string)ability["id"] == abilityName)
                    {
                        setUsing();
                        return;
                    }
                }
            }
            setUsing(true);
        }

        public void _mouseEnter()
        {
            _tooltip._display(abilityData);
        }

        public void _mouseExit()
        {
            _tooltip._hide();
        }

        public void setUsing(bool idle = false)
        {
            _animationPlayer.Play(idle ? "idle" : "using");
        }

        public void _guiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton)
            {
                var @mouseEvent = @event as InputEventMouseButton;
                if (@mouseEvent.ButtonIndex == 1 && mouseEvent.Pressed)
                {
                    var position = RectGlobalPosition;
                    position.x -= 32;
                    position.y -= 48;
                    abilityChoiseContainer.Visible = true;
                    abilityChoiseContainer.RectGlobalPosition = position;
                    abilityChoiseContainer.choiseId = abilityName;
                }
            }
        }
    }
}