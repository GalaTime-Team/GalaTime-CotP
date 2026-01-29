# Slime AI Removal Guide

## Summary

This guide explains why hardcoded AI was removed from the Slime enemy and how to configure Slime behavior through the Godot scene editor.

## Problem

**User Report**: "Slime still moves and attacks" despite no AI rules, conditions, or behaviors being configured in the scene.

**Root Cause**: The `Slime.SetupAI()` method had hardcoded AI rules that automatically made ALL slime instances chase and attack players, regardless of scene configuration.

## What Was Changed

### Before (Hardcoded AI - Forced Behavior)

```csharp
private void SetupAI()
{
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
    
    // HARDCODED RULES - FORCED ALL SLIMES TO MOVE/ATTACK:
    var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(stopDistance: 50f), priority: 50)
        .AddCondition(new HasTargetCondition());
    AIController.AddRule(meleeRule);
    
    var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
        .AddCondition(new NoTargetCondition());
    AIController.AddRule(idleRule);
    
    AddAIBehavior((delta) => AIController.Process(delta));
}
```

### After (Configurable AI - No Forced Behavior)

```csharp
private void SetupAI()
{
    // Create AI Controller (used only when AI rules are configured in scene)
    // AI rules should be configured in the scene via AIRules property, not hardcoded here.
    AIController = new AIController();
    AIController.Entity = this;
    AIController.DebugMode = false;
    AddChild(AIController);
    
    // REMOVED: Hardcoded AI rules. Configure AI in the scene editor instead using AIRules property.
    // This allows each slime instance to have different AI behaviors without code changes.
    // If you need AI, add AIRuleData entries to the AIRules property in the scene inspector.
    
    // Legacy hardcoded rules (commented out for reference):
    // var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(stopDistance: 50f), priority: 50)
    //     .AddCondition(new HasTargetCondition());
    // AIController.AddRule(meleeRule);
    // 
    // var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
    //     .AddCondition(new NoTargetCondition());
    // AIController.AddRule(idleRule);
    
    // Add controller to AI behavior system (processes scene-configured rules)
    AddAIBehavior((delta) => AIController.Process(delta));
}
```

## Why Slime Was Moving and Attacking

### Flow Before Fix

1. **SetupAI()** added `MeleeAttackBehavior` with `HasTargetCondition`
2. **TargetController** node (in slime.tscn) automatically found player as target
3. **MeleeAttackBehavior** moved slime toward player using Navigation
4. **Weapon Area2D** node detected collision with player
5. **Attack()** method dealt damage to player

### Flow After Fix

1. **AIController** created but NO rules added
2. **TargetController** still finds targets (but no AI rules use them)
3. **NO movement behavior** - slime stays stationary
4. **Weapon Area2D** still present but won't trigger (slime doesn't reach player)
5. **Clean separation**: infrastructure exists, behavior configured in scene

## Scene Components

The `slime.tscn` scene file has these components (all preserved):

- **TargetController** (line 525) - Automatically finds targets in the Allies team
- **Navigation** (line 531) - NavigationAgent2D for pathfinding (unused without AI rules)
- **Weapon** (line 540) - Area2D for collision detection with allies
- **DefaultAbilityIds** (line 504) - Set to `["slime_melee"]` - ability is available but not used without AI

All components are present and ready. They just need AI rules to activate!

## Configuring Slime AI

To make slimes move and attack, configure AI in the scene editor:

### Step-by-Step Guide

1. **Open Scene**: Open `slime.tscn` in Godot editor
2. **Select Root Node**: Click on the root "Slime" node
3. **Find AIRules**: In Inspector, scroll to find "AIRules" property
4. **Add Rule**: Click "+" to add a new `AIRuleData` entry
5. **Configure Rule**:
   - **RuleName**: "AttackPlayer" (or any descriptive name)
   - **Priority**: 50 (higher = evaluated first)
   - **Probability**: 1.0 (100% chance to execute)
   - **BehaviorType**: Select "MeleeAttack"
   - **BehaviorParams**: Add dictionary entry: `approach_distance: 50`
6. **Add Condition**: In the "Conditions" array, add `AIConditionData`:
   - **ConditionType**: "HasTarget"
7. **Save Scene**

### Example 1: Basic Chase and Attack

```
Slime Node
└── AIRules: Array[AIRuleData]
    └── [0] "AttackPlayer"
        ├── RuleName: "AttackPlayer"
        ├── Priority: 50
        ├── Probability: 1.0
        ├── BehaviorType: MeleeAttack
        ├── BehaviorParams: {approach_distance: 50}
        └── Conditions: Array[AIConditionData]
            └── [0] HasTargetCondition
                ├── ConditionType: HasTarget
                └── ConditionParams: {}
```

Result: Slime chases and attacks player when target is found.

### Example 2: Advanced - Flee When Low Health

