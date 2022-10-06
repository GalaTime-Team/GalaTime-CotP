using Godot;
using System;
using Galatime;

namespace Galatime {
    public class player : Entity
    {
        // Exports
        [Export] public string IdleAnimation = "idle_down";

        // Variables
        private bool canMove;

        private float stamina, mana, ultimate = 100f;

        // Nodes
        private AnimationPlayer _animationPlayer;
        private AnimationPlayer _animationPlayerWeapon;

        private KinematicBody2D _body;
        private Area2D _weapon;

        private Camera2D _camera;

        private RichTextLabel _debug;

        // Signals
        [Signal] public delegate void wrap();
        [Signal] public delegate void fade(string type);
        [Signal] public delegate void healthChanged(float health);

        public override void _Ready() 
        {
            // Get Nodes
            _animationPlayer = GetNode<AnimationPlayer>("player_body/animation");

            body = GetNode<KinematicBody2D>("player_body");

            _weapon = GetNode<Area2D>("player_body/Weapon");

            _camera = GetNode<Camera2D>("player_body/camera");

            _debug = GetNode<RichTextLabel>("player_body/debuginfo");

            element = GalatimeElement.Ignis;

            // Start
            canMove = true;

            _animationPlayer.PlaybackSpeed = speed / 100;

            EmitSignal("fade", "out");
        }

        // public void _test() {
        //    hit(1, GalatimeElement.Ignis);
        //    EmitSignal("healthChanged", health);
        // }

        private void _SetAnimation(Vector2 animationVelocity)
        {   
            if (IdleAnimation == "idle_down") _setLayerToWeapon(true); else _setLayerToWeapon(false);
            if (animationVelocity.Length() == 0)
            {
                _animationPlayer.Play(IdleAnimation);
            }
            if (animationVelocity.y != 0)
            {
                if (animationVelocity.y <= -1 && _animationPlayer.CurrentAnimation != "walk_up")
                {
                    IdleAnimation = "idle_up";
                    _animationPlayer.Play("walk_up");
                }
                if (animationVelocity.y >= 1 && _animationPlayer.CurrentAnimation != "walk_down")
                {
                    IdleAnimation = "idle_down";
                    _animationPlayer.Play("walk_down");
                    _setLayerToWeapon(true);
                }
            }
            else
            {
                if (animationVelocity.x >= 1 && _animationPlayer.CurrentAnimation != "walk_right")
                {
                    IdleAnimation = "idle_right";
                    _animationPlayer.Play("walk_right");
                }
                if (animationVelocity.x <= -1 && _animationPlayer.CurrentAnimation != "walk_left")
                {
                    IdleAnimation = "idle_left";
                    _animationPlayer.Play("walk_left");
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
            _SetAnimation(inputVelocity);
            inputVelocity = inputVelocity.Normalized() * speed;

            // OS.WindowPosition = windowPosition;
            velocity = inputVelocity;

            _weapon.LookAt(GetGlobalMousePosition());
            _setCameraPosition();
        }

        private void _setCameraPosition()
        {
            _camera.GlobalPosition = _camera.GlobalPosition.LinearInterpolate((_weapon.GlobalPosition + (GetGlobalMousePosition() - _weapon.GlobalPosition) / 5), 0.05f);
        }

        private void _setLayerToWeapon(bool toUp)
        {
            if (toUp) _weapon.ZIndex = 10; else _weapon.ZIndex = 0;
        }

        public void _onWrap()
        {
            canMove = false;
            EmitSignal("fade", "in");
        }

        public override void _moveProcess()
        {
            _SetMove();
        }

        public override void _Process(float delta)
        {
            _debug.Text = $"hp {health} stamina {stamina} mana {mana} ultimate {ultimate} element {element.name}";
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("game_attack"))
            {
                _weapon.Call("attack");
            }
        }
    }
}
