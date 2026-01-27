using Galatime;
using Galatime.AI.Controller;
using Galatime.Helpers;
using Godot;

/// <summary>
/// Example ally using the new AI Controller system.
/// This ally demonstrates:
/// - Using abilities intelligently
/// - Melee attacking when abilities on cooldown
/// - Following player when no enemies
/// - Resource management (mana/stamina)
/// </summary>
public partial class ExampleAIAlly : HumanoidCharacter
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
        AIController.DebugMode = false;
        AddChild(AIController);

        // Priority 90: Dodge when low stamina and target close (avoid using stamina for dodge)
        var conserveStaminaRule = new AIRule("ConserveStamina", new FleeBehavior(300f, cooldown: 2f), priority: 90)
            .AddCondition(new LowStaminaCondition(0.3f))
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 150f));
        AIController.AddRule(conserveStaminaRule);

        // Priority 70: Use ability 0 when mana available and target at good distance (70% probability)
        var ability0Rule = new AIRule("UseAbility0", new RangedAttackBehavior(0, true, 300f, cooldown: 1f), priority: 70, probability: 0.7f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new AbilityReadyCondition(0))
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 200f));
        AIController.AddRule(ability0Rule);

        // Priority 65: Use ability 1 when available (60% probability for variety)
        var ability1Rule = new AIRule("UseAbility1", new RangedAttackBehavior(1, true, 250f, cooldown: 1.5f), priority: 65, probability: 0.6f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new AbilityReadyCondition(1));
        AIController.AddRule(ability1Rule);

        // Priority 60: Use ability 2 when available (50% probability)
        var ability2Rule = new AIRule("UseAbility2", new RangedAttackBehavior(2, true, 280f, cooldown: 2f), priority: 60, probability: 0.5f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new AbilityReadyCondition(2));
        AIController.AddRule(ability2Rule);

        // Priority 40: Dodge when target very close and have stamina
        var dodgeRule = new AIRule("DodgeClose", new DodgeBehavior(150f, true, 10f, cooldown: 3f), priority: 40, probability: 0.8f)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 80f));
        AIController.AddRule(dodgeRule);

        // Priority 30: Melee attack when target close (fallback when abilities on cooldown)
        var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(60f), priority: 30)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 250f));
        AIController.AddRule(meleeRule);

        // Priority 20: Strafe around target
        var strafeRule = new AIRule("Strafe", new StrafeBehavior(200f, true), priority: 20, probability: 0.6f)
            .AddCondition(new HasTargetCondition());
        AIController.AddRule(strafeRule);

        // Priority 10: Follow player when no enemies
        var followRule = new AIRule("FollowPlayer", new FollowPlayerBehavior(120f), priority: 10)
            .AddCondition(new NoTargetCondition());
        AIController.AddRule(followRule);

        // Priority 0: Idle as last resort
        var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0);
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
