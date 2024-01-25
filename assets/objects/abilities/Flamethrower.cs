using System;
using System.Collections.Generic;
using Godot;

namespace Galatime
{
    public partial class Flamethrower : GalatimeAbility
    {
        public Timer ShotTimer;
        public Projectile Projectile;
        public HumanoidCharacter p;

        public override void _Ready()
        {
            Projectile = GetNode<Projectile>("Projectile");
            ShotTimer = new Timer
            {
                WaitTime = 0.015f
            };
        }

        public override void Execute(HumanoidCharacter p)
        {
            this.p = p;

            var magicalAttack = p.Stats[EntityStatType.MagicalAttack].Value;

            ShotTimer.Timeout += () => Shot(magicalAttack);
            AddChild(ShotTimer);
            ShotTimer.Start();

            GetTree().CreateTimer(Data.Duration).Timeout += () =>
            {
                ShotTimer.Stop();
                GetTree().CreateTimer(10).Timeout += () => QueueFree();
            };
        }

        private void Shot(float magicalAttack)
        {
            PlayerVariables.Instance.Player.CameraShakeAmount += 0.15f;

            var rnd = new Random();
            var angle = (rnd.NextDouble() * 2 - 1) / 5;
            
            var prj = Projectile.Duplicate() as Projectile;
            prj.GlobalPosition = p.Weapon.GlobalPosition;
            prj.Rotation = p.Weapon.Rotation + (float)angle;
            prj.AttackStat = magicalAttack;
            prj.Visible = true;

            prj.Exploded += DestroyProjectile;

            AddChild(prj);
            prj.Moving = true;
        }

        private void DestroyProjectile(Projectile prj)
        {
            var ap = prj.GetNode<AnimationPlayer>("AnimationPlayer");
            ap.Play("outro");
        }
    }
}
