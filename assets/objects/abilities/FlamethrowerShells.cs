using Godot;
using System;
using Galatime;
using System.Security.Cryptography;

namespace Galatime {
    public partial class FlamethrowerShells : Node2D
    {
        private Vector2 _velocity;

        public float speed = 500;
        public bool canMove = true;

        private AnimationPlayer _animationPlayer;
        private Sprite2D _sprite;
        private CharacterBody2D _kinematicBody;
        private Area2D _damageArea;

        public override async void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("CharacterBody3D/AnimationPlayer");
            _sprite = GetNode<Sprite2D>("CharacterBody3D/Sprite2D");
            _kinematicBody = GetNode<CharacterBody2D>("CharacterBody3D");
            _damageArea = GetNode<Area2D>("CharacterBody3D/DamageArea");
        }

        public void _bodyEntered(Node2D body, float physicalAttack, float magicalAttack)
        {
            if (body is Entity entity)
            {
                GalatimeElement element = GalatimeElement.Ignis;
                float damageRotation = _kinematicBody.GlobalPosition.AngleToPoint(entity.GlobalPosition);
                entity.hit(1.5f, magicalAttack, element, DamageType.magical, 50, damageRotation);
            }
        }

        public async void execute(float rotation, float physicalAttack, float magicalAttack, Vector2 position)
        {
            var rand = new Random();
            Rotation = rotation + (float)rand.NextDouble() / 5 * rand.Next(-1, 2);
            _kinematicBody.GlobalPosition = position;
            _velocity.X += 1;
            _animationPlayer.Play("intro");
            _damageArea.BodyEntered += (Node2D body) => _bodyEntered(body, physicalAttack, magicalAttack);

            await ToSignal(GetTree().CreateTimer(0.5f + (float)rand.NextDouble() / 10), "timeout");
            destroy();
        }

        public void destroy()
        {
            canMove = false;
            _animationPlayer.Play("outro");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (canMove)
            {
                if (_kinematicBody.IsOnWall()) destroy();
                _kinematicBody.Velocity = _velocity.Rotated(Rotation) * speed;
                _kinematicBody.MoveAndSlide();
            }
        }
    }
}
