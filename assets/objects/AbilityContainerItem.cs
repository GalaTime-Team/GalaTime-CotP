using Godot;
using System;

namespace Galatime
{
    public class AbilityContainerItem : TextureRect
    {
        private Tooltip _tooltip;
        private AbilitiesChoiseContainer abilityChoiseContainer;

        [Export] public string abilityName = "unknown";

        public override void _Ready()
        {
            _tooltip = GetNode<Tooltip>("../../../Tooltip");
            abilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("../AbilitiesChoiseContainer");

            Godot.Collections.Array binds = new Godot.Collections.Array();
            binds.Add(abilityName);
            Connect("mouse_entered", this, "_mouseEnter", binds);
            Connect("gui_input", this, "_guiInput", binds);
            Connect("mouse_exited", this, "_mouseExit");
        }   

        public void _mouseEnter(string ability)
        {
            _tooltip._display(ability);
        }

        public void _mouseExit()
        {
            GD.Print("exit");
            _tooltip._hide();
        }
        
        public void _guiInput(InputEvent @event, string ability)
        {
            if (@event is InputEventMouseButton)
            {
                var @mouseEvent = @event as InputEventMouseButton;
                if (@mouseEvent.ButtonIndex == 1 && mouseEvent.Pressed)
                {
                    var position = RectGlobalPosition;
                    position.x -= 32;
                    position.y -= 32;
                    abilityChoiseContainer.RectGlobalPosition = position;
                    abilityChoiseContainer.choiseId = abilityName;
                }
            }
        }
    }
}