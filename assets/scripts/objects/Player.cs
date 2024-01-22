using Galatime.Global;
using Galatime.Helpers;
using Godot;
using System;
using System.Linq;

namespace Galatime
{
    public partial class Player : HumanoidCharacter
    {
        // Exports
        [Export] public bool CanInteract = true;
        [Export] public Vector2 CameraOffset;
        public float CameraShakeAmount = 0;

        // Variables
        private int xp;
        public int Xp
        {
            get { return xp; }
            set
            {
                xp = value;
                PlayerVariables.OnXpChanged?.Invoke(xp);
            }
        }

        public PlayerGui PlayerGui;

        // Nodes
        private Camera2D Camera;
        private PlayerVariables PlayerVariables;

        /// <summary> The player's current possessed character. </summary>
        public static HumanoidCharacter CurrentCharacter { get; private set; }
        /// <summary> The player's current possessed ally. The same as CurrentCharacter, but CurrentCharacter is a HumanoidCharacter. This object contains all character data. </summary>
        public static AllyData CurrentAlly { get; private set; }
        /// <summary> The identifier for the main character. </summary>
        public string MainCharacterId = "arthur";
        /// <summary> Provides convenient access to the main character object that is identified by the MainCharacterId. </summary>
        public TestCharacter MainCharacter
        {
            get => Array.Find(PlayerVariables.Allies, x => x.ID == MainCharacterId).Instance as TestCharacter;
        }

        private bool isPlayerFrozen = false;
        public bool IsPlayerFrozen
        {
            get => isPlayerFrozen;
            set
            {
                isPlayerFrozen = value;

                CurrentCharacter.DisableHumanoidDoll = isPlayerFrozen;
                if (isPlayerFrozen) WindowManager.Instance.CloseAll();
            }
        }

        public override void _Ready()
        {
            base._Ready();

            // Get Nodes
            Body = this;

            // Humanoid components
            Weapon = GetNode<Hand>("Hand");
            Sprite = GetNode<Sprite2D>("Sprite2D");
            TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
            HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");
            DrinkingAudioPlayer = GetNode<AudioStreamPlayer2D>("DrinkingAudioPlayer");

            Camera = GetNode<Camera2D>("Camera");
            PlayerGui = GetNode<PlayerGui>("CanvasLayer/PlayerGui");

            // Player variables
            PlayerVariables = PlayerVariables.Instance;
            PlayerVariables.OnItemsChanged += OnItemsChanged;
            PlayerVariables.OnAbilitiesChanged += OnAbilitiesChangedForCharacter;

            // Start
            Stamina.Value = Stats[EntityStatType.Stamina].Value;
            Mana.Value = Stats[EntityStatType.Mana].Value;
            Health = Stats[EntityStatType.Health].Value;

            CameraOffset = Vector2.Zero;
            Body.GlobalPosition = GlobalPosition;

            // PlayerVariables.SetPlayerInstance(this);

            PlayerVariables.LoadVariables(this);
            // Load Main character
            LoadCharactersFirst();
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            PlayerVariables.OnItemsChanged -= OnItemsChanged;
            PlayerVariables.OnAbilitiesChanged -= OnAbilitiesChangedForCharacter;
            Array.ForEach(PlayerVariables.Allies, x => x.Instance = null);

            CurrentCharacter = null;
            CurrentAlly = null;
        }

        private void OnStatsChanged(EntityStats stats)
        {
            HumanoidCharacter c = CurrentCharacter;
            PlayerGui.OnStatsChanged(stats, c.Health, c.Stamina.Value, c.Mana.Value);
        }

        private void SetMove()
        {
            Vector2 inputVelocity = Vector2.Zero;
            if (Input.IsActionPressed("game_move_up")) inputVelocity.Y -= 1;
            if (Input.IsActionPressed("game_move_down")) inputVelocity.Y += 1;
            if (Input.IsActionPressed("game_move_right")) inputVelocity.X += 1;
            if (Input.IsActionPressed("game_move_left")) inputVelocity.X -= 1;
            inputVelocity = inputVelocity.Normalized() * CurrentCharacter.Speed;

            if (CanMove && !IsDodge && !IsPlayerFrozen) CurrentCharacter.Body.Velocity = inputVelocity; else Body.Velocity = Vector2.Zero;

            CurrentCharacter?.Weapon.LookAt(GetGlobalMousePosition());
            SetCameraPosition();
        }

