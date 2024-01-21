using Galatime;
using Galatime.Damage;
using Galatime.Helpers;
using Godot;
using System;
using System.Collections.Generic;

public partial class Firecloak : Entity
{
    #region Nodes
    /// <summary> Base Fireball, which similar to player's fireball. </summary>
    public BaseFireball BaseFireball;
    public Timer FireballSpawnTimer, AttackCycleTimer, StrafeTimer, DashPrepareTimer;
    public TargetController TargetController;
    public RangedHitTracker RangedHitTracker;
    public NavigationAgent2D Navigation;
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

    public enum FirecloakAttackCycle
    {
        /// <summary> Shoots 3 fireballs in a row. </summary>
        Fireball,
        /// <summary> Dashes towards the target and deals melee damage. </summary>
        Dash
    }

    public FirecloakAttackCycle CurrentAttackCycle;
    public float DelayBeforeNextCycle = .5f;

    public Dictionary<FirecloakAttackCycle, (Action callback, float chance)> AttackCycles = new();

    // Fireball attack
    public int FireballSpawnCount = 2;
    public float FireballSpawnInterval = .1f;
    public float FireballSpreadOffset = .03f;
    public bool LeftToRight = true;
    public int CurrentFireballSpawnCount;

    // Dash attack
    public float DashPrepareTime = .5f;
    public float DashSpeed = 400f;
    public bool IsDashing = false;
    public Vector2 EndDashPosition;

    #endregion

    public Firecloak()
    {
        AttackCycles.Add(FirecloakAttackCycle.Fireball, (FireballAttack, .75f));
        AttackCycles.Add(FirecloakAttackCycle.Dash, (DashAttack, .25f));
    }

    public override void _Ready()
    {
        base._Ready();

        #region Get nodes
        BaseFireball = GetNode<BaseFireball>("BaseFireball");
        TargetController = GetNode<TargetController>("TargetController");

        FireballSpawnTimer = GetNode<Timer>("FireballSpawnTimer");
        AttackCycleTimer = GetNode<Timer>("AttackCycleTimer");
        StrafeTimer = GetNode<Timer>("StrafeTimer");
        DashPrepareTimer = GetNode<Timer>("DashPrepareTimer");

        RangedHitTracker = GetNode<RangedHitTracker>("RangedHitTracker");
        Navigation = GetNode<NavigationAgent2D>("Navigation");
        DamageArea = GetNode<DamageArea>("DamageArea");

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        TrailEffect = GetNode<TrailEffect>("TrailEffect");
        Sprite2D = GetNode<Sprite2D>("Sprite2D");
        Collision = GetNode<CollisionShape2D>("Collision");
        #endregion

        FireballSpawnTimer.Timeout += FireballAttack;
        AttackCycleTimer.Timeout += StartAttack;
        StrafeTimer.Timeout += ChangeStrafeDirection;
        DashPrepareTimer.Timeout += Dash;

        DeathExplosion = Explosion.GetInstance();
        DeathExplosion.Power = 14;
        DeathExplosion.Element = GalatimeElement.Ignis;
        DeathExplosion.Type = ExplosionType.Red;

        NextCycle();
    }

