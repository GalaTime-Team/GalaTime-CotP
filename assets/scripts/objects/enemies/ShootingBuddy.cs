using Galatime;
using Galatime.Helpers;
using Galatime.AI.Controller;
using Godot;

public partial class ShootingBuddy : Entity
{
	#region Nodes
	public Timer ShootingTimer;
	public Projectile Projectile;
	public Sprite2D Sprite;
	public TargetController TargetController;
	public CollisionShape2D Collision;
	
	/// <summary> AI Controller for intelligent behavior. </summary>
	public AIController AIController;
	#endregion


	public override void _Ready()
	{
		base._Ready();

		#region Get nodes
		ShootingTimer = GetNode<Timer>("ShootingTimer");
		Projectile = GetNode<Projectile>("Projectile");
		Sprite = GetNode<Sprite2D>("Sprite2D");
		TargetController = GetNode<TargetController>("TargetController");
		Collision = GetNode<CollisionShape2D>("Collision");
		#endregion

		Body = this;

		ShootingTimer.Timeout += OnShootingTimerTimeout;

		Projectile.TimeoutTimer.WaitTime = 999f;
		
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
		
		// Keep projectile shooting behavior as timer-based
		AddAIBehavior(ProjectileShootingBehavior);
		
		// Rule 1: Stay at range when has target (priority 30)
		var strafeRule = new AIRule("Strafe", new StrafeBehavior(optimalDistance: 300f, clockwise: true), priority: 30, probability: 0.6f)
			.AddCondition(new HasTargetCondition());
		AIController.AddRule(strafeRule);
		
		// Rule 2: Idle when no target (priority 0)
		var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
			.AddCondition(new NoTargetCondition());
		AIController.AddRule(idleRule);
		
		// Add controller to AI behavior system
		AddAIBehavior((delta) => AIController.Process(delta));
	}
	
	/// <summary> Custom AI behavior for shooting projectiles at the target. </summary>
	private void ProjectileShootingBehavior(double delta)
	{
		// Timer-based shooting - the behavior is handled by the timer
		// This is kept for compatibility with existing timer system
	}

	private void OnShootingTimerTimeout()
	{
		// Don't shoot if no target or AI is disabled.
		if (TargetController.CurrentTarget == null || DisableAI || DeathState) return; 

		var projectile = Projectile.Duplicate() as Projectile;
		projectile.AttackStat = Stats[EntityStatType.MagicalAttack].Value;
		projectile.Visible = true;
		projectile.Moving = true;
		projectile.Explosive = true;
		projectile.Exploded += OnProjectileExploded;
		projectile.TopLevel = true;
		
		projectile.GlobalPosition = GlobalPosition;
		projectile.Rotation = GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition);

		AddChild(projectile);
		projectile.Explosion.Element = Element;
		projectile.TimeoutTimer.WaitTime = 10f;
	}

	private void OnProjectileExploded(Projectile projectile = null)
	{
		projectile.GetNode<Sprite2D>("Sprite").Visible = false;
	}
	
	public override void _AIProcess(double delta)
	{
		// Call base to execute custom AI behaviors (includes AI Controller)
		base._AIProcess(delta);
	}

	public override void _DeathEvent(float damageRotation = 0f)
	{
		base._DeathEvent(damageRotation);
		Sprite.Visible = false;
		ShootingTimer.Stop();
		Callable.From(() => Collision.Disabled = true).CallDeferred();
	}
}