        private void SetCameraPosition()
        {
            var c = CurrentCharacter;
            var cpos = c.Weapon.GlobalPosition;
            Camera.GlobalPosition = Camera.GlobalPosition.Lerp(cpos + ((GetGlobalMousePosition() - c.Weapon.GlobalPosition) / 5 + CameraOffset), 0.05f);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._MoveProcess(delta);

            // Don't move if the player is not event exist.
            if (CurrentCharacter == null || !IsInstanceValid(CurrentCharacter)) return;

            if (CanMove) SetMove(); else if (CurrentCharacter?.Body != null) CurrentCharacter.Body.Velocity = Vector2.Zero;
            var shakeOffset = new Vector2();

            Random rnd = new();
            shakeOffset.X = rnd.Next(-1, 1) * CameraShakeAmount;
            shakeOffset.Y = rnd.Next(-1, 1) * CameraShakeAmount;

            Camera.Offset = shakeOffset;

            CameraShakeAmount = Mathf.Lerp(CameraShakeAmount, 0, 0.05f);
        }

        public new void HealthChangedEvent(float health) => PlayerGui?.OnHealthChanged(health);
        public new void OnManaChanged(float mana) => PlayerGui.OnManaChanged(mana);
        public new void OnStaminaChanged(float stamina) => PlayerGui.OnStaminaChanged(stamina);
        public void OnAbilitiesChangedForCharacter() => OnAbilitiesChangedForCharacter(false);

        /// <summary> Event for when character's abilities changed. </summary>
        public void OnAbilitiesChangedForCharacter(bool justUpdate = true)
        {
            // Retrieve the current character and terminate if not present.
            var c = CurrentCharacter;
            if (c == null) return;

            var abilityList = c.Abilities;
            // If the current ally is the main character, use the main character's abilities instead.
            if (CurrentAlly.ID == MainCharacterId) abilityList = PlayerVariables.Abilities.ToList();

            // Terminate if there are no abilities to process.
            if (abilityList is null) return;
            for (int i = 0; i < abilityList.Count; i++)
            {
                var ability = abilityList[i];
                if (!justUpdate)
                {
                    // Print current ability information
                    // GD.PrintRich($"[color=purple]ABILITIES CHANGED FOR PLAYER[/color]: Is empty: {ability.IsEmpty}. Name: {ability.Name}. Index: {i}");
                    var existAbility = c.Abilities[i];
                    // GD.PrintRich($"[color=green]EXIST ABILITY[/color]: Is empty: {existAbility.IsEmpty}. Name: {existAbility.Name}. Index: {i}");
                    if (ability != existAbility) c.AddAbility(ability, i);

                    // Regardless of the comparison result, add ability at the specified index.
                    else c.AddAbility(ability, i);
                }

                // Update the GUI with the ability
                PlayerGui.AddAbility(ability, i);
            }
        }

        public void OnAbilityAddedForCharacter(AbilityData ab, int i)
        {
            // GD.Print("OnAbilityAddedForCharacter: AbilityData: ", ab.ToString(), " Index: ", i);
            PlayerGui.AddAbility(ab, i);
        }

        public void OnAbilityUsedForCharacter(int i, bool result)
        {
            // GD.Print("OnAbilityUsedForCharacter: Index: ", i, " Result: ", result);

            // If result is false, then failed to use the ability.
            if (!result)
            {
                PlayerGui.GetAbilityContainer(i).No();
                return;
            }

            var ability = CurrentCharacter?.Abilities[i];
            // Send reload the ability to UI because it's used.
            PlayerGui.GetAbilityContainer(i).StartReload(ability.Charges);
        }

        public void OnAbilityReloadForCharacter(int i, double previousTime)
        {
            // GD.Print("OnAbilityReloadForCharacter: Index: ", i);

            var ability = CurrentCharacter?.Abilities[i];
            var abilityContainer = PlayerGui.GetAbilityContainer(i);
            // GD.Print($"Start reloading: {ability.Charges}, {previousTime}");
            abilityContainer.StartReload(ability.Charges, (float)previousTime);
        }

        private void OnItemsChanged()
        {
            var obj = PlayerVariables.Inventory[0];
            if (obj == MainCharacter?.Weapon.ItemData) return;
            if (obj.IsEmpty) MainCharacter?.Weapon.RemoveItem();
            MainCharacter?.Weapon.TakeItem(obj);
        }

        /// If the current character is possessed, attempts to switch control to a living ally character.
        public void OnDeathCharacter()
        {
            var characters = Array.FindAll(PlayerVariables.Allies, x => x.Instance != null && !x.Instance.DeathState);
            if ((CurrentCharacter as TestCharacter).Possessed && characters.Length > 0)
            {
                // If a living ally character is found, switch control to that character.
                var character = characters[0];
                SwitchCharacter(character);
            }
        }

        /// <summary> Loads characters and switches to the main character. </summary>
        public void LoadCharactersFirst() => CallDeferred(nameof(LoadCharacters), MainCharacterId); // For some reason, it should be called in idle frame even if it's called in _Ready.

