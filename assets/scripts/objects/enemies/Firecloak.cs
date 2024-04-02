using Godot;

using System;

using Galatime;
using Galatime.AI;
using Galatime.Damage;
using Galatime.Global;
using Galatime.Helpers;

public partial class Firecloak : Entity
{
    #region Nodes
    /// <summary> Base Fireball, which similar to player's fireball. </summary>
    public BaseFireball BaseFireball;
    public Timer FireballSpawnTimer, StrafeTimer, DashPrepareTimer;
    public TargetController TargetController;
    public RangedHitTracker RangedHitTracker;
    public Navigator Navigator;
    public AttackSwitcher AttackSwitcher;
    public DamageArea DamageArea;
    public Sprite2D Sprite2D;
    public CollisionShape2D Collision;

    public AnimationPlayer AnimationPlayer;
    public DangerNotifierEffect DangerDashEffect;

    public TrailEffect TrailEffect;

    public Explosion DeathExplosion;
    #endregion

    #region Variables
    public VectorShaker DeathShake = new()
    {
        ShakeAmplitude = new Vector2(20, 20),
        Infinite = true
    };

    public readonly Vector2 DangerDashEffectSpawnPosition = new(0, -90);
    public bool StrafeDirection = false;

    // Fireball attack
    public int FireballSpawnCount = 2;
    public float FireballSpawnInterval = .1f;
    public float FireballSpreadOffset = .03f;
    public bool LeftToRight = true;
    public int CurrentFireballSpawnCount;

    // Dash attack
    public float DashPrepareTime = .5f;
    public float DashSpeed = 1000f;
    public bool IsDashing = false;
    public Vector2 EndDashPosition;

    #endregion