    #region Attack cycles
    public void FireballAttack()
    {
        if (RangedHitTracker.GetCollisionPoint().DistanceTo(GlobalPosition) < 100) // Don't launch fireball if we are too close to the target.
        {
            NextCycle();
            return;
        }
        CurrentFireballSpawnCount++;

        // Check if we can spawn another fireball, or if we can end the attack.
        if (CurrentFireballSpawnCount > FireballSpawnCount || !RangedHitTracker.CanHit) // We can't start the attack if not able to see the target.
        {
            CurrentFireballSpawnCount = 0;
            NextCycle();
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

    #region Attack cycles logic
    public void StartAttack()
    {
        if (DeathState) return;
        if (!RangedHitTracker.CanHit)
        {
            NextCycle();
            return;
        }

        if (GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition) > 400) // Force dash if target is too far.
        {
            CurrentAttackCycle = FirecloakAttackCycle.Dash;
            AttackCycles[CurrentAttackCycle].callback();
            return;
        }

        // Select attack based on chances
        var rnd = new Random();
        float roll = (float)rnd.NextDouble();
        float cumulative = 0.0f;

        foreach (var attack in AttackCycles)
        {
            cumulative += attack.Value.chance;
            if (roll < cumulative)
            {
                CurrentAttackCycle = attack.Key;
                break;
            }
        }

        AttackCycles[CurrentAttackCycle].callback();
    }

    public void NextCycle(bool noCooldown = false)
    {
        AttackCycleTimer.Start(noCooldown ? 0.01f : DelayBeforeNextCycle);
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

        PlayerVariables.Instance.Player.CameraShakeAmount += 10;
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

        NextCycle(true);
        AnimationPlayer.Play("dash_outro");
    }
    #endregion

    public override void _DeathEvent(float damageRotation = 0)
    {
        DeathShake.ShakeStart(Sprite2D.Position);
        AnimationPlayer.Play("death");
        Callable.From(() => Collision.Disabled = true).CallDeferred();

        base._DeathEvent(damageRotation);
    }

    public void Explode()
    {
        Sprite2D.Visible = false;
        DeathShake.ShakeStop();
        DropXp();

        AddChild(DeathExplosion);
    }

    public override void _AIProcess(double delta)
    {
        base._AIProcess(delta);

        if (DeathState)
        {
            DeathShake.ShakeProcess(delta);
            Sprite2D.Position = DeathShake.ShakenVector;
        }

        Velocity = Vector2.Zero;
        if (TargetController.CurrentTarget != null)
        {
            if (RangedHitTracker.CanHit && !DeathState)
            {
                var target = TargetController.CurrentTarget;
                var angleTo = target.GlobalPosition.AngleToPoint(GlobalPosition);

                DamageArea.Rotation = angleTo + Mathf.Pi;

                if (CurrentAttackCycle != FirecloakAttackCycle.Dash) // We should do anything if dash is active, because is preparing to dash.
                {
                    Velocity += new Vector2(0, StrafeDirection ? -1 : 1);
                    if (GlobalPosition.DistanceTo(target.GlobalPosition) > 350)
                        Velocity += Vector2.Left.Rotated(angleTo);
                    else if (GlobalPosition.DistanceTo(target.GlobalPosition) < 250)
                        Velocity += Vector2.Right.Rotated(angleTo);
                    Velocity = Velocity.Normalized() * Speed;
                }
                if (IsDashing) // Dash behavior.
                {
                    var angleToEnd = EndDashPosition.AngleToPoint(GlobalPosition);
                    Velocity += Vector2.Left.Rotated(angleToEnd);
                    Velocity = Velocity.Normalized() * DashSpeed;
                    if (GlobalPosition.DistanceTo(EndDashPosition) < 50 || IsOnWall()) EndDash();
                }
            }
            else if (!DeathState)
            {
                // TODO: Move navigation behavior to another class.
                // Move to target if we can't hit.
                Vector2 vectorPath;
                // Set target position to the next enemy.
                Navigation.TargetPosition = TargetController.CurrentTarget.GlobalPosition;
                // Vector from the target.
                var pathRotation = Body.GlobalPosition.AngleToPoint(Navigation.GetNextPathPosition());
                vectorPath = Vector2.Right.Rotated(pathRotation);
                Velocity = vectorPath.Normalized() * Speed;
                // TODO: Move navigation behavior to another class.
            }
            else
            {
                if (IsDashing) EndDash(); // We don't need to dash if we can't hit.
            }
        }
        MoveAndSlide();
    }

    public void ChangeStrafeDirection()
    {
        var rnd = new Random();
        StrafeDirection = rnd.Next(2) == 0;
    }
}
