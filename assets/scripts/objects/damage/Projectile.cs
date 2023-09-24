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

    /// <summary> The target of the projectile to aim to. If this is null, the projectile will move by <see cref="Rotation"/>. </summary>
    public Node2D CurrentTarget;

    /// <summary> The the attack stat of the projectile. </summary>
    public float AttackStat = 0;

    /// <summary> The damage of the projectile to deal. </summary>
    [Export] public int Power = 10;

    /// <summary> How many times the projectile can be pierce through the entities. </summary>
    [Export] public int PiercingTimes = 0;

    /// <summary> The duration how long the projectile can be. </summary>
    public float Duration;

    public int CurrentPiercingTimes { get; private set; } = 0;

    /// <summary> The element of the projectile. </summary>
    public GalatimeElement Element = new();

    /// <summary> Target team to deal damage. Don't confuse with friendly fire, because this target to deal damage. Can be "allies" or "enemies". </summary>
    [Export] public Teams TargetTeam = Teams.enemies;

    public CharacterBody2D FriendlyBody { get; set; }

    /// <summary> 
    /// The parameter that affects how well the projectile aims at the target, from 0 to 1.
    /// A higher value means more accurate aiming, a lower value means more random deviation.
    /// </summary>
    [Export] public float Accuracy = 0.05f;

    /// <summary> If the projectile can explode. </summary>
    [Export] public bool Explosive = false;

    /// <summary> If the projectile is moving in the direction. </summary>
    [Export] public bool Moving = true;
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
    /// <summary> The timer for countdown to freed projectile from the scene when projectile is not needed. </summary>
    public Timer TimeoutTimer;
    /// <summary> The timer for duration how long the projectile can be. </summary>
    // public Timer DurationTimer;
    #endregion

    public override void _Ready()
    {   
        ExplosionScene = ResourceLoader.Load<PackedScene>("res://assets/objects/damage/Explosion.tscn");
        Explosion = ExplosionScene.Instantiate<Explosion>();

        #region Get nodes
        TargetController = GetNode<TargetController>("TargetController");
        DamageArea = GetNode<Area2D>("DamageArea");
        TimeoutTimer = GetNode<Timer>("TimeoutTimer");
        // DurationTimer = GetNode<Timer>("DurationTimer");
        #endregion

        // DurationTimer.Timeout += Destroy;
        
        TimeoutTimer.Timeout += QueueFree;
        DamageArea.BodyEntered += OnDamageAreaBodyEntered;

        // if (Duration > 0) DurationTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        TargetController.TargetTeam = TargetTeam;
        // If the target is not null, rotate the projectile towards it
        if (TargetController.CurrentTarget != null)
        {
            // Get the target's angle from 0 to 360 and adding 360 to move the projectile towards the target
            var targetAngle = GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition) + Mathf.DegToRad(360);
            // Applying accuracy to the angle with Lerp and rotate 
            Rotation = Mathf.LerpAngle(Rotation, targetAngle, Accuracy);
        }
        // Moving the projectile by rotation
        Velocity = Moving ? Vector2.Right.Rotated(Rotation) * Speed : Vector2.Zero;
        MoveAndSlide();
    }

    private void OnDamageAreaBodyEntered(Node node)
    {
        if (node is Entity entity && entity.IsInGroup(TargetController.GetTeamNameByEnum(TargetTeam)) && Moving) {
            var damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
            if (!Explosive) entity.TakeDamage(Power, AttackStat, Element, DamageType.magical, 500, damageRotation);

            // Pierce the entity if the projectile can.
            if (CurrentPiercingTimes >= PiercingTimes) {
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

    public void Destroy() {
        if (Explosive) {
            // Explosion.TopLevel = true; 
            // Explosion.GlobalPosition = GlobalPosition;
            CallDeferred("AddChildDefered", Explosion);
        }
        Moving = false;
        // DurationTimer.Stop();
        TimeoutTimer.Start();
        Exploded?.Invoke(this);
    }

    public void AddChildDefered(Node2D node) {
        AddChild(node);
    }
}
