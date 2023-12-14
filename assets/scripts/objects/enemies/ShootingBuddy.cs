using Galatime;
using Galatime.Helpers;
using Godot;

public partial class ShootingBuddy : Entity
{
    #region Nodes
    public Timer ShootingTimer;
    public Projectile Projectile;
    public Sprite2D Sprite;
    public TargetController TargetController;
    public CollisionShape2D Collision;
    #endregion


    public override void _Ready()
    {
        base._Ready();

        #region Get nodes
        ShootingTimer = GetNode<Timer>("ShootingTimer");
        Projectile = GetNode<Projectile>("Projectile");
        Sprite = GetNode<Sprite2D>("Sprite2D");
        TargetController = GetNode<TargetController>("TargetController");
        Collision = GetNode<CollisionShape2D>("Collision");
        #endregion

        Body = this;

        ShootingTimer.Timeout += OnShootingTimerTimeout;
        ShootingTimer.Start();
    }

    private void OnShootingTimerTimeout()
    {
        // Don't shoot if no target.
        if (TargetController.CurrentTarget == null) return; 

        var projectile = Projectile.Duplicate() as Projectile;
        projectile.AttackStat = Stats[EntityStatType.MagicalAttack].Value;
        projectile.Visible = true;
        projectile.Moving = true;
        projectile.Explosive = true;
        projectile.Exploded += OnProjectileExploded;
        projectile.TopLevel = true;
        
        projectile.GlobalPosition = GlobalPosition;
        projectile.Rotation = GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition);

        AddChild(projectile);
        projectile.Explosion.Element = Element;
        projectile.TimeoutTimer.WaitTime = 10f;
    }

    private void OnProjectileExploded(Projectile projectile = null)
    {
        projectile.GetNode<Sprite2D>("Sprite").Visible = false;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        base._DeathEvent(damageRotation);
        Sprite.Visible = false;
        ShootingTimer.Stop();
        Callable.From(() => Collision.Disabled = true).CallDeferred();
    }
}
