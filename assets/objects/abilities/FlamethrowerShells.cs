using Godot;
using System;
using Galatime;
using System.Security.Cryptography;

namespace Galatime {
    public class FlamethrowerShells : Node2D
    {
        private Vector2 _velocity;

        public float speed = 500;
        public bool canMove = true;

        private AnimationPlayer _animationPlayer;
        private Sprite _sprite;
        private KinematicBody2D _kinematicBody;
        private Area2D _damageArea;

        public override async void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("KinematicBody/AnimationPlayer");
            _sprite = GetNode<Sprite>("KinematicBody/Sprite");
            _kinematicBody = GetNode<KinematicBody2D>("KinematicBody");
            _damageArea = GetNode<Area2D>("KinematicBody/DamageArea");
        }

        public void _bodyEntered(KinematicBody2D body, float physicalAttack, float magicalAttack)
        {
            Entity parent = body.GetParent<Entity>();
            GalatimeElement element = GalatimeElement.Ignis;

            // Get angle of damage
            float damageRotation = _kinematicBody.GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
            if (parent.HasMethod("hit"))
            {
                parent.hit(2, magicalAttack, element, DamageType.magical, 100, damageRotation);
            }
        }

        public async void execute(float rotation, float physicalAttack, float magicalAttack, Vector2 position)
        {
            var rand = new Random();
            Rotation = rotation + (float)rand.NextDouble() / 5 * rand.Next(-1, 2);
            _kinematicBody.GlobalPosition = position;
            _velocity.x += 1;
            _animationPlayer.Play("intro");
            var binds = new Godot.Collections.Array();
            binds.Add(physicalAttack);
            binds.Add(magicalAttack);
            _damageArea.Connect("body_entered", this, "_bodyEntered", binds);

            await ToSignal(GetTree().CreateTimer(0.4f + (float)rand.NextDouble() / 10), "timeout");
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
                if (_kinematicBody.IsOnWall()) destroy();
                _kinematicBody.MoveAndSlide(_velocity.Rotated(Rotation) * speed);
            }
        }
    }
}
