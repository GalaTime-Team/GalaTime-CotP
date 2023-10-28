using Galatime;
using Godot;
using System;

public partial class ShootingBuddy : Entity
{
    #region Nodes
    public Timer ShootingTimer;
    public Projectile Projectile;
    public Sprite2D Sprite;
    #endregion

    public override void _Ready()
    {
        base._Ready();  

        #region Get nodes
        ShootingTimer = GetNode<Timer>("ShootingTimer");
        Projectile = GetNode<Projectile>("Projectile");
        Sprite = GetNode<Sprite2D>("Sprite2D");
        #endregion

        Body = this;

        ShootingTimer.Timeout += OnShootingTimerTimeout;
        ShootingTimer.Start();
    }

    public ShootingBuddy() : base(new(
        PhysicalAttack: 20,
        PhysicalDefense: 20,
        MagicalDefense: 20,
        Health: 20
    ), GalatimeElement.Aqua) {}

    private void OnShootingTimerTimeout() {
        var projectile = Projectile.Duplicate() as Projectile;
        projectile.TargetTeam = Galatime.Helpers.Teams.Allies;
        projectile.AttackStat = Stats[EntityStatType.MagicalAttack].Value;
        projectile.Power = 20;
        projectile.Visible = true;
        projectile.Moving = true;
        projectile.Exploded += OnProjectileExploded;
        AddChild(projectile);
    }

    private void OnProjectileExploded(Projectile projectile = null) {
        projectile.GetNode<Sprite2D>("Sprite").Visible = false;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        base._DeathEvent(damageRotation);
        Sprite.Visible = false;
        ShootingTimer.Stop();
        // QueueFree();
    }
}
