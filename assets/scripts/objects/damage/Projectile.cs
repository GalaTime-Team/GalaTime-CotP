using System;
using Galatime;
using Galatime.Damage;
using Galatime.Helpers;
using Godot;

// Node that represents a projectile. It can has a target to aim at, speed, damage, element and so on.
public partial class Projectile : CharacterBody2D

{
    #region Scenes
    public PackedScene ExplosionScene;
    #endregion

    #region Variables
    /// <summary> The speed of the projectile (how fast it moves). </summary>
    [Export] public float Speed = 400f;

    /// <summary> The the attack stat of the projectile. </summary>
    public float AttackStat = 0;

    /// <summary> The damage of the projectile to deal. </summary>
    [Export] public int Power = 10;

    /// <summary> The element of the projectile. </summary>
    [Export] public GalatimeElement Element;

    /// <summary> How many times the projectile can be pierce through the entities. </summary>
    [Export] public int PiercingTimes = 0;

    /// <summary> The duration how long the projectile can be. 0 means infinite duration. </summary>
    [Export] public float Duration;

    public int CurrentPiercingTimes { get; private set; } = 0;

    /// <summary> Target team to deal damage. Don't confuse with friendly fire, because this target to deal damage. Can be "allies" or "enemies". </summary>
    [Export] public Teams TargetTeam = Teams.Enemies;

    /// <summary> 
    /// The parameter that affects how well the projectile aims at the target, from 0 to 1.
    /// A higher value means more accurate aiming, a lower value means more random deviation.
    /// </summary>
    [Export] public float Accuracy = 0.05f;

    /// <summary> If the projectile can explode. </summary>
    [Export] public bool Explosive = false;

    [Export] public int ExplosionPower;

    /// <summary> If the projectile is moving in the direction. </summary>
    [Export] public bool Moving = true;

    /// <summary> The timeout in seconds, after which the projectile will be freed from the scene. </summary>
    [Export] public float Timeout = 10f;
    #endregion

    #region Events
    public event Action<Projectile> Exploded;
    #endregion

    #region Nodes
    /// <summary> The target controller. </summary> 
    public TargetController TargetController;
    /// <summary> The explosion node. Should to <see cref="Explosive"/> be true to work. </summary>
    public Explosion Explosion;
    /// <summary> The damage area where the projectile will deal damage. </summary>
    public Area2D DamageArea;
    public Timer TimeoutTimer, DurationTimer;
    /// <summary> The timer for duration how long the projectile can be. </summary>
    // public Timer DurationTimer;
    public CollisionShape2D Collision;
    #endregion

    public override void _Ready()
    {
        ExplosionScene = ResourceLoader.Load<PackedScene>("res://assets/objects/damage/Explosion.tscn").Duplicate() as PackedScene;
        Explosion = ExplosionScene.Instantiate<Explosion>().Duplicate() as Explosion;

        #region Get nodes
        TargetController = GetNode<TargetController>("TargetController");
        DamageArea = GetNode<Area2D>("DamageArea");
        TimeoutTimer = GetNode<Timer>("TimeoutTimer");
        DurationTimer = GetNode<Timer>("DurationTimer");
        Collision = GetNode<CollisionShape2D>("Collision");
        #endregion

        // If duration is not 0, start the timer. 0 means infinite duration.
        if (Duration > 0)
        {
            DurationTimer.WaitTime = Duration;
            DurationTimer.Start();
        }

        TimeoutTimer.Timeout += OnTimeout;
        DurationTimer.Timeout += Destroy;

        DamageArea.BodyEntered += OnDamageAreaBodyEntered;
    }

    public override void _Process(double delta)
    {
        Explosion.Power = ExplosionPower;
    }

    public void OnTimeout()
    {
        if (Timeout > 0) QueueFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        TargetController.TargetTeam = TargetTeam;
        // If the target is not null, rotate the projectile towards it
        if (TargetController.CurrentTarget != null && Moving)
        {
            // Applying accuracy to the angle with Lerp and rotate
            Rotation = Mathf.LerpAngle(Rotation, TargetAngle, Accuracy);
        }
        // Moving the projectile by rotation
        Velocity = Moving ? Vector2.Right.Rotated(Rotation) * Speed : Vector2.Zero;
        MoveAndSlide();
    }

    public float TargetAngle
    {
        get
        {
            if (TargetController.CurrentTarget != null) return GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition) + Mathf.DegToRad(360);
            return 0;
        }

    }

    private void OnDamageAreaBodyEntered(Node node)
    {
        if (node is Entity entity && !entity.DeathState && entity.IsInGroup(TargetController.GetTeamNameByEnum(TargetTeam)) && Moving)
        {
            var damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
            if (!Explosive) entity.TakeDamage(Power, AttackStat, Element, DamageType.Magical, 500, damageRotation);

            // Pierce the entity if the projectile can.
            if (CurrentPiercingTimes >= PiercingTimes)
            {
                Destroy();
                return;
            }

            CurrentPiercingTimes++;
        }
        else
        {
            if (node is Entity entity1 && !entity1.IsInGroup(TargetController.GetTeamNameByEnum(TargetTeam))) return;
            // Destroy the projectile if it collides with an solid environment.
            Destroy();
        }
    }

    public void Destroy()
    {
        if (!Moving) return;

        // Explosion behavior
        if (Explosive)
        {
            Callable.From(() =>
            {
                Explosion.Element = Element;
                AddChild(Explosion);
            }).CallDeferred();
        }

        DurationTimer.Stop();
        Moving = false;

        TimeoutTimer.WaitTime = Timeout;
        TimeoutTimer.Start();

        Exploded?.Invoke(this);

        // For some reason, we need to wait for the explosion to finish. This prevent the projectile from colliding with the other objects.
        GetTree().CreateTimer(0.5f).Timeout += () => Collision.Disabled = true;
    }
}
