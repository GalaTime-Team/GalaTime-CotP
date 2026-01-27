# AI Controller System Guide

## Overview

The AI Controller system provides a flexible, condition-based AI framework for entities in GalaTime. It allows you to create intelligent NPCs with varied behaviors that respond dynamically to different game situations.

## Architecture

The system consists of four main components:

### 1. AICondition
Base class for conditions that evaluate the entity's state.
- Returns `true` when the condition is met
- Examples: LowHealthCondition, HasTargetCondition, TargetDistanceCondition

### 2. AIBehavior
Base class for behaviors that execute actions.
- Has optional cooldown to prevent spamming
- Examples: MeleeAttackBehavior, RangedAttackBehavior, FleeBehavior

### 3. AIRule
Links conditions to behaviors with priority and probability.
- Multiple conditions can be combined (all must be true)
- Priority determines evaluation order (higher = first)
- Probability adds randomness (0-1, where 1 = always execute)

### 4. AIController
Main controller that manages rules and executes behaviors.
- Evaluates rules in priority order
- Executes first matching behavior per frame
- Respects cooldowns and entity state

## Available Conditions

### Health/Resource Conditions
- **LowHealthCondition(threshold)** - Health below percentage (default: 30%)
- **LowManaCondition(threshold)** - Mana below percentage (HumanoidCharacter only)
- **LowStaminaCondition(threshold)** - Stamina below percentage (HumanoidCharacter only)

### Target Conditions
- **NoTargetCondition()** - Entity has no target
- **HasTargetCondition()** - Entity has a target
- **TargetDistanceCondition(type, distance)** - Check distance to target
  - Types: LessThan, GreaterThan, Between

### Ability Conditions
- **AbilityReadyCondition(index)** - Check if ability is off cooldown

## Available Behaviors

### Combat Behaviors
- **MeleeAttackBehavior(stopDistance, cooldown)** - Move toward target for melee combat
- **RangedAttackBehavior(abilityIndex, strafe, optimalDistance, cooldown)** - Use ranged ability
- **StrafeBehavior(optimalDistance, clockwise, cooldown)** - Circle around target

### Defensive Behaviors
- **DodgeBehavior(distance, consumeStamina, staminaCost, cooldown)** - Dodge away from target
- **FleeBehavior(fleeDistance, cooldown)** - Run away from target

### Non-Combat Behaviors
- **FollowPlayerBehavior(followDistance, cooldown)** - Follow player character
- **IdleBehavior(cooldown)** - Stand still

## Basic Usage

### Example 1: Simple Melee Enemy

```csharp
public override void _Ready()
{
    base._Ready();
    
    // Create AI Controller
    var aiController = new AIController();
    aiController.Entity = this;
    AddChild(aiController);
    
    // Rule 1: Melee attack when has target (high priority)
    var meleeRule = AIController.CreateRule(
        "MeleeAttack",
        new HasTargetCondition(),
        new MeleeAttackBehavior(stopDistance: 50f),
        priority: 10
    );
    aiController.AddRule(meleeRule);
    
    // Rule 2: Idle when no target (low priority)
    var idleRule = AIController.CreateRule(
        "Idle",
        new NoTargetCondition(),
        new IdleBehavior(),
        priority: 0
    );
    aiController.AddRule(idleRule);
    
    // Add controller to AI behavior system
    AddAIBehavior((delta) => aiController.Process(delta));
}
```

### Example 2: Ranged Enemy with Dodge

```csharp
public override void _Ready()
{
    base._Ready();
    
    var aiController = new AIController();
    aiController.Entity = this;
    AddChild(aiController);
    
    // Rule 1: Flee when low health (highest priority)
    var fleeRule = new AIRule("FleeWhenLowHealth", new FleeBehavior(400f), priority: 100)
        .AddCondition(new LowHealthCondition(0.3f))
        .AddCondition(new HasTargetCondition());
    aiController.AddRule(fleeRule);
    
    // Rule 2: Dodge if target too close (80% probability)
    var dodgeRule = new AIRule("DodgeWhenClose", new DodgeBehavior(200f, false), priority: 50, probability: 0.8f)
        .AddCondition(new HasTargetCondition())
        .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 100f));
    aiController.AddRule(dodgeRule);
    
    // Rule 3: Use ranged attack when ability ready
    var rangedRule = new AIRule("RangedAttack", new RangedAttackBehavior(0, true, 300f, 2f), priority: 20)
        .AddCondition(new HasTargetCondition())
        .AddCondition(new AbilityReadyCondition(0))
        .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 150f));
    aiController.AddRule(rangedRule);
    
    // Rule 4: Strafe around target
    var strafeRule = AIController.CreateRule(
        "Strafe",
        new HasTargetCondition(),
        new StrafeBehavior(250f, true),
        priority: 10
    );
    aiController.AddRule(strafeRule);
    
    AddAIBehavior((delta) => aiController.Process(delta));
}
```

