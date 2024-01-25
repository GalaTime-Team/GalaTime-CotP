using Godot;

namespace Galatime
{
    public partial class Firewave : GalatimeAbility
    {
        private Vector2 _velocity;

        public float speed = 2;
        public bool canMove = true;

        private AnimationPlayer _animationPlayer;
        // private Sprite2D _sprite;
        private CharacterBody2D _kinematicBody;
        private Area2D _damageArea;

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("CharacterBody/AnimationPlayer");
            // _sprite = GetNode<Sprite2D>("CharacterBody/Sprite");
            _kinematicBody = GetNode<CharacterBody2D>("CharacterBody");
            _damageArea = GetNode<Area2D>("CharacterBody/DamageArea");

            _animationPlayer.AnimationFinished += animationFinished;
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
                entity.TakeDamage(20, magicalAttack, element, DamageType.Magical, 500, damageRotation);
            }
        }

        public override async void Execute(HumanoidCharacter p)
        {
            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            Rotation = p.Weapon.Rotation;
            _kinematicBody.GlobalPosition = p.Weapon.GlobalPosition;

            _velocity.X += 1;
            if (playerVariables.Player is Player player) player.CameraShakeAmount += 5;

            _animationPlayer.Play("intro");
            _damageArea.BodyEntered += (Node2D body) => _bodyEntered(body, p.Stats[EntityStatType.PhysicalAttack].Value, p.Stats[EntityStatType.MagicalAttack].Value);

            await ToSignal(GetTree().CreateTimer(Data.Duration), "timeout");
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
                _kinematicBody.MoveAndCollide(_velocity.Rotated(Rotation) * speed);
            }
        }
    }
}
