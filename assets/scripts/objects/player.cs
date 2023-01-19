    using Godot;
using System;
using Galatime;
using System.Collections.Generic;

namespace Galatime
{
    public class Player : Entity
    {
        // Exports
        [Export] public string IdleAnimation = "idle_down";
        [Export] public bool canInteract = true;
        [Export] public bool canMove;
        [Export] public Vector2 cameraOffset;
        public float cameraShakeAmount = 0;

        // Variables
        private int slots = 16;
        private float mana, ultimate = 100f;
        public float stamina { get; private set; }
        private bool _isPause = false;
        private bool _isDodge = false;

        public new EntityStats stats = new EntityStats
        {
            physicalAttack = 75,
            magicalAttack = 80,
            physicalDefence = 65,
            magicalDefense = 75,
            health = 70,
            mana = 65,
            stamina = 65,
            agility = 60
        };

        private List<PackedScene> _abilities = new List<PackedScene>();
        private Timer[] _abilitiesTimers = new Timer[3];
        private int[] _abiltiesReloadTimes = new int[3];

        private Timer _staminaCountdownTimer;
        private Timer _staminaRegenTimer;
        private Timer _dodgeTimer;

        private Vector2 _vectorRotation;

        // Nodes
        private AnimationPlayer _animationPlayer;
        private AnimationPlayer _animationPlayerWeapon;

        private KinematicBody2D _body;
        public Hand weapon;

        private Camera2D _camera;

        private RichTextLabel _debug;

        private PlayerVariables _playerVariables;

        private Sprite _sprite;
        private Particles2D _trailParticles;

        // Signals
        [Signal] public delegate void wrap();
        [Signal] public delegate void on_pause(bool visible);
        [Signal] public delegate void fade(string type);
        [Signal] public delegate void healthChanged(float health);
        [Signal] public delegate void on_stamina_changed(float stamina);
        [Signal] public delegate void on_interact();
        [Signal] public delegate void on_dialog_start(string id);
        [Signal] public delegate void on_dialog_end();
        [Signal] public delegate void on_ability_add(GalatimeAbility ab);
        [Signal] public delegate void reloadAbility(int i);
        [Signal] public delegate void sayNoToAbility(int i);

        public override void _Ready()
        {
            // Get Nodes
            _animationPlayer = GetNode<AnimationPlayer>("player_body/animation");

            body = GetNode<KinematicBody2D>("player_body");

            weapon = GetNode<Hand>("player_body/Hand");

            _camera = GetNode<Camera2D>("player_body/camera");

            _debug = GetNode<RichTextLabel>("player_body/debuginfo");

            _sprite = GetNode<Sprite>("player_body/Sprite");
            _trailParticles = GetNode<Particles2D>("player_body/TrailParticles");

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.Connect("items_changed", this, "_onItemsChanged");

            element = GalatimeElement.Ignis;

            addAbility("res://assets/objects/abilities/fireball.tscn", 0);
            addAbility("res://assets/objects/abilities/blueFireball.tscn", 1);

            // Start
            canMove = true;

            _animationPlayer.PlaybackSpeed = speed / 100;

            EmitSignal("fade", "out");

            stamina = 150;

            cameraOffset = Vector2.Zero;

            _staminaCountdownTimer = new Timer();
            _staminaCountdownTimer.WaitTime = 5f;
            _staminaCountdownTimer.OneShot = true;
            _staminaCountdownTimer.Connect("timeout", this, "_onCountdownStaminaRegen");
            AddChild(_staminaCountdownTimer);

            _staminaRegenTimer = new Timer();
            _staminaRegenTimer.WaitTime = 1f;
            _staminaRegenTimer.OneShot = false;
            _staminaRegenTimer.Connect("timeout", this, "_regenStamina");
            AddChild(_staminaRegenTimer);

            _dodgeTimer = new Timer();
            _dodgeTimer.WaitTime = 0.2f;
            _dodgeTimer.OneShot = true;
            _dodgeTimer.Connect("timeout", this, "_onCountdownDodge");
            AddChild(_staminaRegenTimer);
        }

