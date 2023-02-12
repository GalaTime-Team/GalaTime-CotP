using Godot;
using System;
using Galatime;
using System.Security.Cryptography;

namespace Galatime
{
    public class Firewave : GalatimeAbility
    {
        private Vector2 _velocity;

        public float speed = 2;
        public bool canMove = true;

        private AnimationPlayer _animationPlayer;
        private Sprite _sprite;
        private KinematicBody2D _kinematicBody;
        private Area2D _damageArea;

        public Firewave() : base(
            GD.Load("res://sprites/gui/abilities/ignis/firewave.png") as Texture,
            20,
            6f,
            new System.Collections.Generic.Dictionary<string, float>() { { "mana", 10 } }
        )
        { }

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
                parent.hit(20, magicalAttack, element, DamageType.magical, 500, damageRotation);
            }
        }

        public override async void execute(Player p, float physicalAttack, float magicalAttack)
        {
            Rotation = p.weapon.Rotation;
            _kinematicBody.GlobalPosition = p.weapon.GlobalPosition;

            _velocity.x += 1;
            p.cameraShakeAmount += 10;

            _animationPlayer.Play("intro");
            var binds = new Godot.Collections.Array();
            binds.Add(physicalAttack);
            binds.Add(magicalAttack);
            _damageArea.Connect("body_entered", this, "_bodyEntered", binds);

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
