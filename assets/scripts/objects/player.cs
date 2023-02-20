using Godot;
using System;
using Galatime;
using System.Collections.Generic;

namespace Galatime
{
    public class Player : HumanoidCharacter
    {
        // Exports
        [Export] public bool canInteract = true;
        [Export] public Vector2 cameraOffset;   
        public float cameraShakeAmount = 0;

        // Variables
        private int slots = 16;
        private int xp;
        public int Xp
        {
            get { return xp; }
            set
            {
                _playerGui.changeStats(stats, Xp);
                xp = value;
            }
        }

        private PlayerGui _playerGui;

        private bool _isPause = false;
             
        // Nodes
        private KinematicBody2D _body;

        private Camera2D _camera;

        private RichTextLabel _debug;

        private PlayerVariables _playerVariables;

        // Signals
        [Signal] public delegate void on_pause(bool visible);
        [Signal] public delegate void healthChanged(float health);
        [Signal] public delegate void on_interact();
        [Signal] public delegate void on_dialog_end();
        [Signal] public delegate void reloadDodge();

        public override void _Ready()
        {
            base._Ready();
            // Get Nodes
            _animationPlayer = GetNode<AnimationPlayer>("Animation");

            body = this;

            weapon = GetNode<Hand>("Hand");

            _camera = GetNode<Camera2D>("Camera");

            _sprite = GetNode<Sprite>("Sprite");
            _trailParticles = GetNode<Particles2D>("TrailParticles");   

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.Connect("items_changed", this, "_onItemsChanged");
            _playerVariables.Connect("abilities_changed", this, "_abilitiesChanged");

            _playerGui = GetNode<PlayerGui>("CanvasLayer/player_game_gui");

            element = GalatimeElement.Ignis + GalatimeElement.Chaos;

            stats = new EntityStats
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

            // Start
            canMove = true;

            _animationPlayer.PlaybackSpeed = speed / 100;

            stamina = stats.stamina;
            mana = stats.mana;
            health = stats.health;
            cameraOffset = Vector2.Zero;

            body.GlobalPosition = GlobalPosition;
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

        public void _abilitiesChanged()
        {
            var obj = (Godot.Collections.Dictionary)PlayerVariables.abilities;
            for (int i = 0; i < obj.Count; i++)
            {
                var ability = (Godot.Collections.Dictionary)obj[i];
                if (ability.Contains("path"))
                {
                    var existAbility = _abilities[i];
                    if (existAbility != null)
                    {
                        if ((string)ability["path"] != existAbility.ResourcePath) addAbility((string)ability["path"], i);
                    }
                    else
                    {
                        addAbility((string)ability["path"], i);
                    }
                }
                else
                {
                    removeAbility(i);
                    GD.PushWarning("no path " + i);
                }
            }
        }

        public override GalatimeAbility addAbility(string scenePath, int i)
        {
            var ability = base.addAbility(scenePath, i);
            _playerGui.addAbility(ability, i);
            return ability;
        }

        protected override bool _useAbility(int i)
        {
            var result = base._useAbility(i);
            if (!result)
            {
                _playerGui.pleaseSayNoToAbility(i);
                return result;
            }
            _playerGui.reloadAbility(i);
            return result;
        }

        protected override void removeAbility(int i)
        {
            base.removeAbility(i);
            _playerGui.removeAbility(i);
        }

        public override void _Process(float delta)
        {
            // _debug.Text = $"hp {health} stamina {stamina} mana {mana} element {element.name}";
        }

        private void _onItemsChanged()
        {       
            var obj = (Godot.Collections.Dictionary)PlayerVariables.inventory[0];
            if (weapon._item != null && obj.Count != 0) return;
            weapon.takeItem(obj);
        }

        public void startDialog(string id)
        {
            _playerGui.startDialog(id, this);
        }

        public override async void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion)
            {
                setDirectionByWeapon();
            }
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_isPause)
                {
                    _isPause = false;
                    _playerGui.pause(_isPause);
                    return;
                }
                if (!_isPause)
                {
                    _isPause = true;
                    _playerGui.pause(_isPause);
                    return;
                }
            }
            if (@event.IsActionPressed("game_attack"))
            {
                weapon.attack(stats.physicalAttack, stats.magicalAttack);
            }

            if (@event.IsActionPressed("game_dodge"))
            {
                dodge();
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