        private void _SetAnimation(Vector2 animationVelocity, bool idle)
        {
            _setLayerToWeapon(_animationPlayer.CurrentAnimation == "idle_up" || _animationPlayer.CurrentAnimation == "walk_up" ? false : true) ;
            if (idle) _animationPlayer.Stop();
            if (animationVelocity.y != 0)
            {
                if (animationVelocity.y <= -1 && _animationPlayer.CurrentAnimation != "walk_up")
                {
                    if (!idle) _animationPlayer.Play("walk_up"); else _animationPlayer.Play("idle_up");
                }
                if (animationVelocity.y >= 1 && _animationPlayer.CurrentAnimation != "walk_down")
                {
                    if (!idle) _animationPlayer.Play("walk_down"); else _animationPlayer.Play("idle_down");
                    _setLayerToWeapon(true);
                }
            }
            else
            {
                if (animationVelocity.x >= 1 && _animationPlayer.CurrentAnimation != "walk_right")
                {
                    if (!idle) _animationPlayer.Play("walk_right"); else _animationPlayer.Play("idle_right");
                }
                if (animationVelocity.x <= -1 && _animationPlayer.CurrentAnimation != "walk_left")
                {
                    if (!idle) _animationPlayer.Play("walk_left"); else _animationPlayer.Play("idle_left");
                }
            }
            _trailParticles.Texture = _sprite.Texture;
        }

        public void addAbility(string scenePath, int i)
        {
            PackedScene scene = GD.Load<PackedScene>(scenePath);
            GalatimeAbility ability = scene.Instance<GalatimeAbility>();
            _abilities.Add(scene);
            EmitSignal("on_ability_add", ability, i);
            var binds = new Godot.Collections.Array();
            binds.Add(i);
            _abilitiesTimers[i] = new Timer();
            _abilitiesTimers[i].Connect("timeout", this, "_abilitiesCountdown", binds);
            AddChild(_abilitiesTimers[i]);
        }

        private void _abilitiesCountdown(int i)
        {
            if (_abiltiesReloadTimes[i] <= 0) _abilitiesTimers[i].Stop();
            _abiltiesReloadTimes[i]--;
            GD.Print(_abiltiesReloadTimes[i]);
        }

