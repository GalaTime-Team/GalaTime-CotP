using Galatime;
using Godot;
using System;

public partial class FireBullet : GalatimeAbility
{
    #region Nodes
    public AnimationPlayer AnimationPlayer;
    public AnimatedSprite2D Projectile;
    public Area2D DamageArea;
    public HumanoidCharacter Caster;
    #endregion
    
    public bool Shooted = false;
    public PlayerVariables PlayerVariables;

    public override void _Ready() {
        #region Get nodes
        DamageArea = GetNode<Area2D>("Projectile/DamageArea");
        Projectile = GetNode<AnimatedSprite2D>("Projectile");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        #endregion

        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
    }

    public override void _Process(double delta) {
        if (!Shooted) {
            PlayerVariables.Player.CameraShakeAmount += 0.3f;

            Projectile.GlobalPosition = Caster.Weapon.GlobalPosition;
            Projectile.Rotation = Caster.Weapon.Rotation;
        }
    }

    public override void Execute(HumanoidCharacter p) {
        Caster = p;
        AnimationPlayer.Play("shoot");
    }

    public void DealDamage() {
        Shooted = true;
        var bodies = DamageArea.GetOverlappingBodies();
        foreach (var body in bodies) {
            if (body is Entity entity) 
                entity.TakeDamage(40, Caster.Stats[EntityStatType.MagicalAttack].Value, Data.Element, DamageType.magical);
            if (body is Projectile projectile && projectile.Moving) {
                PlayerVariables.Player.PlayerGui.ParryEffect();
                var explosion = projectile.Explosion;
                explosion.Type = Galatime.Damage.ExplosionType.Red;
                explosion.Power += 6;
                projectile.Destroy();
            }
        }
    }
}
