using Godot;

namespace Galatime
{
    public partial class Flamethrower : GalatimeAbility
    {
        public Timer shotTimer;
        public PackedScene projectiveScene;

        public HumanoidCharacter p;

        public Sprite2D sprite;

        public override void _Ready()
        {
            sprite = GetNode<Sprite2D>("Sprite2D");

            shotTimer = new Timer();
            shotTimer.WaitTime = 0.05f;
        }

        public override void _PhysicsProcess(double delta)
        {
            sprite.GlobalPosition = p.Weapon.GlobalPosition;
            sprite.Rotation = p.Weapon.Rotation;
        }

        public override async void Execute(HumanoidCharacter p)
        {
            var physicalAttack = p.Stats[EntityStatType.PhysicalAttack].Value;
            var magicalAttack = p.Stats[EntityStatType.MagicalAttack].Value;

            projectiveScene = GD.Load<PackedScene>("res://assets/objects/abilities/FlamethrowerShells.tscn");

            this.p = p;

            var rotation = p.Weapon.Rotation;
            var position = p.Weapon.GlobalPosition;

            var binds = new Godot.Collections.Array();
            binds.Add(physicalAttack);
            binds.Add(magicalAttack);
            binds.Add(sprite);

            shotTimer.Timeout += () => _onTimeoutShot(physicalAttack, magicalAttack, sprite);
            AddChild(shotTimer);
            shotTimer.Start();

            await ToSignal(GetTree().CreateTimer(Data.Duration), "timeout");

            shotTimer.Stop();

            await ToSignal(GetTree().CreateTimer(2f), "timeout");
            QueueFree();
        }

        private void _onTimeoutShot(float physicalAttack, float magicalAttack, Sprite2D spr)
        {
            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            if (playerVariables.Player is Player player) player.CameraShakeAmount += 0.4f;
            FlamethrowerShells ability = projectiveScene.Instantiate<FlamethrowerShells>();
            var position = spr.GlobalPosition;
            AddChild(ability);
            ability.execute(spr.Rotation, physicalAttack, magicalAttack, position);
        }
    }
}
