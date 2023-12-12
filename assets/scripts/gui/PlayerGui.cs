using Galatime.UI;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galatime
{
    public partial class PlayerGui : Control
    {
        #region Nodes
        public ColorRect FadeScreen;
        public PauseMenu PauseMenu;

        // Stats
        public ValueBar HealthValueBar;
        public ValueBar StaminaValueBar;
        public ValueBar ManaValueBar;

        public Label VersionText;
        public DialogBox DialogBox;
        public Panel InventoryPanel;

        // Abilities
        public HBoxContainer AbilitiesContainer;
        public AbilitiesContainer AbilitiesListContainer;

        // Audio
        public AudioStreamPlayer ParrySound;
        public ColorRect ParryOverlay;

        public SelectWheel SelectWheel;
        #endregion

        #region Scenes
        public string ParryFlashScenePath = "res://assets/objects/gui/ParryFlash.tscn";
        public PackedScene ParryFlashScene;
        #endregion

        #region Variables
        public float RemainingDodge;
        public List<AbilityContainer> AbilityContainers = new();
        private bool inventoryOpen;
        public bool InventoryOpen
        {
            get => InventoryPanel.Visible;
            set
            {
                inventoryOpen = value;
                InventoryPanel.Visible = value;
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
            PauseMenu = GetNode<PauseMenu>("PauseMenu");

            HealthValueBar = GetNode<ValueBar>("HealthValueBar");
            StaminaValueBar = GetNode<ValueBar>("StaminaValueBar");
            ManaValueBar = GetNode<ValueBar>("ManaValueBar");

            VersionText = GetNode<Label>("Version");
            DialogBox = GetNode<DialogBox>("DialogBox");
            AbilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            AbilitiesListContainer = GetNode<AbilitiesContainer>("InventoryContainer/AbilitiesContainer");
            InventoryPanel = GetNode<Panel>("InventoryContainer");

            ParrySound = GetNode<AudioStreamPlayer>("ParrySound");
            ParryOverlay = GetNode<ColorRect>("ParryOverlay");

            SelectWheel = GetNode<SelectWheel>("SelectWheel");
            #endregion

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            PlayerVariables.OnItemsChanged += DisplayItem;

            ParryFlashScene = ResourceLoader.Load<PackedScene>(ParryFlashScenePath);

            // Setting up text of the version
            VersionText.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.Version}\n{GalatimeConstants.VersionDescription}";

            // Loading ability containers scene 
            var abilityContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/gui/AbilityContainer.tscn");
            // Adding ability containers
            for (int i = 0; i < PlayerVariables.AbilitySlots; i++)
            {
                // Instantiate ability container and add it to the abilities container
                var instance = abilityContainerScene.Instantiate<AbilityContainer>();
                AbilitiesContainer.AddChild(instance);
                AbilityContainers.Add(instance);
            }

            OnFade(false);
        }

        public override void _ExitTree()
        {
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
        public async void ParryEffect(Vector2 position)
        {
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

        public AbilityContainer GetAbilityContainer(int i)
        {
            var ability = AbilitiesContainer.GetChild(i) as AbilityContainer;
            return ability;
        }

            public void CallConsumableWheel() 
            {
                var itemContainerScene = ResourceLoader.Load<PackedScene>("res://assets/objects/ItemContainer.tscn");

                var max = SelectWheel.WheelSegmentMaxCount;

                // Needed arrays for the wheel to work.
                var placeholders = new ItemContainer[max];
                var names = new string[max];

                var inventory = PlayerVariables.GetConsumables();

                var count = Math.Min(inventory.Length, max);

                // Don't do anything if no specified items.
                if (count == 0) return;

                for (int i = 0; i < count; i++)
                {
                    // Instantiate item container and add it to the wheel segment.
                    var ic = itemContainerScene.Instantiate<ItemContainer>();

                    var item = inventory[i];
                    ic.Data = item;
                    
                    // Add item container to the arrays of placeholders and names.
                    placeholders[i] = ic;
                    names[i] = item.Name;

                    // Disabling mouse filter to make sure that hover event will be triggered.
                    ic.MouseFilter = MouseFilterEnum.Ignore;
                    // Set position of the item container in the wheel segment.
                    ic.Position = new Vector2(10, 3);
                }

                SelectWheel.CallWheel("item_wheel", count, placeholders, names, (int i) => {
                    var items = PlayerVariables.GetConsumables();
                    // Check if the index is valid.
                    if (i < 0 || i >= items.Length) return;
                    var item = items[i];
                    item.Use();
                    GD.Print($"Consumed {item.Name}, now have {item.Quantity}.");
                });
            }

        public void DisplayItem() => OnItemsChanged?.Invoke();
    }
}