using Godot;

namespace Galatime
{
    public partial class AbilityContainerItem : TextureRect
    {
        private Tooltip Tooltip;
        public AnimationPlayer AnimationPlayer;

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

        private bool learned = false;
        private bool Learned {
            get => learned;
            set {
                learned = value;
                LockedTexture.Modulate = learned ? new Color(1, 1, 1, 0) : new Color(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Sets the ability as learned and plays unlocking animation.
        /// </summary>
        /// <param name="learned"> If true, the ability is learned. </param>
        public void SetLearned(bool learned, bool playAnimation = false)
        {
            if (playAnimation) AnimationPlayer.Play("unlocking");
            Learned = learned;
        }

        public override void _Ready()
        {
            #region Get nodes
            LockedTexture = GetNode<TextureRect>("Locked");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            Tooltip = GetNode<Tooltip>("../../../Tooltip");
            #endregion

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;

            LockedTexture.Material.Set("shader_parameter/whitening", 0);
            AnimationPlayer.Play("idle");

        }

        public void OnMouseEntered() => Tooltip.Display(AbilityData);
        public void OnMouseExited() => Tooltip.Hide();
    }
}