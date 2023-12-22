using Galatime;
using Galatime.Interfaces;
using Godot;
using System;

public partial class GoldenHolderSword : Area2D, IWeapon
{
    private HumanoidCharacter CurrentHumanoidCharacter;
    private AnimationPlayer AnimationPlayer;
    private bool CanAttack = true;

    public float Power { get; set; } = 1f;
    public float Cooldown { get; set; } = 0.2f;

    private float PhysicalAttack;

    public override void _Ready()
    {
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Attack(HumanoidCharacter p)
    {
        CurrentHumanoidCharacter = p;

        PhysicalAttack = p.Stats[EntityStatType.PhysicalAttack].Value;
        if (CanAttack)
        {
            p.State = HumanoidStates.Attack;
            p.CanMove = false;

            p.Stamina.Value -= p.Stats[EntityStatType.Stamina].Value * 0.03f;

            // Play animation.
            AnimationPlayer.Stop();
            AnimationPlayer.Play("swing");

            // Delay before attacking again.
            CanAttack = false;

            // Create a delay between attacks.
            GetTree().CreateTimer(Cooldown).Timeout += () => {
                p.State = HumanoidStates.Idle;
                p.CanMove = true;

                CanAttack = true;
            };
        }
    }

    public void DealDamage() {
        var bodies = GetOverlappingBodies();
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] is Projectile projectile && projectile.TargetTeam == Galatime.Helpers.Teams.Allies && projectile.Moving) Parry(projectile);
            if (bodies[i] is Entity entity)
            {
                GalatimeElement element = new();
                float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
                entity.TakeDamage(10, PhysicalAttack, element, DamageType.Physical, 500, damageRotation);
            }
        }
    }
    
    public void Parry(Projectile projectile) {
        projectile.Rotation = GlobalRotation;
        projectile.Accuracy = 0f;
        projectile.Speed *= 3f;
        projectile.Power = (int)(projectile.Power * 2f);
        projectile.TargetTeam = Galatime.Helpers.Teams.Enemies;
        projectile.Explosive = true;
    }

    public void StepForwardAnimation(float step) => CurrentHumanoidCharacter?.SetKnockback(CurrentHumanoidCharacter.Stamina.Value <= 0 ? 0 : step, CurrentHumanoidCharacter.Weapon.Rotation);
}