        private void _useAbility(int i)
        {
            try
            {
                if (_abiltiesReloadTimes[i] <= 0)
                {
                    GD.Print(_abilities[i]);
                    var ability = _abilities[i].Instance<GalatimeAbility>();
                    if (ability.costs.ContainsKey("stamina")) { 
                        if (stamina - ability.costs["stamina"] < 0) 
                        { 
                            EmitSignal("sayNoToAbility", i); return; 
                        } 
                    }
                    if (ability.costs.ContainsKey("mana")) {
                        if (stamina - ability.costs["mana"] < 0) {
                            EmitSignal("sayNoToAbility", i); return; 
                        } 
                    }
                    GetParent().AddChild(ability);
                    EmitSignal("reloadAbility", i);
                    ability.execute(this, stats.physicalAttack, stats.magicalAttack);
                    reduceStamina(ability.costs["stamina"]);
                    _abilitiesTimers[i].Stop();
                    _abilitiesTimers[i].Start();
                    _abiltiesReloadTimes[i] = (int)Math.Round(ability.reload);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr("Error when used ability: " + ex.Message);
            }
        }

        private void _SetMove()
        {
            Vector2 inputVelocity = Vector2.Zero;
            // Vector2 windowPosition = OS.WindowPosition;

            if (Input.IsActionPressed("game_move_up"))
            {
                inputVelocity.y -= 1;
                // windowPosition.y -= 1;
            }
            if (Input.IsActionPressed("game_move_down"))
            {
                inputVelocity.y += 1;
                // windowPosition.y += 1;
            }
            if (Input.IsActionPressed("game_move_right"))
            {
                inputVelocity.x += 1;
                // windowPosition.x += 1;
            }   
            if (Input.IsActionPressed("game_move_left"))
            {
                inputVelocity.x -= 1;
                // windowPosition.x -= 1;
            }
           inputVelocity = inputVelocity.Normalized() * speed;

            // OS.WindowPosition = windowPosition;
            if (canMove && !_isDodge) velocity = inputVelocity; else velocity = Vector2.Zero;

            weapon.LookAt(GetGlobalMousePosition());
            _SetAnimation(_vectorRotation, velocity.Length() == 0 ? true : false);
            _setCameraPosition();
        }


        private void _setCameraPosition()
        {
            _camera.GlobalPosition = _camera.GlobalPosition.LinearInterpolate((weapon.GlobalPosition + (GetGlobalMousePosition() - weapon.GlobalPosition) / 5) + cameraOffset, 0.05f);
        }

        private void _setLayerToWeapon(bool toUp)
        {
            if (toUp) weapon.ZIndex = 10; else weapon.ZIndex = -10;
        }

        public void _onWrap()
        {
            canMove = false;
            EmitSignal("fade", "in");
        }

        public override void _moveProcess()
        {
            if (!_isPause) _SetMove(); else velocity = Vector2.Zero;
            var shakeOffset = new Vector2();

            
            Random rnd = new();
            shakeOffset.x = rnd.Next(-1, 1) * cameraShakeAmount;
            shakeOffset.y = rnd.Next(-1, 1) * cameraShakeAmount;

            _camera.Offset = shakeOffset;

            cameraShakeAmount = Mathf.Lerp(cameraShakeAmount, 0, 0.05f);
        }

        public override void _healthChangedEvent(float health)
        {
            EmitSignal("healthChanged", health);
        }

        public void reduceStamina(float stam)
        {
            stamina -= stam;
            stamina = Mathf.Clamp(stamina, 0, 150);
            _OnStaminaChanged(stamina);
        }

        private void _OnStaminaChanged(float stam)
        {
            _staminaRegenTimer.Stop();
            _staminaCountdownTimer.Start();
            EmitSignal("on_stamina_changed", stam);
        }

        public void _onCountdownStaminaRegen()
        {
            GD.Print("workkfkffk");
            _staminaCountdownTimer.Stop();
            _staminaRegenTimer.Start();
        }

        public void _regenStamina()
        {
            stamina += 10;
            stamina = Mathf.Clamp(stamina, 0, 150);
            EmitSignal("on_stamina_changed", stamina);
            if (stamina >= 150) _staminaRegenTimer.Stop();
        }

        public override void _Process(float delta)
        {
            _debug.Text = $"hp {health} stamina {stamina} mana {mana} ultimate {ultimate} element {element.name}";
        }

        private void _onItemsChanged()
        {
            var obj = (Godot.Collections.Dictionary)PlayerVariables.inventory[0];
            weapon.takeItem(obj);
        }

        public void startDialog(string id)
        {
            EmitSignal("on_dialog_start", id, this);
        }

        public override async void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion)
            {
                var r = Mathf.Wrap(weapon.RotationDegrees, 0, 360);
                var v = Vector2.Zero;
                if (r <= 45) v = Vector2.Right;
                if (r >= 45 && r <= 135) v = Vector2.Down;
                if (r >= 135 && r <= 220) v = Vector2.Left;
                if (r >= 220 && r <= 320) v = Vector2.Up;
                if (r >= 320) v = Vector2.Right;
                _vectorRotation = v;
            }
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_isPause)
                {
                    _isPause = false;
                    EmitSignal("on_pause", _isPause);
                    return;
                }
                if (!_isPause)
                {
                    _isPause = true;
                    EmitSignal("on_pause", _isPause);
                    return;
                }
            }
            if (@event.IsActionPressed("game_attack"))
            {
                weapon.attack(stats.physicalAttack, stats.magicalAttack);
            }

            if (@event.IsActionPressed("game_dodge"))
            {
                if (stamina - 20 >= 0)
                {
                    _isDodge = true;
                    float direction = weapon.Rotation + 3.14159f;
                    setKnockback(900, direction);
                    _trailParticles.Emitting = true;
                    reduceStamina(20);
                    await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
                    _isDodge = false;
                    _trailParticles.Emitting = false;
                }
            }

            if (@event.IsActionPressed("game_ability_1")) _useAbility(0);
            if (@event.IsActionPressed("game_ability_2")) _useAbility(1);
            if (@event.IsActionPressed("game_ability_3")) _useAbility(2);

            if (@event.IsActionPressed("ui_accept"))
            {
                if (canInteract)
                {
                    EmitSignal("on_interact");
                }
            }
        }
    }
}