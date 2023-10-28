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

        private bool _isPause = false;

        // Nodes
        private Camera2D Camera;
        private PlayerVariables PlayerVariables;
        private HumanoidDoll HumanoidDoll;

        public override void _Ready()
        {
            base._Ready();

            // Get Nodes
            AnimationPlayer = GetNode<AnimationPlayer>("Animation");

            Body = this;

            Weapon = GetNode<Hand>("Hand");

            Camera = GetNode<Camera2D>("Camera3D");

            Sprite = GetNode<Sprite2D>("Sprite2D");
            TrailParticles = GetNode<GpuParticles2D>("TrailParticles");

            HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");

            PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            PlayerVariables.OnItemsChanged += _onItemsChanged;
            PlayerVariables.OnAbilitiesChanged += OnAbilitiesChanged;

            PlayerGui = GetNode<PlayerGui>("CanvasLayer/PlayerGui");

            // var playerGuiScene = ResourceLoader.Load<PackedScene>("res://assets/objects/PlayerGui.tscn");
            // playerGui = playerGuiScene.Instantiate<PlayerGui>();
            // GetNode("CanvasLayer").AddChild(playerGui);
            // playerGui.RequestReady();

            Element = GalatimeElement.Ignis + GalatimeElement.Chaos;

            // Start
            CanMove = true;

            Stamina = Stats[EntityStatType.Stamina].Value;
            Mana = Stats[EntityStatType.Mana].Value;
            Health = Stats[EntityStatType.Health].Value;

            CameraOffset = Vector2.Zero;

            Body.GlobalPosition = GlobalPosition;

            Stats.OnStatsChanged += OnStatsChanged;
            OnStatsChanged(Stats);

            PlayerVariables.setPlayerInstance(this);
        }

        public Player() : base(new(
            PhysicalAttack: 100,
            MagicalAttack: 100,
            PhysicalDefense: 100,
            MagicalDefense: 100,
            Health: 100,
            Mana: 100,
            Stamina: 100,
            Agility: 100
        ), GalatimeElement.Ignis + GalatimeElement.Chaos) {}



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

            if (CanMove && !IsDodge) Velocity = inputVelocity; else Velocity = Vector2.Zero;
            setDirectionByWeapon();

            Weapon.LookAt(GetGlobalMousePosition());
            if (IsWalk) State = Velocity.Length() == 0 ? HumanoidStates.Idle : HumanoidStates.Walk;
            HumanoidDoll.SetAnimation(VectorRotation, State);
            SetCameraPosition();

            SetLayerToWeapon(AnimationPlayer.CurrentAnimation != "idle_up" && AnimationPlayer.CurrentAnimation != "walk_up");
            TrailParticles.Texture = Sprite.Texture;
        }

        private void SetCameraPosition()
        {
            Camera.GlobalPosition = Camera.GlobalPosition.Lerp((Weapon.GlobalPosition + (GetGlobalMousePosition() - Weapon.GlobalPosition) / 5) + CameraOffset, 0.05f);
        }

        public override void _MoveProcess()
        {
            if (!_isPause) SetMove(); else Velocity = Vector2.Zero;
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
                    if (ability != existAbility) addAbility(ability, i);
                    else addAbility(ability, i);
                }
                else
                {
                    RemoveAbility(i);
                }
            }
        }

        public override AbilityData addAbility(AbilityData ab, int i)
        {
            var ability = base.addAbility(ab, i);
            PlayerGui.AddAbility(ability, i);
            return ability;
        }

        protected override bool _useAbility(int i)
        {
            var result = base._useAbility(i);
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
            Weapon.TakeItem(obj);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel")) PlayerGui.TogglePause();
            if (@event.IsActionPressed("game_attack"))
            {
                Weapon.Attack(this);
                State = HumanoidStates.Attack;
                HumanoidDoll.SetAnimation(VectorRotation, State);
            }

            if (@event.IsActionPressed("game_dodge"))
            {
                dodge();
                var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
                globals.save(PlayerVariables.currentSave, PlayerGui);
            }

            // Checking for input for abilities.
            for (int i = 0; i < PlayerVariables.abilities.Count; i++) if (@event.IsActionPressed($"game_ability_{i+1}")) _useAbility(i);
        }
    }
}
