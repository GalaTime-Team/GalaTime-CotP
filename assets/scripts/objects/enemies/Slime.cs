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
		
		// Setup AI Controller
		SetupAI();
	}
	
	private void SetupAI()
	{
		// Create AI Controller (used only when AI rules are configured in scene)
		// AI rules should be configured in the scene via AIRules property, not hardcoded here.
		AIController = new AIController();
		AIController.Entity = this;
		AIController.DebugMode = false;
		AddChild(AIController);
		
		// Add controller to AI behavior system (processes scene-configured rules)
		AddAIBehavior((delta) => AIController.Process(delta));
	}

	public override void _ExitTree()
	{
		
	}

	public void Spawned()
	{
		if (AnimationPlayer == null) return;
		
		CanMove = true;
		// Don't auto-play walk animation - let _PhysicsProcess control it based on actual movement
	}

	public override void _AIProcess(double delta)
	{
		// Call base AI behaviors first (includes AI Controller)
		base._AIProcess(delta);
		
		// Legacy movement method (commented out):
		// if (!DeathState) Move(); else Body.Velocity = Vector2.Zero;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		// Control animation based on actual movement
		if (AnimationPlayer != null && !DeathState)
		{
			// Check if slime is actually moving (velocity > small threshold)
			if (Body.Velocity.Length() > 10f)
			{
				// Only play walk if not already playing
				if (AnimationPlayer.CurrentAnimation != "walk")
				{
					AnimationPlayer.Play("walk");
				}
			}
			else
			{
				// Stop animation when idle (not moving)
				if (AnimationPlayer.CurrentAnimation == "walk")
				{
					AnimationPlayer.Stop();
				}
			}
		}
	}

	public override void _DeathEvent(float damageRotation = 0f)
	{
		base._DeathEvent();
		PlayerVariables.Instance.DiscoverEnemy(1);
		DropXp();
		AnimationPlayer.Play("outro");
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

	// DISABLED: Legacy method for hardcoded attack system
	// Legacy method (commented out):
	// public void OnAreaExit(Node2D body) => AttackCountdownTimer.Stop();

	public void Move()
	{
		// Check if required nodes are initialized
		if (Navigation == null || TargetController == null || Weapon == null) return;
		
		var enemy = TargetController.CurrentTarget;
		if (enemy != null && CanMove)
		{
			// Calculate distance to target
			float distanceToTarget = Body.GlobalPosition.DistanceTo(enemy.GlobalPosition);
			
			// Stop moving when close enough to target (prevents sticking/overlapping)
			// Minimum distance should be slightly more than weapon range
			const float MIN_DISTANCE = 70f; // Stop at 70 pixels from target
			
			if (distanceToTarget > MIN_DISTANCE)
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
			else
			{
				// Too close - stop moving to prevent sticking
				Body.Velocity = Vector2.Zero;
				// Still face the target
				float rotation = Body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
				Weapon.Rotation = rotation;
				float rotationDeg = Mathf.RadToDeg(rotation);
				float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
				if (Sprite != null) Sprite.FlipH = rotationDegPositive <= 90;
			}
		}
		else Body.Velocity = Vector2.Zero;
	}
}