```
Slime Node
└── AIRules: Array[AIRuleData]
    ├── [0] "FleeWhenHurt"
    │   ├── Priority: 100
    │   ├── BehaviorType: Flee
    │   ├── BehaviorParams: {flee_distance: 400}
    │   └── Conditions: [LowHealthCondition {threshold: 0.3}]
    │
    ├── [1] "ChasePlayer"
    │   ├── Priority: 50
    │   ├── BehaviorType: MeleeAttack
    │   ├── BehaviorParams: {approach_distance: 50}
    │   └── Conditions: [HasTarget]
    │
    └── [2] "IdleWait"
        ├── Priority: 0
        ├── BehaviorType: Idle
        └── Conditions: [NoTarget]
```

Result: Slime flees when health below 30%, chases when healthy, idles when no target.

### Example 3: Patrol Pattern

```
Slime Node
└── AIRules: Array[AIRuleData]
    ├── [0] "AttackIfClose"
    │   ├── Priority: 60
    │   ├── BehaviorType: MeleeAttack
    │   ├── Conditions: [HasTarget, TargetDistance {type: "LessThan", distance: 200}]
    │
    └── [1] "WanderAround"
        ├── Priority: 20
        ├── BehaviorType: Strafe
        ├── BehaviorParams: {distance: 150, clockwise: false}
        └── Conditions: []
```

Result: Slime attacks if player gets close, otherwise circles/wanders around.

## Available AI Components

### Behaviors (7 types)

1. **Idle** - Stand still
2. **MeleeAttack** - Move toward target for melee combat
   - Params: `approach_distance` (how close to get)
3. **RangedAttack** - Use ranged ability
   - Params: `ability_index` or `ability_id`, `optimal_distance`
4. **Strafe** - Circle around target
   - Params: `distance`, `clockwise`
5. **Dodge** - Dash away from danger
   - Params: `distance`, `consume_stamina`
6. **Flee** - Run away from target
   - Params: `flee_distance`
7. **FollowPlayer** - Follow the player
   - Params: `follow_distance`

### Conditions (7 types)

1. **HasTarget** - True when target exists
2. **NoTarget** - True when no target
3. **LowHealth** - True when health below threshold
   - Params: `threshold` (0.0 to 1.0, e.g., 0.3 = 30%)
4. **LowMana** - True when mana below threshold
   - Params: `threshold`
5. **LowStamina** - True when stamina below threshold
   - Params: `threshold`
6. **TargetDistance** - True based on distance to target
   - Params: `distance_type` ("LessThan", "GreaterThan", "Between"), `distance`, `max_distance`
7. **AbilityReady** - True when ability off cooldown
   - Params: `ability_index`

## Technical Details

### How AI Controller Works

1. **Every Frame**: `AIController.Process(delta)` is called
2. **Rule Evaluation**: Rules are evaluated in priority order (highest first)
3. **Condition Check**: For each rule, ALL conditions must be true
4. **Probability**: If conditions met, roll against probability (0.0-1.0)
5. **Execute**: If passed, execute the behavior (only ONE per frame)
6. **Cooldowns**: Behaviors have cooldowns to prevent spam

### Why This Approach

**Before**: Hardcoded behavior forced on all instances
**After**: Each instance configurable independently

**Benefits**:
- **Flexibility**: Different slime types with different behaviors
- **Designer-friendly**: Configure in editor, no code changes
- **Maintainable**: AI logic separated from entity code
- **Testable**: Easy to try different configurations

## Testing Checklist

After configuring AI:

1. **Start Game**: Launch the game with slime instance
2. **Observe**:
   - ✅ Slime should move toward player (if MeleeAttack configured)
   - ✅ Slime should attack when close (weapon collision)
   - ✅ Slime should flee if low health (if configured)
   - ✅ Slime should be stationary if no AI configured
3. **Console**: Check for any errors related to AI
4. **Tweak**: Adjust priorities, probabilities, and parameters
5. **Test Again**: Iterate until behavior is correct

## Troubleshooting

### Slime Still Not Moving

**Check**:
1. Is AIRules array empty? Add rules!
2. Are conditions set correctly? (HasTarget for chase behavior)
3. Is CanMove = true? (Set by Spawned() method)
4. Is DefaultAbilityIds set? (Should be `["slime_melee"]`)

### Slime Moves But Doesn't Attack

**Check**:
1. Is Weapon Area2D configured correctly?
2. Is collision mask set to detect allies? (mask = 2)
3. Is attack animation playing? (Check AnimationPlayer)

### AI Not Responding

**Check**:
1. Is AIController.Enabled = true?
2. Are rule priorities correct? (Higher = evaluated first)
3. Are conditions too restrictive? (Try NoTarget condition)
4. Enable AIController.DebugMode = true for logs

## Related Documentation

- `EXPORTABLE_ENTITY_GUIDE.md` - Complete guide to exportable entity system
- `AI_CONTROLLER_GUIDE.md` - Detailed AI Controller documentation
- `HARDCODED_MOVEMENT_DISABLED.md` - Movement system changes

## Conclusion

The hardcoded AI has been removed from Slime to give full control over behavior through scene configuration. This makes it easy to:
- Create different slime variants (aggressive, passive, fleeing, patrol)
- Configure behavior without code changes
- Test and balance AI easily

To use slimes with AI, simply configure AIRules in the scene editor as shown in the examples above!
