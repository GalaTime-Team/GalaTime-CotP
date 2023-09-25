using System.Collections.Generic;
using System;
using Godot;

namespace Galatime
{
    public partial class PlayerGui : Control
    {
        #region Nodes
        public AnimationPlayer FadeAnimation;
        public ColorRect FadeScreen;

        // Stats
        public ValueBar HealthValueBar;
        public ValueBar StaminaValueBar;
        public ValueBar ManaValueBar;
        public TextureProgressBar HealthDrainProgressBar;

        // Text Stats
        public Godot.Label StaminaLabel;
        public Godot.Label ManaLabel;
        public Godot.Label DodgeCountdownText;
        public RichTextLabel TextStats;

        public Timer DodgeTextTimer;
        public Godot.Label VersionText;

        public NinePatchRect DialogBox;

        public Panel PauseContainer;

        // Abilities
        public HBoxContainer AbilitiesContainer;
        public AbilitiesContainer AbilitiesListContainer;

        // Stats
        public GridContainer StatsContainer;

        // Audio
        public AudioStreamPlayer ParrySound;
        public ColorRect ParryOverlay;
        #endregion

        #region Variables
        public float LocalHp = 0f;
        public float RemainingDodge;
        public List<AbilityContainer> AbilityContainers = new();

        public PlayerVariables PlayerVariables;
        #endregion

        #region Events
        public Action OnItemsChanged;
        public Action<bool> OnPause;
        #endregion

        public override void _Ready()
        {
            #region Get nodes
            FadeAnimation = GetNode<AnimationPlayer>("FadeAnimationPlayer");  
            FadeScreen = GetNode<ColorRect>("FadeScreen");  

            // Stats
            HealthValueBar = GetNode<ValueBar>("HealthValueBar");
            StaminaValueBar = GetNode<ValueBar>("StaminaValueBar");
            ManaValueBar = GetNode<ValueBar>("ManaValueBar");

            VersionText = GetNode<Godot.Label>("Version");
            DodgeCountdownText = GetNode<Godot.Label>("LeftCenter/DodgeContainer/Countdown");

            DialogBox = GetNode<NinePatchRect>("DialogBox");
            
            AbilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            AbilitiesListContainer = GetNode<AbilitiesContainer>("PauseContainer/AbilitiesContainer");

            PauseContainer = GetNode<Panel>("PauseContainer");
            StatsContainer = GetNode<GridContainer>("PauseContainer/StatsContainer/Stats");

            ParrySound = GetNode<AudioStreamPlayer>("ParrySound");
            ParryOverlay = GetNode<ColorRect>("ParryOverlay");
            #endregion
            
            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            
            AbilitiesListContainer._onAbilityLearned();

            PlayerVariables.OnItemsChanged += DisplayItem;

            // Setting up dodge timer
            DodgeTextTimer = new Timer();
            DodgeTextTimer.Timeout += ProcessDodgeReloading;
            AddChild(DodgeTextTimer);

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


        /// <summary>
        /// Makes the fade animation, which fades the screen in and out.
        /// </summary>
        /// <param name="type">The type of fade</param>
        /// <param name="duration">The duration of the fade</param>
        /// <param name="callback">The callback to call when the fade is finished</param>
        public void OnFade(bool type, float duration = 0.5f, Action callback = null)
        {
            // Make sure the fade screen is visible. I wait it to be hidden in the editor, not in a game.
            FadeScreen.Visible = true;

            // Start fade by setting the alpha to 0 with ease
            var tween = GetTree().CreateTween();
            tween.TweenProperty(FadeScreen, "modulate:a", type ? 1 : 0, duration);
            tween.Finished += () => callback?.Invoke();
        }

        public async void ParryEffect() {
            GetTree().Paused = true;
            ParryOverlay.Visible = true;
            ParrySound.Play();  
            await ToSignal(GetTree().CreateTimer(.36f), "timeout");
            ParryOverlay.Visible = false;
            GetTree().Paused = false;
        }

        public void OnHealthChanged(float health)
        {   
            HealthValueBar.Value = health;
        }

        public void OnStaminaChanged(float stamina)
        {
            StaminaValueBar.Value = stamina;
        }

        public void OnManaChanged(float mana)
        {
            ManaValueBar.Value = mana;
        }

        public void ChangeStats(EntityStats entityStats, float XP)
        {
            if (StatsContainer.GetChildCount() <= 0)
            {
                var statContainerScene = GD.Load<PackedScene>("res://assets/objects/gui/StatContainer.tscn");
                for (int i = 0; i < 8; i++)
                {
                    var instance = statContainerScene.Instantiate<StatContainer>();
                    StatsContainer.AddChild(instance);
                    instance.OnUpgrade += (int id) => OnUpgradeStat(id, instance);
                }
            }

            var statContainers = StatsContainer.GetChildren();
            for (int i = 0; i < entityStats.Count; i++)
            {
                var statContainer = statContainers[i] as StatContainer;
                var stat = entityStats[i];
                statContainer.loadData(stat, (int)XP);
            }
        }

        private void OnUpgradeStat(int id, StatContainer instance)
        {
            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            var result = playerVariables.upgradeStat((EntityStatType)id);
            instance.playAnimation(result);
        }

        public void AddAbility(AbilityData ab, int i)
        {
            AbilitiesContainer.GetChild<AbilityContainer>(i).Load(ab);
        }

        public void RemoveAbility(int i)
        {
            AbilitiesContainer.GetChild<AbilityContainer>(i).Unload();
        }

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

        public void StartDialog(string id, Player p)
        {
            DialogBox.Call("startDialog", id, p);
        }

        public void DisplayItem()
        {
            OnItemsChanged?.Invoke();
        }

        public void pause(bool visible)
        {
            OnPause?.Invoke(visible);
            PauseContainer.Visible = visible;
        }
    }
}