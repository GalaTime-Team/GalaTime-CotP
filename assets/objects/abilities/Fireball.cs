using Godot;
using System;
using Galatime;
using System.Security.Cryptography;

namespace Galatime {
    public partial class Fireball : GalatimeAbility
    {
        private Vector2 _velocity;

        public float speed = 500;
        public bool canMove = true;

        private AnimationPlayer _animationPlayer;
        private Sprite2D _sprite;
        private CharacterBody2D _kinematicBody;
        private Area2D _damageArea;

        public Fireball() : base(
            GD.Load("res://sprites/gui/abilities/ignis/fire_ball.png") as Texture2D,
            5,
            2f,
            new System.Collections.Generic.Dictionary<string, float>() { { "mana", 10 } }
        )
        { }

        public override async void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("CharacterBody3D/AnimationPlayer");
            _sprite = GetNode<Sprite2D>("CharacterBody3D/Sprite2D");
            _kinematicBody = GetNode<CharacterBody2D>("CharacterBody3D");
            _damageArea = GetNode<Area2D>("CharacterBody3D/DamageArea");

            _animationPlayer.AnimationFinished += (StringName name) => animationFinished(name);
        }

        public void animationFinished(StringName name)
        {
            if (name == "intro")
            {
                _animationPlayer.Play("loop");
            }
        }

        public void _bodyEntered(Node2D body, float physicalAttack, float magicalAttack)
        {
            if (body is Entity entity)
            {
                GalatimeElement element = GalatimeElement.Ignis;
                float damageRotation = _kinematicBody.GlobalPosition.AngleToPoint(entity.GlobalPosition);
                entity.hit(20, magicalAttack, element, DamageType.magical, 500, damageRotation);
                destroy();
            }
        }

        public override async void execute(HumanoidCharacter p, float physicalAttack, float magicalAttack)
        {
            Rotation = p.weapon.Rotation;
            _kinematicBody.GlobalPosition = p.weapon.GlobalPosition;

            _velocity.X += 1;
            if (PlayerVariables.player is Player player) player.cameraShakeAmount += 10;

            _animationPlayer.Play("intro");
            _damageArea.BodyEntered += (Node2D body) => _bodyEntered(body, physicalAttack, magicalAttack);

            await ToSignal(GetTree().CreateTimer(duration), "timeout");
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
