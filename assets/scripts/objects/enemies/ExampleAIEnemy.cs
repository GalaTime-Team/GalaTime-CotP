using Galatime;
using Galatime.AI.Controller;
using Galatime.Helpers;
using Godot;

/// <summary>
/// Example enemy using the new AI Controller system.
/// This enemy demonstrates:
/// - Fleeing when low on health
/// - Using ranged attacks when possible
/// - Dodging when target gets too close
/// - Strafing around target
/// - Distinct behavior through probability
/// </summary>
public partial class ExampleAIEnemy : Entity
{
    public NavigationAgent2D Navigation;
    public TargetController TargetController;
    public AIController AIController;

    public override void _Ready()
    {
        base._Ready();
        
        Body = this;
        
        // Get required nodes
        Navigation = GetNodeOrNull<NavigationAgent2D>("Navigation");
        TargetController = GetNodeOrNull<TargetController>("TargetController");
        
        // Setup AI Controller
        SetupAI();
    }

    private void SetupAI()
    {
        // Create AI Controller
        AIController = new AIController();
        AIController.Entity = this;
        AIController.DebugMode = false; // Set to true for debugging
        AddChild(AIController);

        // Priority 100: Flee when health is low (always executes when conditions met)
        var fleeRule = new AIRule("FleeWhenLowHealth", new FleeBehavior(fleeDistance: 400f), priority: 100)
            .AddCondition(new LowHealthCondition(threshold: 0.25f)) // Below 25% health
            .AddCondition(new HasTargetCondition());
        AIController.AddRule(fleeRule);

        // Priority 80: Dodge when target gets too close (70% probability for variety)
        var dodgeRule = new AIRule("DodgeWhenClose", new DodgeBehavior(200f, false, 0f, cooldown: 3f), priority: 80, probability: 0.7f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 120f));
        AIController.AddRule(dodgeRule);

        // Priority 60: Use first ability when ready and at good distance
        var ability0Rule = new AIRule("UseAbility0", new RangedAttackBehavior(abilityIndex: 0, strafe: true, optimalDistance: 300f, cooldown: 1.5f), priority: 60)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new AbilityReadyCondition(0))
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 150f));
        AIController.AddRule(ability0Rule);

        // Priority 50: Use second ability when ready (if available)
        var ability1Rule = new AIRule("UseAbility1", new RangedAttackBehavior(abilityIndex: 1, strafe: true, optimalDistance: 250f, cooldown: 2f), priority: 50, probability: 0.8f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new AbilityReadyCondition(1))
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 150f));
        AIController.AddRule(ability1Rule);

        // Priority 30: Strafe clockwise (50% probability)
        var strafeClockwiseRule = new AIRule("StrafeClockwise", new StrafeBehavior(optimalDistance: 250f, clockwise: true), priority: 30, probability: 0.5f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 100f));
        AIController.AddRule(strafeClockwiseRule);

        // Priority 30: Strafe counter-clockwise (50% probability)
        var strafeCounterRule = new AIRule("StrafeCounterClockwise", new StrafeBehavior(optimalDistance: 250f, clockwise: false), priority: 30, probability: 0.5f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 100f));
        AIController.AddRule(strafeCounterRule);

        // Priority 20: Move toward target when too far
        var approachRule = new AIRule("Approach", new MeleeAttackBehavior(stopDistance: 200f), priority: 20)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 350f));
        AIController.AddRule(approachRule);

        // Priority 0: Idle when no target
        var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
            .AddCondition(new NoTargetCondition());
        AIController.AddRule(idleRule);

        // Integrate with entity AI system
        AddAIBehavior((delta) => AIController.Process(delta));
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        base._DeathEvent(damageRotation);
        // Cleanup if needed
    }
}
