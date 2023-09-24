using Godot;

namespace Galatime
{
    public partial class AbilityContainerItem : TextureRect
    {
        private Tooltip _tooltip;
        private AbilitiesChoiseContainer abilityChoiseContainer;
        private AnimationPlayer _animationPlayer;

        [Export] public string abilityName = "unknown";

        public AbilityData abilityData;

        private TextureRect lockedTexture;
        private AnimationPlayer animationPlayer;
        public Timer abilitySetCountdown;

        private bool learned = false;

        private PlayerVariables _playerVariables;

        public override void _Ready()
        {
            lockedTexture = GetNode<TextureRect>("Locked");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            _tooltip = GetNode<Tooltip>("../../../Tooltip");
            abilityChoiseContainer = GetNode<AbilitiesChoiseContainer>("../AbilitiesChoiseContainer");
            abilityData = GalatimeGlobals.getAbilityById(abilityName);

            Texture = abilityData.Icon;

            Connect("mouse_entered", new Callable(this, "_mouseEnter"));
            Connect("mouse_exited", new Callable(this, "_mouseExit"));

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.OnAbilitiesChanged += _onAbilitiesChanged;

            lockedTexture.Material.Set("shader_parameter/whitening", 0);

            animationPlayer.Play("idle");

            abilitySetCountdown = new Timer
            {
                WaitTime = 6.7f,
                OneShot = true
            };
            AddChild(abilitySetCountdown);
        }

        public override void _ExitTree()
        {
            _playerVariables.OnAbilitiesChanged -= _onAbilitiesChanged;
        }

        public void setLearned()
        {
            lockedTexture.Visible = false;
            if (!learned)
            {
                abilitySetCountdown.Start();
                animationPlayer.Play("unlocking");
                learned = true;
            }
        }

        public void _onAbilitiesChanged()
        {
            for (int i = 0; i < _playerVariables.abilities.Count; i++)
            {
                var ability = _playerVariables.abilities[i];
                if (ability.ID == abilityName)
                {
                    return;
                }
            }
        }

        public void _mouseEnter()
        {
            _tooltip.Display(abilityData);
        }

        public void _mouseExit()
        {
            _tooltip._hide();
        }

        public void _guiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton)
            {
                var @mouseEvent = @event as InputEventMouseButton;
                if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                {
                    var position = GlobalPosition;
                    position.X -= 80;
                    position.Y -= 70;
                    abilityChoiseContainer.Visible = true;
                    abilityChoiseContainer.GlobalPosition = position;
                    abilityChoiseContainer.ChoiceId = abilityName;
                }
            }
        }
    }
}