### Example 3: Ally Character

```csharp
public override void _Ready()
{
    base._Ready();
    
    var aiController = new AIController();
    aiController.Entity = this;
    AddChild(aiController);
    
    // Rule 1: Use ability when available and has target
    var abilityRule = new AIRule("UseAbility", new RangedAttackBehavior(0, true, 300f, 3f), priority: 50, probability: 0.7f)
        .AddCondition(new HasTargetCondition())
        .AddCondition(new AbilityReadyCondition(0));
    aiController.AddRule(abilityRule);
    
    // Rule 2: Melee attack when has target
    var meleeRule = AIController.CreateRule(
        "MeleeAttack",
        new HasTargetCondition(),
        new MeleeAttackBehavior(60f),
        priority: 30
    );
    aiController.AddRule(meleeRule);
    
    // Rule 3: Follow player when no target
    var followRule = AIController.CreateRule(
        "FollowPlayer",
        new NoTargetCondition(),
        new FollowPlayerBehavior(100f),
        priority: 10
    );
    aiController.AddRule(followRule);
    
    AddAIBehavior((delta) => aiController.Process(delta));
}
```

## Adding Randomness and Variety

### Method 1: Probability on Rules
```csharp
// 70% chance to dodge when conditions met
var dodgeRule = new AIRule("Dodge", new DodgeBehavior(), priority: 50, probability: 0.7f);
```

### Method 2: Multiple Behaviors at Same Priority
```csharp
// First matching rule wins, creating variety
var strafe1 = new AIRule("StrafeClockwise", new StrafeBehavior(250f, true), priority: 10, probability: 0.5f);
var strafe2 = new AIRule("StrafeCounterClockwise", new StrafeBehavior(250f, false), priority: 10, probability: 0.5f);
```

### Method 3: Cooldowns
```csharp
// Can only dodge every 3 seconds
var dodgeBehavior = new DodgeBehavior(200f, false, 0f, cooldown: 3f);
```

## Creating Custom Conditions

```csharp
public class CustomCondition : AICondition
{
    public CustomCondition() : base("MyCondition") { }
    
    public override bool Evaluate(Entity entity)
    {
        // Your custom logic here
        return true; // or false
    }
}
```

## Creating Custom Behaviors

```csharp
public class CustomBehavior : AIBehavior
{
    public CustomBehavior(float cooldown = 0f) : base("MyBehavior", cooldown) { }
    
    protected override void OnExecute(Entity entity, double delta)
    {
        // Your custom behavior logic here
        entity.Body.Velocity = Vector2.Zero;
    }
}
```

## Best Practices

1. **Use Priority Wisely**: Higher priority = evaluated first
   - Emergency behaviors (flee, heal): 100+
   - Combat abilities: 50-80
   - Movement behaviors: 10-40
   - Idle/default: 0-10

2. **Add Probability for Variety**: Don't make AI too predictable
   - Critical behaviors: 1.0 (always)
   - Combat choices: 0.5-0.8
   - Optional moves: 0.3-0.5

3. **Use Cooldowns**: Prevent behavior spam
   - Dodge: 2-3 seconds
   - Abilities: Match ability cooldown
   - Movement changes: 0.5-1 second

4. **Combine Conditions**: Create complex rules
   ```csharp
   rule.AddCondition(new LowHealthCondition(0.3f))
       .AddCondition(new HasTargetCondition())
       .AddCondition(new AbilityReadyCondition(2));
   ```

5. **Debug Mode**: Enable for testing
   ```csharp
   aiController.DebugMode = true; // Logs rule executions
   ```

## Performance Tips

- Keep number of rules reasonable (< 20 per entity)
- Use cooldowns to reduce processing frequency
- Combine related conditions into single rules
- Only enable AIController when needed

## Integration with Existing Systems

The AI Controller works alongside the existing AIBehaviors system:

```csharp
// Old way (still works)
AddAIBehavior((delta) => {
    // Custom behavior
});

// New way (more flexible)
var aiController = new AIController();
aiController.AddRule(myRule);
AddAIBehavior((delta) => aiController.Process(delta));

// Both can coexist!
```

## Debugging

Enable debug mode to see which rules are executing:

```csharp
aiController.DebugMode = true;
```

Console output:
```
[AIController] Executing rule 'FleeWhenLowHealth' with behavior 'Flee'
[AIController] Executing rule 'RangedAttack' with behavior 'RangedAttack0'
```

## Summary

The AI Controller system provides:
- ✅ Condition-based AI (health, mana, stamina, targets, distances)
- ✅ Multiple behaviors (melee, ranged, dodge, flee, follow, strafe)
- ✅ Distinct movement via probability and randomness
- ✅ Easy to extend with custom conditions/behaviors
- ✅ Compatible with existing AI system
- ✅ Flexible and maintainable

This creates smarter, more varied NPCs that respond intelligently to game situations!
