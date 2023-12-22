using Galatime.Helpers;
using Godot;
using System;
using System.Collections.Generic;
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

        public override void _Ready()
        {
            base._Ready();

            // Get Nodes
            Body = this;
            Weapon = GetNode<Hand>("Hand");
            Sprite = GetNode<Sprite2D>("Sprite2D");
            TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
            HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");
            DrinkingAudioPlayer = GetNode<AudioStreamPlayer2D>("DrinkingAudioPlayer");

            Camera = GetNode<Camera2D>("Camera");

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            PlayerVariables.OnItemsChanged += OnItemsChanged;
            PlayerVariables.OnAbilitiesChanged += OnAbilitiesChangedForCharacter;

            PlayerGui = GetNode<PlayerGui>("CanvasLayer/PlayerGui");

            // Start
            Stamina.Value = Stats[EntityStatType.Stamina].Value;
            Mana.Value = Stats[EntityStatType.Mana].Value;
            Health = Stats[EntityStatType.Health].Value;

            CameraOffset = Vector2.Zero;
            Body.GlobalPosition = GlobalPosition;

            // Stats.OnStatsChanged += OnStatsChanged;
            // OnStatsChanged(Stats);

            // PlayerVariables.OnAlliesChanged += LoadCharacters;

            PlayerVariables.SetPlayerInstance(this);
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            Stats.OnStatsChanged -= OnStatsChanged;
            PlayerVariables.OnItemsChanged -= OnItemsChanged;
            PlayerVariables.OnAbilitiesChanged -= OnAbilitiesChangedForCharacter;
            PlayerVariables.OnAlliesChanged -= LoadCharacters;
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
            inputVelocity = inputVelocity.Normalized() * Speed;

            if (CanMove && !IsDodge) CurrentCharacter.Body.Velocity = inputVelocity; else Body.Velocity = Vector2.Zero;

            CurrentCharacter?.Weapon.LookAt(GetGlobalMousePosition());
            SetCameraPosition();
        }

        private void SetCameraPosition()
        {
            var c = CurrentCharacter;
            var cpos = c.Weapon.GlobalPosition;
            Camera.GlobalPosition = Camera.GlobalPosition.Lerp(cpos + ((GetGlobalMousePosition() - c.Weapon.GlobalPosition) / 5 + CameraOffset), 0.05f);
        }

        public override void _MoveProcess()
        {
            base._MoveProcess();

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

        public new void HealthChangedEvent(float health)
        {
            PlayerGui?.OnHealthChanged(health);
        }

        public new void OnManaChanged(float mana)
        {
            PlayerGui.OnManaChanged(mana);
        }

        public new void OnStaminaChanged(float stamina)
        {
            PlayerGui.OnStaminaChanged(stamina);
        }

        public void OnAbilitiesChangedForCharacter()
        {
            var c = CurrentCharacter;
            if (c == null) return;
            var abilityList = c.Abilities;
            if (CurrentAlly.ID == "arthur") abilityList = PlayerVariables.Abilities.ToList();

            if (abilityList is null) return;
            for (int i = 0; i < abilityList.Count; i++)
            {
                var ability = abilityList[i];
                if (CurrentAlly.ID == "arthur")
                {
                    // Print current ability information
                    GD.PrintRich($"[color=purple]ABILITIES CHANGED FOR PLAYER[/color]: Is empty: {ability.IsEmpty}. Name: {ability.Name}. Index: {i}");
                    var existAbility = c.Abilities[i];
                    GD.PrintRich($"[color=green]EXIST ABILITY[/color]: Is empty: {existAbility.IsEmpty}. Name: {existAbility.Name}. Index: {i}");
                    if (ability != existAbility) c.AddAbility(ability, i);
                    else c.AddAbility(ability, i);
                }
                PlayerGui.AddAbility(ability, i);
            }
        }

        // public override AbilityData AddAbility(AbilityData ab, int i)
        // {
        //     var ability = base.AddAbility(ab, i);
        //     PlayerGui.AddAbility(ability, i);
        //     return ability;
        // }

        // public override bool UseAbility(int i)
        // {
        //     var result = base.UseAbility(i);
        //     if (!result)
        //     {
        //         PlayerGui.GetAbilityContainer(i).No();
        //         return result;
        //     }
        //     return result;
        // }

        // public override void _OnAbilityReload(int i)
        // {
        //     var ability = Abilities[i];
        //     var abilityContainer = PlayerGui.GetAbilityContainer(i);
        //     abilityContainer.StartReload(ability.Charges);
        // }

        public void OnAbilityAddedForCharacter(AbilityData ab, int i)
        {
            GD.Print("OnAbilityAddedForCharacter: AbilityData: ", ab.ToString(), " Index: ", i);
            PlayerGui.AddAbility(ab, i);
        }

        public void OnAbilityUsedForCharacter(int i, bool result)
        {
            GD.Print("OnAbilityUsedForCharacter: Index: ", i, " Result: ", result);

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
            GD.Print("OnAbilityReloadForCharacter: Index: ", i);

            var ability = CurrentCharacter?.Abilities[i];
            var abilityContainer = PlayerGui.GetAbilityContainer(i);
            abilityContainer.StartReload(ability.Charges, (float)previousTime);
        }

        // public void RemoveAbilityForCharacter(int i)
        // {
        //     PlayerGui.RemoveAbility(i);
        // }

        private void OnItemsChanged()
        {
            var obj = PlayerVariables.Inventory[0];
            if (obj == Weapon.ItemData) return;
            if (obj.IsEmpty) Weapon.RemoveItem();
            Weapon.TakeItem(obj);
        }

        /// <summary> Loads characters for the game and spawns them in the scene. </summary>
        public void LoadCharacters()
        {
            foreach (var character in PlayerVariables.Allies)
            {
                // Checking if the character is not empty and no instance.
                if (!character.IsEmpty && character.Instance == null)
                {
                    var hc = character.Scene.Instantiate<HumanoidCharacter>();
                    // Top level so they don't be affected by the Player transform.
                    hc.TopLevel = true;
                    hc.GlobalPosition = GlobalPosition;
                    GetParent().AddChild(hc);
                    character.Instance = hc;
                }
            }
        }

        public void SwitchCharacter(AllyData data)
        {
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
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("game_attack")) Weapon.Attack(this);
            if (@event.IsActionPressed("game_dodge"))
            {
                CurrentCharacter?.Dodge();
            }

            if (@event.IsActionPressed("game_inventory"))
            {
                PlayerGui.InventoryOpen = !PlayerGui.InventoryOpen;
                CanMove = !PlayerGui.InventoryOpen;
            }

            if (@event.IsActionPressed("game_potion_wheel")) PlayerGui.CallConsumableWheel();
            if (@event.IsActionPressed("game_character_wheel")) PlayerGui.CallCharacterWheel();


            // Checking for input for abilities.
            for (int i = 0; i < PlayerVariables.Abilities.Length; i++) if (@event.IsActionPressed($"game_ability_{i + 1}")) CurrentCharacter?.UseAbility(i);
        }

        public void StartDialog(string id) => PlayerGui.DialogBox.StartDialog(id);
    }
}
