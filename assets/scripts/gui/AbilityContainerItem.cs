using Godot;

namespace Galatime
{
    public partial class AbilityContainerItem : TextureRect
    {
        private Tooltip Tooltip;
        public AnimationPlayer AnimationPlayer;

        [Export] public string AbilityName = "Unknown";

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
            // TODO: REWORK THIS CRAP
            Tooltip = GetNode<Tooltip>("../../../../Tooltip");
            #endregion

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;

            LockedTexture.Material.Set("shader_parameter/whitening", 0);
            AnimationPlayer.Play("idle");

            LoadAbility(AbilityName);
        }

        public void LoadAbility(string name)
        {
            AbilityData = GalatimeGlobals.GetAbilityById(name);
            Texture = AbilityData.Icon;
        }

        public void OnMouseEntered() => Tooltip.Display(AbilityData);
        public void OnMouseExited() => Tooltip.HideTooltip();
    }
}