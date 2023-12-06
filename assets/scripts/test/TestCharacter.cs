using Galatime;
using Galatime.Helpers;

using Godot;
using System;

public partial class TestCharacter : HumanoidCharacter
{
    [Export] public int FollowOrder;

    public NavigationAgent2D Navigation;
    public RayCast2D RayCast;
    public AnimationPlayer AnimationPlayer;

    public TargetController TargetController;

    public Timer RetreatDelayTimer;
    public Timer MoveDelayTimer;
    public Timer EnemySwitchDelayTimer;

    public Player Player;

    // public TestCharacter() : base(new EntityStats(new()
    // {
    //     [EntityStatType.Health] = 100,
    //     [EntityStatType.Mana] = 100,
    //     [EntityStatType.Stamina] = 100,
    //     [EntityStatType.PhysicalAttack] = 100,
    //     [EntityStatType.PhysicalDefense] = 100,
    //     [EntityStatType.MagicalAttack] = 100,
    //     [EntityStatType.MagicalDefense] = 100,
    //     [EntityStatType.Agility] = 100,
    // }))
    // { }


    public override void _Ready()
    {
        base._Ready();

        Weapon = GetNode<Hand>("Hand");
        HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");
        TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
        Body = this;

        AnimationPlayer = GetNode<AnimationPlayer>("Animation");
        TargetController = GetNode<TargetController>("TargetController");
        Navigation = GetNode<NavigationAgent2D>("Navigation");
        RayCast = GetNode<RayCast2D>("RayCast");

        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        Player = playerVariables.Player;

        Stamina += 99999;
        Mana += 99999;

        InitializeTimers();

        AddAbility(GalatimeGlobals.GetAbilityById("flamethrower"), 0);
        AddAbility(GalatimeGlobals.GetAbilityById("fireball"), 1);
        AddAbility(GalatimeGlobals.GetAbilityById("firewave"), 2);
    }

    private void InitializeTimers()
    {
        RetreatDelayTimer = new()
        {
            WaitTime = 0.3f,
            OneShot = true
        };
        AddChild(RetreatDelayTimer);

        MoveDelayTimer = new()
        {
            WaitTime = 0.3f,
            OneShot = true
        };
        AddChild(MoveDelayTimer);
    }

    float PathRotation => Body.GlobalPosition.AngleToPoint(Navigation.GetNextPathPosition());

    public override void _MoveProcess()
    {
        base._MoveProcess();

        if (TargetController.CurrentTarget != null) CombatMovement();
        // Moving normally when there is no enemies.
        else NormalMovement();
    }

    private async void CombatMovement()
    {
        Vector2 vectorPath;

        // Take a sword if not equipped.
        if (Weapon.Item == null) Weapon.TakeItem(GalatimeGlobals.GetItemById("golden_holder_sword"));
        // Set RayCast position by angle to the enemy.
        RayCast.TargetPosition = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition)) * 200;
        // Set target position to the next enemy.
        Navigation.TargetPosition = TargetController.CurrentTarget.GlobalPosition;

        // Vector from the target.
        await ToSignal(GetTree(), "physics_frame"); // Wait one physics frame
        vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition());

        // Rotation to the enemy.
        var enemyRotation = Body.GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition);
        Weapon.Rotation = enemyRotation;

        // Moving behavior based on distance.
        var distance = Body.GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition);
        if (distance >= 200 && MoveDelayTimer.TimeLeft == 0) MoveDelayTimer.Start();
        vectorPath = MoveDelayTimer.TimeLeft > 0 ? vectorPath : Vector2.Zero;
        if (RetreatDelayTimer.TimeLeft > 0) vectorPath = Vector2.Right.Rotated(enemyRotation + MathF.PI);
        if (distance <= 150 && RetreatDelayTimer.TimeLeft == 0) RetreatDelayTimer.Start();

        // Checking if enemy is close.
        if (RayCast.IsColliding())
        {
            var obj = RayCast.GetCollider();

            // Check if enemy is enemy and not dead.
            if (obj is Entity e && e.IsInGroup("enemy") && !e.DeathState)
            {
                // Weapon.Rotation = (float)Body.GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition);

                // Use all abilities by order.
                for (int i = 0; i < Abilities.Count; i++)
                {
                    var ability = Abilities[i];
                    if (ability.IsReloaded) UseAbility(i);
                }
            }
        }
        // Check if any enemies are too close.
        var swordColliders = Weapon.GetOverlappingBodies();
        if (swordColliders.Count >= 1)
        {
            var obj = swordColliders[0];
            // Check if enemy is enemy and not dead.
            if (obj is Entity e && e.IsInGroup("enemy") && !e.DeathState) Weapon.Attack(this);
        }
        Body.Velocity = vectorPath.Normalized() * Speed;
        Body.MoveAndSlide();
    }

    /// <summary> Process of normal movement of the character. </summary>
    private async void NormalMovement()
    {
        Weapon.Rotation = PathRotation;

        var allies = GetTree().GetNodesInGroup("ally");
        var followTo = allies[FollowOrder] as CharacterBody2D;

        Vector2 vectorPath;
        RayCast.TargetPosition = Vector2.Zero;
        Navigation.TargetPosition = followTo.GlobalPosition;

        await ToSignal(GetTree(), "physics_frame"); // Wait one physics frame
        vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition());
        var distance = Body.GlobalPosition.DistanceTo(followTo.GlobalPosition);
        if (distance >= 150 && MoveDelayTimer.TimeLeft == 0) MoveDelayTimer.Start();
        vectorPath = MoveDelayTimer.TimeLeft > 0 ? vectorPath : Vector2.Zero;
        Body.Velocity = vectorPath.Normalized() * Speed;
        Body.MoveAndSlide();
    }
}
