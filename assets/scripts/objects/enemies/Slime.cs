using Galatime;
using Galatime.Helpers;
using Godot;

public partial class Slime : Entity
{
    #region Nodes
    private NavigationAgent2D Navigation;
    private Sprite2D Sprite;

    /// <summary> Area for the character's weapon. </summary>
    private Area2D Weapon;
    private AnimationPlayer AnimationPlayer;

    /// <summary> Timer for countdown to attack. </summary>
    private Timer AttackCountdownTimer;
    private TargetController TargetController;

    private GpuParticles2D Particles;
    #endregion

    #region Variables
    /// <summary> Packed scene for slime enemies. </summary>
    private PackedScene SlimeScene;
    /// <summary> Character speed. </summary>
    #endregion

    public override void _Ready()
    {
        base._Ready();
        CanMove = false;

        Body = this;

        SlimeScene = ResourceLoader.Load<PackedScene>("res://assets/objects/enemy/Slime.tscn");

        Sprite = GetNode<Sprite2D>("Sprite2D");
        Navigation = GetNode<NavigationAgent2D>("Navigation");
        Particles = GetNode<GpuParticles2D>("Particles");
        TargetController = GetNode<TargetController>("TargetController");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Weapon = GetNode<Area2D>("Weapon");

        TargetController.TargetTeam = Teams.Allies;

        Weapon.BodyEntered += Attack;
        Weapon.BodyExited += OnAreaExit;

        AttackCountdownTimer = new Timer
        {
            WaitTime = 1f,
            OneShot = true
        };
        AttackCountdownTimer.Timeout += JustHit;
        AddChild(AttackCountdownTimer);
    }

    public override void _ExitTree()
    {
        Weapon.BodyEntered -= Attack;
        Weapon.BodyExited -= OnAreaExit;
    }

    public void Spawned()
    {
        CanMove = true;
        AnimationPlayer.Play("walk");
    }

    public override void _AIProcess(double delta)
    {
        if (!DeathState) Move(); else Body.Velocity = Vector2.Zero;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        base._DeathEvent();
        DropXp();
        AnimationPlayer.Play("outro");
    }

    public void Attack(Node2D body)
    {
        if (!DeathState && body is Entity entity) DealDamage(entity);
    }

    public void JustHit()
    {
        var bodies = Weapon.GetOverlappingBodies()[0] as Entity;
        if (bodies is Entity entity) DealDamage(entity);
    }

    private void DealDamage(Entity entity)
    {
        AttackCountdownTimer.Start();
        GalatimeElement element = GalatimeElement.Aqua;
        float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
        entity.TakeDamage(50, Stats[EntityStatType.PhysicalAttack].Value, element, DamageType.Physical, 500, damageRotation);

        AnimationPlayer.Play("hit");
    }

    public void SpawnParticles()
    {
        var particles = Particles.Duplicate() as GpuParticles2D;
        AddChild(particles);
        particles.TopLevel = true;
        particles.Emitting = true;
        particles.GlobalPosition = GlobalPosition;
    }

    public void OnAreaExit(Node2D body) => AttackCountdownTimer.Stop();

    public void Move()
    {
        var enemy = TargetController.CurrentTarget;
        if (enemy != null && CanMove)
        {
            Vector2 vectorPath = Vector2.Zero;
            Navigation.TargetPosition = enemy.GlobalPosition;
            vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition()) * Speed;
            float rotation = Body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
            Weapon.Rotation = rotation;
            float rotationDeg = Mathf.RadToDeg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            Sprite.FlipH = rotationDegPositive <= 90;
            Body.Velocity = vectorPath;
        }
        else Body.Velocity = Vector2.Zero;
    }
}