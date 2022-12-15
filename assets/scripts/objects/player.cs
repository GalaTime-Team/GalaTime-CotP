using Godot;
using System;
using Galatime;

namespace Galatime
{
    public class player : Entity
    {
        // Exports
        [Export] public string IdleAnimation = "idle_down";
        [Export] public bool canInteract = true;
        [Export] public bool canMove;

        // Variables

        private int slots = 16;
        private float stamina, mana, ultimate = 100f;
        private bool _isPause = false;

        private Vector2 _vectorRotation;

        // Nodes
        private AnimationPlayer _animationPlayer;
        private AnimationPlayer _animationPlayerWeapon;

        private KinematicBody2D _body;
        private Hand _weapon;

        private Camera2D _camera;

        private RichTextLabel _debug;

        private PlayerVariables _playerVariables;

        // Signals
        [Signal] public delegate void wrap();
        [Signal] public delegate void on_pause(bool visible);
        [Signal] public delegate void fade(string type);
        [Signal] public delegate void healthChanged(float health);
        [Signal] public delegate void on_interact();
        [Signal] public delegate void on_dialog_start(string id);   

        public override void _Ready()
        {
            // Get Nodes
            _animationPlayer = GetNode<AnimationPlayer>("player_body/animation");

            body = GetNode<KinematicBody2D>("player_body");

            _weapon = GetNode<Hand>("player_body/Hand");

            _camera = GetNode<Camera2D>("player_body/camera");

            _debug = GetNode<RichTextLabel>("player_body/debuginfo");

            _playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerVariables.Connect("items_changed", this, "_onItemsChanged");

            element = GalatimeElement.Ignis;

            // Start
            canMove = true;

            _animationPlayer.PlaybackSpeed = speed / 100;

            EmitSignal("fade", "out");

            health = 100;
        }

        // public void _test() {
        //    hit(1, GalatimeElement.Ignis);
        //    EmitSignal("healthChanged", health);
        // }

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
            velocity = inputVelocity;

            _weapon.LookAt(GetGlobalMousePosition());
            _SetAnimation(_vectorRotation, inputVelocity.Length() == 0 ? true : false);
            _setCameraPosition();
        }


        private void _setCameraPosition()
        {
            _camera.GlobalPosition = _camera.GlobalPosition.LinearInterpolate((_weapon.GlobalPosition + (GetGlobalMousePosition() - _weapon.GlobalPosition) / 5), 0.05f);
        }

        private void _setLayerToWeapon(bool toUp)
        {
            if (toUp) _weapon.ZIndex = 10; else _weapon.ZIndex = -10;
        }

        public void _onWrap()
        {
            canMove = false;
            EmitSignal("fade", "in");
        }

        public override void _moveProcess()
        {
            if (!_isPause) _SetMove();
        }

        public override void _healthChangedEvent(float health)
        {
            EmitSignal("healthChanged", health);
        }

        public override void _Process(float delta)
        {
            _debug.Text = $"hp {health} stamina {stamina} mana {mana} ultimate {ultimate} element {element.name}";
        }

        private void _onItemsChanged()
        {
            var obj = (Godot.Collections.Dictionary)PlayerVariables.inventory[0];
            _weapon.takeItem(obj);
        }

        public void startDialog(string id)
        {
            EmitSignal("on_dialog_start", id);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion)
            {
                var r = Mathf.Wrap(_weapon.RotationDegrees, 0, 360);
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
                // _weapon.Call("attack");
                PackedScene abilityScene = GD.Load<PackedScene>("res://assets/objects/abilities/fireball.tscn");
                GalatimeAbility ability = (GalatimeAbility)abilityScene.Instance();
                ability.rotation = _weapon.Rotation;
                ability.GlobalPosition = _weapon.GlobalPosition;
                GetParent().AddChild(ability);
            }
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