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
        Element = GalatimeElement.Aqua;
        Stats = new EntityStats(
            physicalAttack: 21,
            magicalAttack: 25,
            physicalDefence: 10,
            magicalDefence: 34,
            health: 5
        );
        Health = Stats[EntityStatType.Health].Value;

        ShootingTimer.Timeout += OnShootingTimerTimeout;
        ShootingTimer.Start();
    }

    private void OnShootingTimerTimeout() {
        var projectile = Projectile.Duplicate() as Projectile;
        projectile.TargetTeam = Galatime.Helpers.Teams.allies;
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
