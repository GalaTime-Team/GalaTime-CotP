using Godot;

using Galatime;
using Galatime.Global;
using Galatime.Helpers;
using Galatime.AI.Controller;

public partial class Slime : Entity
{
	#region Nodes
	public NavigationAgent2D Navigation;
	public Sprite2D Sprite;

	/// <summary> Area for the character's weapon. </summary>
	public Area2D Weapon;
	public AnimationPlayer AnimationPlayer;

	/// <summary> Timer for countdown to attack. </summary>
	public Timer AttackCountdownTimer;
	public TargetController TargetController;

	public GpuParticles2D Particles;
	
	/// <summary> AI Controller for intelligent behavior. </summary>
	public AIController AIController;
	#endregion

	#region Variables
	/// <summary> Packed scene for slime enemies. </summary>
	public PackedScene SlimeScene;
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
		
		// Setup AI Controller
		SetupAI();
	}
	
	private void SetupAI()
	{
		// Create AI Controller
		AIController = new AIController();
		AIController.Entity = this;
		AIController.DebugMode = false;
		AddChild(AIController);
		
		// Load slime melee ability
		var meleeAbility = GalatimeGlobals.GetAbilityById("slime_melee");
		if (meleeAbility != null)
		{
			AddAbility(meleeAbility, 0);
		}
		
		// Rule 1: Melee attack when has target (priority 50)
		var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(stopDistance: 50f), priority: 50)
			.AddCondition(new HasTargetCondition());
		AIController.AddRule(meleeRule);
		
		// Rule 2: Idle when no target (priority 0)
		var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
			.AddCondition(new NoTargetCondition());
		AIController.AddRule(idleRule);
		
		// Add controller to AI behavior system
		AddAIBehavior((delta) => AIController.Process(delta));
	}

	public override void _ExitTree()
	{
		Weapon.BodyEntered -= Attack;
		Weapon.BodyExited -= OnAreaExit;
	}

	public void Spawned()
	{
		if (AnimationPlayer == null) return;
		
		CanMove = true;
		AnimationPlayer.Play("walk");
	}

	public override void _AIProcess(double delta)
	{
		// Call base AI behaviors first (includes AI Controller)
		base._AIProcess(delta);
		
		// Keep existing movement logic for compatibility
		if (!DeathState) Move(); else Body.Velocity = Vector2.Zero;
	}

	public override void _DeathEvent(float damageRotation = 0f)
	{
		base._DeathEvent();
		PlayerVariables.Instance.DiscoverEnemy(1);
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
		GalatimeElement element = ElementManager.Aqua;
		float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
		entity.TakeDamage(50, Stats[EntityStatType.PhysicalAttack].Value, element, DamageType.Physical, 500, damageRotation);

		AnimationPlayer.Play("hit");
	}

	public void SpawnParticles()
	{
		if (Particles == null) return;
		
		var particles = Particles.Duplicate() as GpuParticles2D;
		AddChild(particles);
		particles.TopLevel = true;
		particles.Emitting = true;
		particles.GlobalPosition = GlobalPosition;
	}

	public void OnAreaExit(Node2D body) => AttackCountdownTimer.Stop();

	public void Move()
	{
		// Check if required nodes are initialized
		if (Navigation == null || TargetController == null || Weapon == null) return;
		
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
			if (Sprite != null) Sprite.FlipH = rotationDegPositive <= 90;
			Body.Velocity = vectorPath;
		}
		else Body.Velocity = Vector2.Zero;
	}
}
