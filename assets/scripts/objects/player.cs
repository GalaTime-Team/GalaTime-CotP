using Galatime.Helpers;
using Godot;
using System;

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

        // public Player() : base(new EntityStats(new()
        // {
        //     [EntityStatType.Health] = 100,
        //     [EntityStatType.Mana] = 100,
        //     [EntityStatType.Stamina] = 100,
        //     [EntityStatType.PhysicalAttack] = 100,
        //     [EntityStatType.PhysicalDefense] = 100,
        //     [EntityStatType.MagicalAttack] = 100,
        //     [EntityStatType.MagicalDefense] = 100,
        //     [EntityStatType.Agility] = 100,
        // })) {}

        public override void _Ready()
        {
            base._Ready();

            // Get Nodes
            Body = this;
            Weapon = GetNode<Hand>("Hand");
            Sprite = GetNode<Sprite2D>("Sprite2D");
            TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
            HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");

            Camera = GetNode<Camera2D>("Camera");

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            PlayerVariables.OnItemsChanged += _onItemsChanged;
            PlayerVariables.OnAbilitiesChanged += OnAbilitiesChanged;

            PlayerGui = GetNode<PlayerGui>("CanvasLayer/PlayerGui");

            Element = GalatimeElement.Ignis + GalatimeElement.Chaos;

            // Start
            Stamina = Stats[EntityStatType.Stamina].Value;
            Mana = Stats[EntityStatType.Mana].Value;
            Health = Stats[EntityStatType.Health].Value;

            CameraOffset = Vector2.Zero;
            Body.GlobalPosition = GlobalPosition;

            Stats.OnStatsChanged += OnStatsChanged;
            OnStatsChanged(Stats);

            PlayerVariables.setPlayerInstance(this);
        }

        public override void _ExitTree()
        {
            Stats.OnStatsChanged -= OnStatsChanged;
            PlayerVariables.OnItemsChanged -= _onItemsChanged;
            PlayerVariables.OnAbilitiesChanged -= OnAbilitiesChanged;
        }

        private void OnStatsChanged(EntityStats stats)
        {
            Health = Stats[EntityStatType.Health].Value;
            Mana = Stats[EntityStatType.Mana].Value;
            Stamina = Stats[EntityStatType.Stamina].Value;

            PlayerGui.OnStatsChanged(stats);
        }

        private void SetMove()
        {
            Vector2 inputVelocity = Vector2.Zero;
            if (Input.IsActionPressed("game_move_up")) inputVelocity.Y -= 1;
            if (Input.IsActionPressed("game_move_down")) inputVelocity.Y += 1;
            if (Input.IsActionPressed("game_move_right")) inputVelocity.X += 1;
            if (Input.IsActionPressed("game_move_left")) inputVelocity.X -= 1;
            inputVelocity = inputVelocity.Normalized() * Speed;

            if (CanMove && !IsDodge) Body.Velocity = inputVelocity; else Body.Velocity = Vector2.Zero;

            Weapon.LookAt(GetGlobalMousePosition());
            SetCameraPosition();
        }

        private void SetCameraPosition()
        {
            Camera.GlobalPosition = Camera.GlobalPosition.Lerp(Weapon.GlobalPosition + (GetGlobalMousePosition() - Weapon.GlobalPosition) / 5 + CameraOffset, 0.05f);
        }

        public override void _MoveProcess()
        {
            base._MoveProcess();

            if (CanMove) SetMove(); else Body.Velocity = Vector2.Zero;
            var shakeOffset = new Vector2();

            Random rnd = new();
            shakeOffset.X = rnd.Next(-1, 1) * CameraShakeAmount;
            shakeOffset.Y = rnd.Next(-1, 1) * CameraShakeAmount;

            Camera.Offset = shakeOffset;

            CameraShakeAmount = Mathf.Lerp(CameraShakeAmount, 0, 0.05f);
        }

        public override void HealthChangedEvent(float health)
        {
            PlayerGui?.OnHealthChanged(health);
        }

        protected override void OnManaChanged(float mana)
        {
            PlayerGui.OnManaChanged(mana);
        }

        protected override void OnStaminaChanged(float stamina)
        {
            PlayerGui.OnStaminaChanged(stamina);
        }

        private void OnAbilitiesChanged()
        {
            for (int i = 0; i < PlayerVariables.abilities.Count; i++)
            {
                var ability = PlayerVariables.abilities[i];
                // Print current ability information
                GD.PrintRich($"[color=purple]ABILITIES CHANGED FOR PLAYER[/color]: Is empty: {ability.IsEmpty}. Name: {ability.Name}. Index: {i}");
                if (!ability.IsEmpty)
                {
                    var existAbility = Abilities[i];
                    GD.PrintRich($"[color=green]EXIST ABILITY[/color]: Is empty: {existAbility.IsEmpty}. Name: {existAbility.Name}. Index: {i}");
                    if (ability != existAbility) AddAbility(ability, i);
                    else AddAbility(ability, i);
                }
                else
                {
                    RemoveAbility(i);
                }
            }
        }

        public override AbilityData AddAbility(AbilityData ab, int i)
        {
            var ability = base.AddAbility(ab, i);
            PlayerGui.AddAbility(ability, i);
            return ability;
        }

        protected override bool UseAbility(int i)
        {
            var result = base.UseAbility(i);
            if (!result)
            {
                PlayerGui.GetAbilityContainer(i).No();
                return result;
            }
            return result;
        }

        protected override void _OnAbilityReload(int i)
        {
            var ability = Abilities[i];
            var abilityContainer = PlayerGui.GetAbilityContainer(i);
            abilityContainer.StartReload(ability.Charges);
        }

        protected override void RemoveAbility(int i)
        {
            base.RemoveAbility(i);
            PlayerGui.RemoveAbility(i);
        }

        public override void _Process(double delta)
        {
            // _debug.Text = $"hp {health} stamina {stamina} mana {mana} element {element.name}";
        }

        private void _onItemsChanged()
        {
            var obj = PlayerVariables.inventory[0];
            if (obj == Weapon.ItemData) return;
            if (obj.IsEmpty) Weapon.RemoveItem();
            Weapon.TakeItem(obj);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("game_attack")) Weapon.Attack(this);
            if (@event.IsActionPressed("game_dodge"))
            {
                Dodge();
            }

            if (@event.IsActionPressed("game_inventory"))
            {
                PlayerGui.InventoryOpen = !PlayerGui.InventoryOpen;
                CanMove = !PlayerGui.InventoryOpen;
            }

            // Checking for input for abilities.
            for (int i = 0; i < PlayerVariables.abilities.Count; i++) if (@event.IsActionPressed($"game_ability_{i + 1}")) UseAbility(i);
        }

        public void StartDialog(string id) => PlayerGui.DialogBox.StartDialog(id);
    }
}