    public void RegisterAttackCycles()
    {
        AttackSwitcher.RegisterAttackCycles(new AttackCycle("fireball", FireballAttack, null, .75f));
        AttackSwitcher.RegisterAttackCycles(new AttackCycle("dash", DashAttack, null, .25f, () => GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition) > 350));
    }

    public override void _Ready()
    {
        base._Ready();

        #region Get nodes
        BaseFireball = GetNode<BaseFireball>("BaseFireball");
        TargetController = GetNode<TargetController>("TargetController");

        FireballSpawnTimer = GetNode<Timer>("FireballSpawnTimer");
        StrafeTimer = GetNode<Timer>("StrafeTimer");
        DashPrepareTimer = GetNode<Timer>("DashPrepareTimer");

        RangedHitTracker = GetNode<RangedHitTracker>("RangedHitTracker");
        Navigator = GetNode<Navigator>("Navigator");
        AttackSwitcher = GetNode<AttackSwitcher>("AttackSwitcher");
        DamageArea = GetNode<DamageArea>("DamageArea");

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        TrailEffect = GetNode<TrailEffect>("TrailEffect");
        Sprite2D = GetNode<Sprite2D>("Sprite2D");
        Collision = GetNode<CollisionShape2D>("Collision");
        #endregion

        FireballSpawnTimer.Timeout += FireballAttack;
        StrafeTimer.Timeout += ChangeStrafeDirection;
        DashPrepareTimer.Timeout += Dash;

        DeathExplosion = Explosion.GetInstance();
        DeathExplosion.Power = 14;
        DeathExplosion.Element = GalatimeElement.Ignis;
        DeathExplosion.Type = ExplosionType.Red;

        TargetController.OnTargetChanged += () =>
        {
            GD.Print("Changed target");
            Navigator.Target = TargetController.CurrentTarget;
        };

        RegisterAttackCycles();
        AttackSwitcher.NextCycle();
    }

    #region Attack cycles
    public void FireballAttack()
    {
        if (GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition) < 100 && !DeathState) // Don't launch fireball if we are too close to the target.
        {
            AttackSwitcher.NextCycle();
            return;
        }
        CurrentFireballSpawnCount++;

        // Check if we can spawn another fireball, or if we can end the attack.
        if (CurrentFireballSpawnCount > FireballSpawnCount || !RangedHitTracker.CanHit) // We can't start the attack if not able to see the target.
        {
            CurrentFireballSpawnCount = 0;
            AttackSwitcher.NextCycle();
            LeftToRight = !LeftToRight; // Change direction of the spread of fireballs.
            return;
        }

        var a = CurrentFireballSpawnCount * FireballSpreadOffset * 2;
        var b = FireballSpawnCount * FireballSpreadOffset - FireballSpreadOffset;

        var offset = LeftToRight ? (a - b) : (b - a); // Spread of fireballs.
        SpawnFireball(offset);
        FireballSpawnTimer.Start(FireballSpawnInterval);
    }

    public void DashAttack()
    {
        if (DeathState) return;

        // Spawn danger effect to notify the player that enemy is about to dash.
        DangerDashEffect = DangerNotifierEffect.GetInstance();
        DangerDashEffect.Position = DangerDashEffectSpawnPosition;
        AddChild(DangerDashEffect);
        DangerDashEffect.Start();

        AnimationPlayer.Play("dash_intro");

        DashPrepareTimer.Start(DashPrepareTime);
    }
    #endregion

    #region Attacks
    public void SpawnFireball(float rotationOffset = 0)
    {
        if (DeathState) return;
        var magicalAttack = Stats[EntityStatType.MagicalAttack].Value;
        var prj = BaseFireball.Duplicate() as BaseFireball;
        prj.GlobalPosition = GlobalPosition;
        GetParent().AddChild(prj);
        prj.Launch(TargetController.CurrentTarget.GlobalPosition.AngleToPoint(GlobalPosition) + Mathf.Pi + rotationOffset, magicalAttack);
    }

    public void Dash()
    {
        DangerDashEffect.End();

        if (DeathState) return;

        TrailEffect.Enabled = true;
        EndDashPosition = TargetController.CurrentTarget.GlobalPosition;
        IsDashing = true;

        AnimationPlayer.Play("dash_loop");
    }

    public void EndDash()
    {
        TrailEffect.Enabled = false;
        IsDashing = false;
        DamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
        DamageArea.HitOneTime();

        if (DeathState) return;

        AttackSwitcher.NextCycle();
        AnimationPlayer.Play("dash_outro");
    }
    #endregion

    public override void _DeathEvent(float damageRotation = 0)
    {
        FireballSpawnTimer.Stop();
        StrafeTimer.Stop();
        DashPrepareTimer.Stop();

        DeathShake.ShakeStart(Sprite2D.Position);
        AnimationPlayer.Play("death");
        Callable.From(() => Collision.Disabled = true).CallDeferred();

        PlayerVariables.Instance.DiscoverEnemy(3);
    }

    public void Explode()
    {
        Sprite2D.Visible = false;
        DeathShake.ShakeStop();
        DropXp();

        AddChild(DeathExplosion);
    }

    public override void _MoveProcess(double delta)
    {
        AttackSwitcher.Enabled = !DisableAI;
        if (DeathState) AttackSwitcher.Enabled = false;

        Velocity = Vector2.Zero;
        if (DeathState)
        {
            DeathShake.ShakeProcess(delta);
            Sprite2D.Position = DeathShake.ShakenVector;
        }
        if (IsDashing)
        {
            var angleToEnd = EndDashPosition.AngleToPoint(GlobalPosition);
            Velocity += Vector2.Left.Rotated(angleToEnd).Normalized() * DashSpeed;
            if (GlobalPosition.DistanceTo(EndDashPosition) < 50 || IsOnWall()) EndDash();
            MoveAndSlide();
        }
    }

    public override void _AIProcess(double delta)
    {
        Velocity = Vector2.Zero;

        if (TargetController.CurrentTarget != null)
        {
            if (RangedHitTracker.CanHit && !DeathState)
            {
                var target = TargetController.CurrentTarget;
                var angleTo = target.GlobalPosition.AngleToPoint(GlobalPosition);

                DamageArea.Rotation = angleTo + Mathf.Pi;

                if (!AttackSwitcher.IsAttackCycleActive("dash")) // We should do anything if dash is active, because is preparing to dash.
                {
                    Velocity += new Vector2(0, StrafeDirection ? -1 : 1).Rotated(angleTo);
                    if (GlobalPosition.DistanceTo(target.GlobalPosition) > 350)
                        Velocity += Vector2.Left.Rotated(angleTo);  
                    else if (GlobalPosition.DistanceTo(target.GlobalPosition) < 250)
                        Velocity += Vector2.Right.Rotated(angleTo);
                    Velocity = Velocity.Normalized() * Speed;
                }
            }
            else if (!DeathState)
            {
                Navigator.Speed = Speed;
                Velocity = Navigator.NavigatorVelocity;
            }
            else 
            {
                if (IsDashing) EndDash(); // We don't need to dash if we can't hit.
            }
        }
    }

    public void ChangeStrafeDirection()
    {
        var rnd = new Random();
        StrafeDirection = rnd.Next(2) == 0;
    }
}
