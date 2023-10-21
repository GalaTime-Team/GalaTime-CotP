using Godot;

namespace Galatime
{
    public partial class AbilityContainerItem : TextureRect
    {
        private Tooltip Tooltip;
        // private AbilitiesChoiseContainer AbilityChoiceContainer;
        private AnimationPlayer AnimationPlayer;

        private string abilityName = "Unknown";
        [Export] public string AbilityName {
            get => abilityName;
            set {
                abilityName = value;
                
                AbilityData = GalatimeGlobals.GetAbilityById(value);
                Texture = AbilityData.Icon;
            }
        }

        public AbilityData AbilityData;

        private TextureRect LockedTexture;
        private bool Learned = false;

        public override void _Ready()
        {
            #region Get nodes
            LockedTexture = GetNode<TextureRect>("Locked");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            Tooltip = GetNode<Tooltip>("../../../Tooltip");
            // AbilityChoiceContainer = GetNode<AbilitiesChoiseContainer>("../AbilitiesChoiseContainer");
            #endregion

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;

            LockedTexture.Material.Set("shader_parameter/whitening", 0);

            AnimationPlayer.Play("idle");

        }

        public void SetLearned()
        {
            LockedTexture.Visible = false;
            if (!Learned)
            {
                AnimationPlayer.Play("unlocking");
                Learned = true;
            }
        }

        public void OnMouseEntered() => Tooltip.Display(AbilityData);
        public void OnMouseExited() => Tooltip.Hide();

        // public void _guiInput(InputEvent @event)
        // {
        //     if (@event is InputEventMouseButton)
        //     {
        //         var @mouseEvent = @event as InputEventMouseButton;
        //         if (@mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        //         {
        //             var position = GlobalPosition;
        //             position.X -= 80;
        //             position.Y -= 70;
        //             AbilityChoiceContainer.Visible = true;
        //             AbilityChoiceContainer.GlobalPosition = position;
        //             AbilityChoiceContainer.ChoiceId = AbilityName;
        //         }
        //     }
        // }
    }
}