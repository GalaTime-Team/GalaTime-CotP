using System;
using Galatime;
using Galatime.Global;
using Galatime.Helpers;
using Galatime.Interfaces;
using Galatime.AI.Controller;
using Godot;

public partial class TestCharacter : HumanoidCharacter, IDrama
{
	[Export] public int FollowOrder;
	[Export] public Godot.Collections.Array<string> DefaultAbilities;

	public NavigationAgent2D Navigation;
	public RayCast2D RayCast;
	public AnimationPlayer AnimationPlayer;

	public bool StrafeDirection = true;
	public bool MeleeMode;

	public TargetController TargetController;

	public Timer RetreatDelayTimer, MoveDelayTimer, StrafeTimer, EnemySwitchDelayTimer, AttackTimer;

	public Player Player;
	
	/// <summary> AI Controller for intelligent behavior when not possessed. </summary>
	public AIController AIController;

	private bool possessed;
	/// <summary> True if the character is currently being possessed. That means the player is controlling it. </summary>
	public bool Possessed
	{
		get => possessed;
		set
		{
			possessed = value;
			// Stop the attack timer, because no need to attack automatically.
			if (value) AttackTimer.Stop();
			// Disable AI Controller when possessed
			if (AIController != null) AIController.Enabled = !value;
		}
	}

	[Export] public string DramaID { get; set; }
	[Export] public Node2D DramaNode { get; set; }
	public void SetDramaObject() => CutsceneManager.Instance.RegisterDramaObject(this);
	

	public override void _Ready()
	{
		base._Ready();

		SetDramaObject();

		Weapon = GetNode<Hand>("Hand");
		HumanoidDoll = GetNode<HumanoidDoll>("HumanoidDoll");
		TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
		DrinkingAudioPlayer = GetNode<AudioStreamPlayer2D>("DrinkingAudioPlayer");
		Sprite = GetNode<Sprite2D>("Sprite2D");

		Body = this;

		AnimationPlayer = GetNode<AnimationPlayer>("Animation");
		TargetController = GetNode<TargetController>("TargetController");
		Navigation = GetNode<NavigationAgent2D>("Navigation");
		RayCast = GetNode<RayCast2D>("RayCast");

		var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
		Player = playerVariables.Player;

		InitializeTimers();

		for (var i = 0; i < (DefaultAbilities != null ? DefaultAbilities.Count : 0); i++) { AddAbility(GalatimeGlobals.GetAbilityById(DefaultAbilities[i]), i); }

		if (LevelManager.Instance.CheatsMenu.GetCheat("god_mode").Active) Invincible = true;
		
		// Setup AI Controller for when not possessed
		SetupAI();
	}
	
	private void SetupAI()
	{
		// Create AI Controller (only used when not possessed)
		AIController = new AIController();
		AIController.Entity = this;
		AIController.DebugMode = false;
		AIController.Enabled = !Possessed; // Disable if currently possessed
		AddChild(AIController);
		
		// Priority 90: Conserve stamina when low
		var conserveStaminaRule = new AIRule("ConserveStamina", new FleeBehavior(300f, cooldown: 2f), priority: 90)
			.AddCondition(new LowStaminaCondition(0.3f))
			.AddCondition(new HasTargetCondition())
			.AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 150f));
		AIController.AddRule(conserveStaminaRule);
		
		// Priority 70: Use ability 0 when available (70% probability)
		var ability0Rule = new AIRule("UseAbility0", new RangedAttackBehavior(0, true, 300f, cooldown: 1f), priority: 70, probability: 0.7f)
			.AddCondition(new HasTargetCondition())
			.AddCondition(new AbilityReadyCondition(0))
			.AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 200f));
		AIController.AddRule(ability0Rule);
		
		// Priority 65: Use ability 1 when available (60% probability)
		var ability1Rule = new AIRule("UseAbility1", new RangedAttackBehavior(1, true, 250f, cooldown: 1.5f), priority: 65, probability: 0.6f)
			.AddCondition(new HasTargetCondition())
			.AddCondition(new AbilityReadyCondition(1));
		AIController.AddRule(ability1Rule);
		
		// Priority 60: Use ability 2 when available (50% probability)
		var ability2Rule = new AIRule("UseAbility2", new RangedAttackBehavior(2, true, 280f, cooldown: 2f), priority: 60, probability: 0.5f)
			.AddCondition(new HasTargetCondition())
			.AddCondition(new AbilityReadyCondition(2));
		AIController.AddRule(ability2Rule);
		
		// Priority 10: Follow player when no enemies
		var followRule = new AIRule("FollowPlayer", new FollowPlayerBehavior(120f), priority: 10)
			.AddCondition(new NoTargetCondition());
		AIController.AddRule(followRule);
		
		// Priority 0: Idle as last resort
		var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0);
		AIController.AddRule(idleRule);
		
		// Add controller to AI behavior system (only active when not possessed)
		AddAIBehavior((delta) => {
			if (!Possessed && AIController != null)
			{
				AIController.Process(delta);
			}
		});
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

	public override void _AIProcess(double delta)
	{
		// Call base AI behaviors first
		base._AIProcess(delta);
		
		if (Possessed || DeathState) return;
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

		Body.Velocity = vectorPath.Normalized() * Speed;
	}

	public bool IsEnemy() => RayCast.GetCollider() is Entity e && e.IsInGroup("enemy") && !e.DeathState;
	public void Attack()
	{
		var reloadedAbilities = Abilities.FindAll(x => CanUseAbility(x));

		// If there are no abilities that can be used, use sword.
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

		Body.Velocity = vectorPath.Normalized() * Speed;
		if (IsPushing) Body.Velocity *= PushingSpeedMultiplier;
	}

	public bool PlayDramaAnimation(string animationName)
	{
		if (!AnimationPlayer.HasAnimation(animationName)) return false;
		AnimationPlayer.Play(animationName);
		return true;
	}

	public void StopDramaAnimation()
	{
		AnimationPlayer.Stop();
	}
}