        /// <summary> Loads characters for the game and spawns them in the scene. </summary>
        public void LoadCharacters(string characterToSwitchId = "")
        {
            foreach (var character in PlayerVariables.Allies)
            {
                // Checking if the character is not empty and no instance.
                if (character != null && !character.IsEmpty && character.Instance == null)
                {
                    var hc = character.Scene.Instantiate<HumanoidCharacter>();
                    GetParent().AddChild(hc);
                    character.Instance = hc;

                    hc.GlobalPosition = GlobalPosition;

                    hc.OnDeath += OnDeathCharacter;
                }
            }

            // Ensure main character is properly loaded.
            var mainCharacter = MainCharacter;
            if (mainCharacter != null)
            {
                // Remove any previous death event bindings to prevent duplicate triggers.
                mainCharacter.OnDeath -= PlayerGui.DeathScreenContainer.CallDeath;

                // Bind main character's death event to the game's UI, to display the death screen when main character dies.
                mainCharacter.OnDeath += PlayerGui.DeathScreenContainer.CallDeath;
            }

            // Switch character if needed right after loading characters.
            if (characterToSwitchId != "")
            {
                // Find specified character.
                var ally = PlayerVariables.Allies.FirstOrDefault(x => x.ID == characterToSwitchId);
                if (ally != default) SwitchCharacter(ally);
            }
        }

        public void SwitchCharacter(AllyData data)
        {
            if (data.Instance is not null && data.Instance.DeathState) return; // Don't let to switch character if it's dead.

            // Make sure that all characters are loaded.
            LoadCharacters();

            // Unsubscribe from events from the previous character.
            if (CurrentCharacter != null)
            {
                CurrentCharacter.Stats.OnStatsChanged -= OnStatsChanged;
                CurrentCharacter.Stamina.OnValueChanged -= OnStaminaChanged;
                CurrentCharacter.Mana.OnValueChanged -= OnManaChanged;
                CurrentCharacter.OnHealthChanged -= HealthChangedEvent;

                CurrentCharacter.OnAbilityAdded -= OnAbilityAddedForCharacter;
                CurrentCharacter.OnAbilityUsed -= OnAbilityUsedForCharacter;
                CurrentCharacter.OnAbilityReload -= OnAbilityReloadForCharacter;

                (CurrentCharacter as TestCharacter).Possessed = false;
            }

            CurrentCharacter = data.Instance;
            CurrentAlly = data;

            // Subscribe to events for the new character.
            CurrentCharacter.Stats.OnStatsChanged += OnStatsChanged;
            CurrentCharacter.Stamina.OnValueChanged += OnStaminaChanged;
            CurrentCharacter.Mana.OnValueChanged += OnManaChanged;
            CurrentCharacter.OnHealthChanged += HealthChangedEvent;

            CurrentCharacter.OnAbilityAdded += OnAbilityAddedForCharacter;
            CurrentCharacter.OnAbilityUsed += OnAbilityUsedForCharacter;
            CurrentCharacter.OnAbilityReload += OnAbilityReloadForCharacter;

            // Set the character as possessed to control it.
            (CurrentCharacter as TestCharacter).Possessed = true;

            // Again, update the UI.
            OnStatsChanged(CurrentCharacter.Stats);
            OnAbilitiesChangedForCharacter();
            OnItemsChanged();

            PlayerGui.SetCharacterIcon(CurrentAlly);
        }

        // All input handling for the player goes here.
        public override void _UnhandledInput(InputEvent @event)
        {
            if (IsPlayerFrozen) return;
            if (@event.IsActionPressed("game_attack")) CurrentCharacter?.Weapon.Attack(CurrentCharacter);
            if (@event.IsActionPressed("game_dodge")) CurrentCharacter?.Dodge();
            if (@event.IsActionPressed("game_inventory")) PlayerGui.InventoryOpen = !PlayerGui.InventoryOpen;
            if (@event.IsActionPressed("game_potion_wheel")) PlayerGui.CallConsumableWheel();
            if (@event.IsActionPressed("game_character_wheel")) PlayerGui.CallCharacterWheel();

            // Checking for input for abilities.
            for (int i = 0; i < PlayerVariables.Abilities.Length; i++) if (@event.IsActionPressed($"game_ability_{i + 1}")) CurrentCharacter?.UseAbility(i);
        }

        public void StartDialog(string id) => PlayerGui.DialogBox.StartDialog(id);
        public void StartDialog(string id, Action dialogEndCallback) => PlayerGui.DialogBox.StartDialog(id, dialogEndCallback);
        public void StartCutscene(string cutscene) => CutsceneManager.Instance.StartCutscene(cutscene);
    }
}