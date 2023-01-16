using Godot;
using System;
using Galatime;
using System.Security.Cryptography;

namespace Galatime {
    public class Fireball : GalatimeAbility
    {
        private Vector2 _velocity;

        public float speed = 10;
        public bool canMove = true;

        private AnimationPlayer _animationPlayer;
        private Sprite _sprite;
        private KinematicBody2D _kinematicBody;
        private Area2D _damageArea;

        public Fireball() : base(
            GD.Load("res://sprites/gui/abilities/ignis/fire_ball.png") as Texture,
            2,
            2f,
            new System.Collections.Generic.Dictionary<string, float>() { { "stamina", 10 } }
        ) { }

        public override async void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("KinematicBody/AnimationPlayer");
            _sprite = GetNode<Sprite>("KinematicBody/Sprite");
            _kinematicBody = GetNode<KinematicBody2D>("KinematicBody");
            _damageArea = GetNode<Area2D>("KinematicBody/DamageArea");

            _damageArea.Connect("body_entered", this, "_bodyEntered");
        }

        public void _bodyEntered(KinematicBody2D body)
        {
            Node2D parent = body.GetParent<Node2D>();
            GalatimeElement element = GalatimeElement.Ignis;

            // Get angle of damage
            float damageRotation = _kinematicBody.GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
            if (parent.HasMethod("hit"))
            {
                parent.Call("hit", 25, element, 500, damageRotation);
                destroy();
            }
        }

        public override async void execute(Player p)
        {
            Rotation = p.weapon.Rotation;
            _kinematicBody.GlobalPosition = p.weapon.GlobalPosition;

            _velocity.x += 1;
            p.cameraShakeAmount += 10;

            _animationPlayer.Play("intro");

            await ToSignal(GetTree().CreateTimer(duration), "timeout");
            destroy();
        }

        public void destroy()
        {
            canMove = false;
            _animationPlayer.Play("outro");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (canMove)
            {
                _kinematicBody.MoveAndCollide(_velocity.Rotated(Rotation) * speed);
            }
        }
    }
}
