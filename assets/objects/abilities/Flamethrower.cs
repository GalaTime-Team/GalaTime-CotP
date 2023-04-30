using Godot;
using System;
using Galatime;
using System.Security.Cryptography;

namespace Galatime
{
    public partial class Flamethrower : GalatimeAbility
    {
        public Timer shotTimer;
        public PackedScene projectiveScene;

        public HumanoidCharacter p;

        public Sprite2D sprite;

        public Flamethrower() : base(
            GD.Load("res://sprites/gui/abilities/ignis/flamethrower.png") as Texture2D,
            5,
            2f,
            new System.Collections.Generic.Dictionary<string, float>() { { "mana", 10 } }
        )
        { }

        public override async void _Ready()
        {
            sprite = GetNode<Sprite2D>("Sprite2D");

            shotTimer = new Timer();
            shotTimer.WaitTime = 0.05f;
        }

        public override void _PhysicsProcess(double delta)
        {
            sprite.GlobalPosition = p.weapon.GlobalPosition;
            sprite.Rotation = p.weapon.Rotation;
        }

        public override async void execute(HumanoidCharacter p, float physicalAttack, float magicalAttack)
        {
            projectiveScene = GD.Load<PackedScene>("res://assets/objects/abilities/FlamethrowerShells.tscn");

            this.p = p;

            var rotation = p.weapon.Rotation;
            var position = p.weapon.GlobalPosition;

            var binds = new Godot.Collections.Array();
            binds.Add(physicalAttack);
            binds.Add(magicalAttack);
            binds.Add(sprite);

            shotTimer.Timeout += () => _onTimeoutShot(physicalAttack, magicalAttack, sprite); 
            AddChild(shotTimer);
            shotTimer.Start();

            await ToSignal(GetTree().CreateTimer(duration), "timeout");

            shotTimer.Stop();

            await ToSignal(GetTree().CreateTimer(2f), "timeout");
            QueueFree();
        }

        private void _onTimeoutShot(float physicalAttack, float magicalAttack, Sprite2D spr)
        {
            if (PlayerVariables.Player is Player player) player.cameraShakeAmount += 0.4f;
            FlamethrowerShells ability = projectiveScene.Instantiate<FlamethrowerShells>();
            var position = spr.GlobalPosition;
            AddChild(ability);
            ability.execute(spr.Rotation, physicalAttack, magicalAttack, position);
        }
    }
}
