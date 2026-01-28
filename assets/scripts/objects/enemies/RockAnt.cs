using System;

using Galatime;
using Galatime.AI;
using Galatime.AI.Controller;
using Galatime.Helpers;
using Galatime.Damage;
using ExtensionMethods;

using Godot;
using Galatime.Global;

public partial class RockAnt : Entity
{
	public Sprite2D Sprite;
	public AudioStreamPlayer2D AudioWalk;
	public AudioStreamPlayer2D AudioBurrow;
	public CollisionShape2D Collision;
	public DamageArea DigDamageArea;
	public DamageArea DamageArea;

	public DangerNotifierEffect DangerEffect;

	public Navigator Navigator;
	public TargetController TargetController;
	public AttackSwitcher AttackSwitcher;
	public RangedHitTracker RangedHitTracker;
	
	/// <summary> AI Controller for intelligent behavior. </summary>
	public AIController AIController;

	/// <summary> If the rock ant is currently targetting (is positioning towards a target). </summary>
	public bool DigTargetting;

	public override void _Ready()
	{
		base._Ready();

		Sprite = GetNode<Sprite2D>("Sprite2D");
		AudioWalk = GetNode<AudioStreamPlayer2D>("AudioWalk");
		AudioBurrow = GetNode<AudioStreamPlayer2D>("AudioBurrow");
		Collision = GetNode<CollisionShape2D>("Collision");
		Navigator = GetNode<Navigator>("Navigator");
		TargetController = GetNode<TargetController>("TargetController");
		AttackSwitcher = GetNode<AttackSwitcher>("AttackSwitcher");
		RangedHitTracker = GetNode<RangedHitTracker>("RangedHitTracker");
		DigDamageArea = GetNode<DamageArea>("DigDamageArea");
		DamageArea = GetNode<DamageArea>("DamageArea");

		TargetController.OnTargetChanged += () =>
		{
			GD.Print("Changed target");
			Navigator.Target = TargetController.CurrentTarget;
		};

		Body = this;

		RegisterAttacks();
		AttackSwitcher.NextCycle();
		
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
		
		// Load RockAnt abilities
		var digAbility = GalatimeGlobals.GetAbilityById("rockant_dig");
		if (digAbility != null)
		{
			AddAbility(digAbility, 0);
		}
		
		var meleeAbility = GalatimeGlobals.GetAbilityById("rockant_melee");
		if (meleeAbility != null)
		{
			AddAbility(meleeAbility, 1);
		}
		
		// Priority 50: Melee when close to target
		var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(stopDistance: 80f), priority: 50)
			.AddCondition(new HasTargetCondition())
			.AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 150f));
		AIController.AddRule(meleeRule);
		
		// Priority 40: Approach if too far
		var approachRule = new AIRule("Approach", new MeleeAttackBehavior(stopDistance: 100f), priority: 40)
			.AddCondition(new HasTargetCondition())
			.AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 150f));
		AIController.AddRule(approachRule);
		
		// Priority 0: Idle when no target
		var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
			.AddCondition(new NoTargetCondition());
		AIController.AddRule(idleRule);
		
		// Keep existing movement behavior for compatibility with AttackSwitcher
		AddAIBehavior(MovementBehavior);
		
		// Add controller to AI behavior system
		AddAIBehavior((delta) => AIController.Process(delta));
	}

	public void RegisterAttacks()
	{
		AttackSwitcher.RegisterAttackCycles
		(
			new AttackCycle("dig", Dig, Reset, .25f, () => !RangedHitTracker.CanHit),
			new AttackCycle("melee", () => {
				DamageArea.Active = true;
				DamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
				
				AttackSwitcher.StartTimer("melee", () => 
				{
					DamageArea.Active = false;
					AttackSwitcher.NextCycle();
				}, 1f);
			}, Reset, .75f)
		);
	}

	public override void _MoveProcess(double delta)
	{
		bool on = true;
		if (!DeathState) on = !DisableAI;
		if (DeathState || !TargetController.HasTarget) on = false;
		AttackSwitcher.Enabled = on;

		Velocity = Vector2.Zero;
	}

	public override void _DeathEvent(float damageRotation = 0f)
	{
		AttackSwitcher.Enabled = false;
		Reset();

		// Make sprite red so it's obvious it's dead.
		Sprite.Modulate = GameColors.Red;

		PlayerVariables.Instance.DiscoverEnemy(2);
	}

	public void Dig()
	{
		AudioBurrow.Play();
		AttackSwitcher.StartTimer("dig", () => 
		{
			AIIgnore = true;
			Sprite.Visible = false;
			EndDig();
		}, .5f);
	}

	public void EndDig()
	{
		DigTargetting = true;

		var rnd = new Random();
		var delay = rnd.Next(1, 4);

		AttackSwitcher.StartTimer("dig", () => 
		{
			DangerEffect = DangerNotifierEffect.GetInstance();
			AddChild(DangerEffect);
			DangerEffect.GlobalPosition = GlobalPosition;

			DigTargetting = false;

			DangerEffect.Start();
			AttackSwitcher.StartTimer("dig", () => 
			{
				AIIgnore = false;
				Sprite.Visible = true;

				DigDamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
				DigDamageArea.HitOneTime();

				DangerEffect.QueueFree();
				DangerEffect = null;
				AudioBurrow.Play();

				AttackSwitcher.NextCycle();
			}, .35f);
		}, delay);
	}

	public void Reset()
	{
		DigTargetting = false;

		DangerEffect?.QueueFree();
		DangerEffect = null;

		Sprite.Visible = true;
		Callable.From(() => Collision.Disabled = false).CallDeferred();

		DamageArea.Active = false;
		DigDamageArea.Active = false;

		AudioWalk.Stop();
	}

	public override void _AIProcess(double delta)
	{
		// Call base to execute custom AI behaviors
		base._AIProcess(delta);
	}
	
	/// <summary> Custom AI behavior for movement and attack patterns. </summary>
	private void MovementBehavior(double delta)
	{
		Velocity = Vector2.Zero;

		var t = TargetController.CurrentTarget;
		// If no target, do nothing
		if (t == null || DeathState) return;

		if (AttackSwitcher.IsAttackCycleActive("melee"))
		{
			DamageArea.Rotation = GlobalPosition.AngleToPoint(t.GlobalPosition) + (float)Math.PI;

			Navigator.Speed = Speed;
			var v = Navigator.NavigatorVelocity;
			
			if (!AudioWalk.Playing) AudioWalk.Play();
			if (v.Length() < 10 && AudioWalk.Playing) AudioWalk.Stop();

			Velocity = v;
			MoveAndSlide();
		}
		else
			if (AudioWalk.Playing) AudioWalk.Stop();

		// Targetting behavior
		if (DigTargetting)
			GlobalPosition = t.GlobalPosition;

		// Disable collision if targetting.
		if (DigTargetting != Collision.Disabled) Callable.From(() => Collision.Disabled = DigTargetting).CallDeferred();
 
		// Flip sprite based on direction.
		Sprite.FlipSpriteByAngle(GlobalPosition.AngleToPoint(t.GlobalPosition));
	}
}
