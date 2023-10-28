using System.Collections.Generic;
using System;
using Godot;

namespace Galatime
{
    public partial class PlayerGui : Control
    {
        #region Nodes
        public ColorRect FadeScreen;

        // Stats
        public ValueBar HealthValueBar;
        public ValueBar StaminaValueBar;
        public ValueBar ManaValueBar;

        // Text Stats
        public Label DodgeCountdownText;
        public Timer DodgeTextTimer;
        public Label VersionText;

        public DialogBox DialogBox;

        public Panel PauseContainer;

        // Abilities
        public HBoxContainer AbilitiesContainer;
        public AbilitiesContainer AbilitiesListContainer;

        // Audio
        public AudioStreamPlayer ParrySound;
        public ColorRect ParryOverlay;
        #endregion

        #region Scenes
        public string ParryFlashScenePath = "res://assets/objects/gui/ParryFlash.tscn";
        public PackedScene ParryFlashScene;
        #endregion

        #region Variables
        public float RemainingDodge;
        public List<AbilityContainer> AbilityContainers = new();
        private bool isPause = false;
        public bool IsPause {
            get => isPause;
            set {
                isPause = value;
                OnPause?.Invoke(isPause );
                PauseContainer.Visible = isPause;
            }
        }

        public PlayerVariables PlayerVariables;
        #endregion

        #region Events
        public Action OnItemsChanged;
        public Action<bool> OnPause;
        #endregion

        public override void _Ready()
        {
            #region Get nodes
            FadeScreen = GetNode<ColorRect>("FadeScreen");  

            // Stats
            HealthValueBar = GetNode<ValueBar>("HealthValueBar");
            StaminaValueBar = GetNode<ValueBar>("StaminaValueBar");
            ManaValueBar = GetNode<ValueBar>("ManaValueBar");

            VersionText = GetNode<Godot.Label>("Version");
            DodgeCountdownText = GetNode<Godot.Label>("LeftCenter/DodgeContainer/Countdown");

            DialogBox = GetNode<DialogBox>("DialogBox");
            
            AbilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            AbilitiesListContainer = GetNode<AbilitiesContainer>("PauseContainer/AbilitiesContainer");
            PauseContainer = GetNode<Panel>("PauseContainer");

            ParrySound = GetNode<AudioStreamPlayer>("ParrySound");
            ParryOverlay = GetNode<ColorRect>("ParryOverlay");
            #endregion
            
            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            PlayerVariables.OnItemsChanged += DisplayItem;

            // Setting up dodge timer
            DodgeTextTimer = new Timer();
            DodgeTextTimer.Timeout += ProcessDodgeReloading;
            AddChild(DodgeTextTimer);

            ParryFlashScene = ResourceLoader.Load<PackedScene>(ParryFlashScenePath);

            // Setting up text of the version
            VersionText.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.version}\n{GalatimeConstants.versionDescription}";

            // Loading ability containers scene 
            var abilityContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/AbilityContainer.tscn");
            // Adding ability containers
            for (int i = 0; i < PlayerVariables.abilitySlots; i++) {
                // Instantiate ability container and add it to the abilities container
                var instance = abilityContainerScene.Instantiate<AbilityContainer>();
                AbilitiesContainer.AddChild(instance);
                AbilityContainers.Add(instance);
            }

            OnFade(false);
        }

        public override void _ExitTree() {
            PlayerVariables.OnItemsChanged -= DisplayItem;
        }

        public void OnStatsChanged(EntityStats stats)
        {
            HealthValueBar.MaxValue = stats[EntityStatType.Health].Value;
            StaminaValueBar.MaxValue = stats[EntityStatType.Stamina].Value;
            ManaValueBar.MaxValue = stats[EntityStatType.Mana].Value;
        }

        /// <summary> Makes the fade animation, which fades the screen in and out. </summary>
        /// <param name="type"> The type of fade. True for fade in, false for fade out. </param>
        /// <param name="duration"> The duration of the fade. </param>
        /// <param name="callback"> The callback to call when the fade is finished. </param>
        public void OnFade(bool type, float duration = 0.5f, Action callback = null)
        {
            // Make sure the fade screen is visible. I wait it to be hidden in the editor, not in a game.
            FadeScreen.Visible = true;

            // Start fade by setting the alpha to 0 with ease
            var tween = GetTree().CreateTween();
            tween.TweenProperty(FadeScreen, "modulate:a", type ? 1 : 0, duration);
            if (callback != null) tween.Finished += callback;
        }

        /// <summary> Plays a parry effect by pausing the game, showing an overlay, and playing a sound. </summary>
        /// <param name="position"> The position of the parry effect in global coordinates. </param>
        public async void ParryEffect(Vector2 position) {
            var parryFlashInstance = ParryFlashScene.Instantiate<GpuParticles2D>();
            GetTree().Root.AddChild(parryFlashInstance);
            parryFlashInstance.GlobalPosition = position;
            parryFlashInstance.Emitting = true;

            GetTree().Paused = true;
            ParrySound.Play();  

            ParryOverlay.Visible = true;

            await ToSignal(GetTree().CreateTimer(.36f), "timeout");
            ParryOverlay.Visible = false;
            parryFlashInstance.Emitting = false;
            parryFlashInstance.QueueFree();
            GetTree().Paused = false;
        }

        public void OnHealthChanged(float health) => HealthValueBar.Value = health;
        public void OnStaminaChanged(float stamina) => StaminaValueBar.Value = stamina;
        public void OnManaChanged(float mana) => ManaValueBar.Value = mana;

        public void AddAbility(AbilityData ab, int i) => AbilitiesContainer.GetChild<AbilityContainer>(i).Load(ab);
        public void RemoveAbility(int i) => AbilitiesContainer.GetChild<AbilityContainer>(i).Unload();

        public void ReloadDodge()
        {
            RemainingDodge = 2;
            DodgeTextTimer.Start();
            ProcessDodgeReloading();
        }

        private void ProcessDodgeReloading()
        {
            RemainingDodge--;
            DodgeCountdownText.Text = RemainingDodge + "s";
            if (DodgeCountdownText.Text == "-1s")
            {
                DodgeTextTimer.Stop(); DodgeCountdownText.Text = "";
            }
        }
        
        public AbilityContainer GetAbilityContainer(int i)
        {
            var ability = AbilitiesContainer.GetChild(i) as AbilityContainer;
            return ability;
        }

        public void DisplayItem() => OnItemsChanged?.Invoke();
        public void TogglePause() => IsPause = !IsPause;
    }
}