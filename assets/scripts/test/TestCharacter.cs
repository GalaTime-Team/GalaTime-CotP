using System;
using Galatime;
using Galatime.Helpers;
using Godot;

public partial class TestCharacter : HumanoidCharacter
{
    [Export] public int FollowOrder;
    [Export] public Godot.Collections.Array<string> DefaultAbilities;

    public NavigationAgent2D Navigation;
    public RayCast2D RayCast;
    public AnimationPlayer AnimationPlayer;

    public bool StrafeDirection = true;
    public bool MeleeMode = false;

    public TargetController TargetController;

    public Timer RetreatDelayTimer, MoveDelayTimer, StrafeTimer, EnemySwitchDelayTimer, AttackTimer;

    public Player Player;

    private bool possessed = false;
    /// <summary> True if the character is currently being possessed. That means the player is controlling it. </summary>
    public bool Possessed
    {
        get => possessed;
        set
        {
            possessed = value;
            // Stop the attack timer, because no need to attack automatically.
            if (value) AttackTimer.Stop();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        Weapon = GetNode<Hand>("Hand");
        HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");
        TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
        DrinkingAudioPlayer = GetNode<AudioStreamPlayer2D>("DrinkingAudioPlayer");

        Body = this;

        AnimationPlayer = GetNode<AnimationPlayer>("Animation");
        TargetController = GetNode<TargetController>("TargetController");
        Navigation = GetNode<NavigationAgent2D>("Navigation");
        RayCast = GetNode<RayCast2D>("RayCast");

        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        Player = playerVariables.Player;

        InitializeTimers();

        for (var i = 0; i < (DefaultAbilities != null ? DefaultAbilities.Count : 0); i++) { AddAbility(GalatimeGlobals.GetAbilityById(DefaultAbilities[i]), i); }
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

        StrafeTimer = new()
        {
            WaitTime = 0.5f
        };
        AddChild(StrafeTimer);
        StrafeTimer.Timeout += ChangeStrafeDirection;
        StrafeTimer.Start();

        AttackTimer = new()
        {
            WaitTime = 0.25f
        };
        AddChild(AttackTimer);
        AttackTimer.Timeout += Attack;
        AttackTimer.Start();
    }

    float PathRotation => Body.GlobalPosition.AngleToPoint(Navigation.GetNextPathPosition());

    public override void _MoveProcess()
    {
        base._MoveProcess();

        if (Possessed) return;

        if (TargetController.CurrentTarget != null) CombatMovement();
        // Moving normally when there is no enemies.
        else NormalMovement();
    }

    private async void CombatMovement()
    {
        if (AttackTimer.IsStopped()) AttackTimer.Start();

        Vector2 vectorPath;

        // Take a sword if not equipped.
        if (Weapon.Item == null) Weapon.TakeItem(GalatimeGlobals.GetItemById("golden_holder_sword"));
        // Set RayCast position by angle to the enemy.
        RayCast.TargetPosition = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition)) * 200;
        // Set target position to the next enemy.
        Navigation.TargetPosition = TargetController.CurrentTarget.GlobalPosition;

        // Vector from the target.
        var pathRotation = Body.GlobalPosition.AngleToPoint(Navigation.GetNextPathPosition());
        await ToSignal(GetTree(), "physics_frame"); // Wait one physics frame
        vectorPath = Vector2.Right.Rotated(pathRotation);

        // Rotation to the enemy.
        if (TargetController.CurrentTarget == null) return; // Make sure there is an enemy.
        var enemyRotation = Body.GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition);
        Weapon.Rotation = enemyRotation;

        // Check if is in melee mode. Melee mode is when ally only uses sword. No need to use abilities when in melee mode.
        if (!MeleeMode)
        {
            // Moving behavior based on distance.
            var distance = Body.GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition);
            if (distance >= 200 && MoveDelayTimer.TimeLeft == 0) MoveDelayTimer.Start();
            vectorPath = MoveDelayTimer.TimeLeft > 0 ? vectorPath : Vector2.Zero;
            if (RetreatDelayTimer.TimeLeft > 0) vectorPath = Vector2.Right.Rotated(enemyRotation + MathF.PI);
            if (distance <= 150 && RetreatDelayTimer.TimeLeft == 0) RetreatDelayTimer.Start();
        }

        // Strafe up and down if the enemy.
        vectorPath += new Vector2(0, StrafeDirection ? -1 : 1).Rotated(pathRotation);

        // Check if any enemies are too close.
        var swordColliders = Weapon.GetOverlappingBodies();
        if (swordColliders.Count >= 1)
        {
            var obj = swordColliders[0];
            // Check if enemy is enemy and not dead.
            if (obj is Entity e && e.IsInGroup("enemy") && !e.DeathState) Weapon.Attack(this);
        }
        // For some reason it is moving at twice the speed, so it is divided by 2.
        Body.Velocity = vectorPath.Normalized() * Speed / 2;
        Body.MoveAndSlide();
    }

    public bool IsEnemy() => RayCast.GetCollider() is Entity e && e.IsInGroup("enemy") && !e.DeathState;
    public void Attack()
    {
        var reloadedAbilities = Abilities.FindAll(x => CanUseAbility(x));

        // If there are no abilities that can be used, use sword.
        GD.Print($"Melee mode: {MeleeMode}. Reloaded abilities: {reloadedAbilities.Count}");
        if (reloadedAbilities.Count == 0)
        {
            MeleeMode = true;
            return;
        }
        else MeleeMode = false;

        var obj = RayCast.GetCollider();
        // Check if enemy is enemy and not dead.
        if (IsEnemy())
        {
            var rnd = new Random();
            var i = rnd.Next(0, reloadedAbilities.Count);
            UseAbility(i);
        }
    }

    public void ChangeStrafeDirection()
    {
        var rnd = new Random();
        var i = rnd.Next(0, 2);
        StrafeDirection = i == 0;
    }

    /// <summary> Process of normal movement of the character. </summary>
    private async void NormalMovement()
    {
        Weapon.Rotation = PathRotation;

        // var allies = GetTree().GetNodesInGroup("ally");
        // var followTo = allies[FollowOrder] as CharacterBody2D;
        var followTo = Player.CurrentCharacter;

        if (followTo == null) return;

        Vector2 vectorPath;
        RayCast.TargetPosition = Vector2.Zero;
        Navigation.TargetPosition = followTo.GlobalPosition;

        await ToSignal(GetTree(), "physics_frame"); // Wait one physics frame
        vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition());
        var distance = Body.GlobalPosition.DistanceTo(followTo.GlobalPosition);
        if (distance >= 150 && MoveDelayTimer.TimeLeft == 0) MoveDelayTimer.Start();
        vectorPath = MoveDelayTimer.TimeLeft > 0 ? vectorPath : Vector2.Zero;
        // For some reason it is moving at twice the speed, so it is divided by 2.
        Body.Velocity = vectorPath.Normalized() * Speed / 2;
        Body.MoveAndSlide();
    }
}
