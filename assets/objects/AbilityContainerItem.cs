using Godot;
using System;

namespace Galatime
{
    public partial class AbilityContainerItem : TextureRect
    {
        //private Tooltip _tooltip;
        //private AbilitiesChoiseContainer abilityChoiseContainer;
        //private AnimationPlayer _animationPlayer;

        [Export] public string abilityName = "unknown";

        public Godot.Collections.Dictionary abilityData;

        private TextureRect lockedTexture;
        private AnimationPlayer animationPlayer;
        public Timer abilitySetCountdown;

        private bool learned = false;

        //private PlayerVariables _playerVariables;

        public override void _Ready()
        {
            lockedTexture = GetNode<TextureRect>("Locked");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            //_tooltip = GetNode<Tooltip>("../../../Tooltip");
            //abilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("../AbilitiesChoiseContainer");
            //abilityData = GalatimeGlobals.getAbilityById(abilityName);

            //if (abilityData.ContainsKey("icon")) Texture = GD.Load<Texture2D>((string)abilityData["icon"]);

            //Connect("mouse_entered", new Callable(this, "_mouseEnter"));
            //Connect("mouse_exited", new Callable(this, "_mouseExit"));

            //_playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            //_playerVariables.Connect("abilities_changed",new Callable(this,"_onAbilitiesChanged"));

            // lockedTexture.Material.Set("shader_parameter/whitening", 0);    

            animationPlayer.Play("idle");

            abilitySetCountdown = new Timer();
            abilitySetCountdown.WaitTime = 6.7f;
            abilitySetCountdown.OneShot = true;
            AddChild(abilitySetCountdown);
        }

        public void setLearned()
        {
            //  lockedTexture.Visible = false;
            if (!learned)
            {
                abilitySetCountdown.Start();
                animationPlayer.Play("unlocking");
                learned = true;
            }
        }

        //public void _onAbilitiesChanged()
        //{
        //    for (int i = 0; i < PlayerVariables.abilities.Count; i++)
        //    {
        //        var ability = (Godot.Collections.Dictionary)PlayerVariables.abilities[i];
        //        if (ability.ContainsKey("id"))
        //        {
        //            if ((string)ability["id"] == abilityName)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //}

        //public void _mouseEnter()
        //{
        //    _tooltip._display(abilityData);
        //}

        //public void _mouseExit()
        //{
        //    _tooltip._hide();
        //}

        //public void _guiInput(InputEvent @event)
        //{
        //    if (@event is InputEventMouseButton)
        //    {
        //        var @mouseEvent = @event as InputEventMouseButton;
        //        if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        //        {
        //            var position = GlobalPosition;
        //            position.X -= 80;
        //            position.Y -= 70;
        //            abilityChoiseContainer.Visible = true;
        //            abilityChoiseContainer.GlobalPosition = position;
        //            abilityChoiseContainer.choiseId = abilityName;
        //        }
        //    }
        //}
    }
}