using Godot;

namespace Galatime
{
    public partial class Flamethrower : GalatimeAbility
    {
        public Timer shotTimer;
        public PackedScene projectiveScene;

        public HumanoidCharacter p;

        public Sprite2D sprite;
        public GpuParticles2D Particles;

        public override void _Ready()
        {
            // Particles = GetNode<GpuParticles2D>("Particles");
            sprite = GetNode<Sprite2D>("Sprite2D");

            shotTimer = new Timer
            {
                WaitTime = 0.015f
            };
        }

        public override void _PhysicsProcess(double delta)
        {
            sprite.GlobalPosition = p.Weapon.GlobalPosition;
            sprite.Rotation = p.Weapon.Rotation;

            // Particles.GlobalPosition = p.Weapon.GlobalPosition;;
            // Particles.Rotation = p.Weapon.Rotation;
        }

        public override async void Execute(HumanoidCharacter p)
        {
            var physicalAttack = p.Stats[EntityStatType.PhysicalAttack].Value;
            var magicalAttack = p.Stats[EntityStatType.MagicalAttack].Value;

            projectiveScene = GD.Load<PackedScene>("res://assets/objects/abilities/FlamethrowerShells.tscn");

            this.p = p;

            var rotation = p.Weapon.Rotation;
            var position = p.Weapon.GlobalPosition;

            shotTimer.Timeout += () => _onTimeoutShot(physicalAttack, magicalAttack, sprite);
            AddChild(shotTimer);
            shotTimer.Start();

            // Particles.Emitting = true;

            await ToSignal(GetTree().CreateTimer(Data.Duration), "timeout");

            shotTimer.Stop();
            // Particles.Emitting = false;

            await ToSignal(GetTree().CreateTimer(2f), "timeout");
            QueueFree();
        }

        private void _onTimeoutShot(float physicalAttack, float magicalAttack, Sprite2D spr)
        {
            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            if (playerVariables.Player is Player player) player.CameraShakeAmount += 0.2f;
            FlamethrowerShells ability = projectiveScene.Instantiate<FlamethrowerShells>();
            var position = spr.GlobalPosition;
            AddChild(ability);
            ability.execute(spr.Rotation, physicalAttack, magicalAttack, position);
        }
    }
}
