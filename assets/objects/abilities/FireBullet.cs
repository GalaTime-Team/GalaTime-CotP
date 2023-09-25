using Galatime;
using Galatime.Damage;
using Godot;
using System;

public partial class FireBullet : GalatimeAbility
{
    #region Nodes
    public AnimationPlayer AnimationPlayer;
    public AnimatedSprite2D Projectile;
    public Area2D DamageArea;
    public HumanoidCharacter Caster;
    public RayCast2D RayCast;

    public PackedScene ExplosionScene;
    public Explosion Explosion;

    #endregion

    public bool Shotted = false;
    public PlayerVariables PlayerVariables;

    public override void _Ready()
    {
        #region Get nodes
        DamageArea = GetNode<Area2D>("Projectile/DamageArea");
        Projectile = GetNode<AnimatedSprite2D>("Projectile");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        RayCast = GetNode<RayCast2D>("Projectile/RayCast");
        #endregion

        ExplosionScene = ResourceLoader.Load<PackedScene>("res://assets/objects/damage/Explosion.tscn");
        Explosion = ExplosionScene.Instantiate<Explosion>();
        Explosion.Power = 8;

        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
    }

    public override void _Process(double delta)
    {
        if (!Shotted)
        {
            PlayerVariables.Player.CameraShakeAmount += 0.3f;

            Projectile.GlobalPosition = Caster.Weapon.GlobalPosition;
            Projectile.Rotation = Caster.Weapon.Rotation;
        }
    }

    public override void Execute(HumanoidCharacter p)
    {
        Caster = p;
        AnimationPlayer.Play("shoot");
    }

    public void DealDamage()
    {
        Shotted = true;

        var bodies = DamageArea.GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is Entity entity)
                entity.TakeDamage(40, Caster.Stats[EntityStatType.MagicalAttack].Value, Data.Element, DamageType.magical);
            if (body is Projectile projectile && projectile.Moving)
            {
                PlayerVariables.Player.PlayerGui.ParryEffect();
                var explosion = projectile.Explosion;
                explosion.Type = ExplosionType.Red;
                explosion.Power += 6;
                projectile.Destroy();
                return;
            }
        }
        if (RayCast.IsColliding())
        {

            var position = RayCast.GetCollisionPoint(); // Get the position of the collision point in global coordinates.
            GD.Print(RayCast.GlobalPosition.DistanceTo(position) / 4);
            Explosion.GlobalPosition = position;
            AddChild(Explosion);
        }
    }
}
