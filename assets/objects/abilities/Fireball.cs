using Galatime.Damage;
using Godot;

namespace Galatime
{
    public partial class Fireball : GalatimeAbility
    {
        private AnimationPlayer AnimationPlayer;
        private Projectile Projectile;
        private Timer DurationTimer;

        private float PhysicalAttack = 0;
        private float MagicalAttack = 0; 
    
        public override void _Ready()
        {
            AnimationPlayer = GetNode<AnimationPlayer>("Projectile/AnimationPlayer");
            Projectile = GetNode<Projectile>("Projectile");
            DurationTimer = GetNode<Timer>("DurationTimer");

            AnimationPlayer.AnimationFinished += AnimationFinished;

            DurationTimer.WaitTime = Data.Duration;
        }

        public void AnimationFinished(StringName name)
        {
            if (name == "intro") AnimationPlayer.Play("loop");
        }

        public override void Execute(HumanoidCharacter p)
        {
            PhysicalAttack = p.Stats[EntityStatType.PhysicalAttack].Value;
            MagicalAttack = p.Stats[EntityStatType.MagicalAttack].Value;
            
            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            Projectile.Rotation = p.Weapon.Rotation;
            Projectile.GlobalPosition = p.Weapon.GlobalPosition;
            Projectile.AttackStat = MagicalAttack;
            Projectile.Power = 20;
            Projectile.Accuracy = 0.01f;
            Projectile.PiercingTimes = 0;
            Projectile.Explosive = true;

            Projectile.Explosion.Power = 8;
            
            Projectile.Duration = Data.Duration;
            Projectile.Exploded += Destroy;

            if (playerVariables.Player is Player player) player.CameraShakeAmount += 10;

            AnimationPlayer.Play("intro");
            DurationTimer.Start();
        }

        public void Destroy(Projectile projectile = null)
        {
            AnimationPlayer.Play("outro");
        }
    }
}
