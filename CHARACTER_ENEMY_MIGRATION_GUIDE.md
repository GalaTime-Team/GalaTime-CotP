# Character and Enemy Migration to New Systems - Implementation Guide

## Overview

All characters and enemies in GalaTime have been updated to use the new AI Controller and Ability systems. This document explains the changes and how to work with the updated entities.

## Systems Used

### 1. AI Controller System
- **Condition-based AI** - Entities make decisions based on game state
- **Priority rules** - Higher priority rules evaluated first
- **Probability** - Adds randomness for varied behavior
- **Behaviors** - Reusable actions (melee, ranged, dodge, flee, follow, etc.)

### 2. Centralized Ability System
- **JSON-based** - All abilities defined in `abilities.json`
- **Consistent properties** - Damage, range, cooldown, costs, etc.
- **Universal** - Works for all entity types (player, ally, enemy)

## Updated Entities

### Enemies

#### Slime
**Type:** Basic melee enemy

**AI Setup:**
```csharp
AIController = new AIController();
AIController.Entity = this;
AddChild(AIController);

// Load slime melee ability
var meleeAbility = GalatimeGlobals.GetAbilityById("slime_melee");
AddAbility(meleeAbility, 0);

// Melee attack when has target (priority 50)
var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(stopDistance: 50f), priority: 50)
    .AddCondition(new HasTargetCondition());
AIController.AddRule(meleeRule);

// Idle when no target (priority 0)
var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
    .AddCondition(new NoTargetCondition());
AIController.AddRule(idleRule);
```

**Behavior:**
- Approaches and attacks targets
- Uses slime_melee ability (10 damage, 60 range)
- Idles when no target

#### ShootingBuddy
**Type:** Ranged enemy

**AI Setup:**
```csharp
AIController = new AIController();
AIController.Entity = this;
AddChild(AIController);

// Strafe at range when has target (priority 30, 60% probability)
var strafeRule = new AIRule("Strafe", new StrafeBehavior(optimalDistance: 300f, clockwise: true), priority: 30, probability: 0.6f)
    .AddCondition(new HasTargetCondition());
AIController.AddRule(strafeRule);

// Idle when no target
var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
    .AddCondition(new NoTargetCondition());
AIController.AddRule(idleRule);
```

**Behavior:**
- Shoots projectiles using timer (existing system)
- Strafes around target at 300 range
- Idles when no target

#### Firecloak
**Type:** Advanced enemy with ranged and dash attacks

**AI Setup:**
```csharp
AIController = new AIController();
AIController.Entity = this;
AddChild(AIController);

// Load abilities
var fireballAbility = GalatimeGlobals.GetAbilityById("firecloak_fireball");
AddAbility(fireballAbility, 0);

var dashAbility = GalatimeGlobals.GetAbilityById("firecloak_dash");
AddAbility(dashAbility, 1);

// Strafe around target (priority 50, 60% probability)
var strafeRule = new AIRule("Strafe", new StrafeBehavior(optimalDistance: 250f, clockwise: true), priority: 50, probability: 0.6f)
    .AddCondition(new HasTargetCondition())
    .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.Between, 100f, 400f));
AIController.AddRule(strafeRule);

// Approach if too far (priority 30)
var approachRule = new AIRule("Approach", new MeleeAttackBehavior(stopDistance: 250f), priority: 30)
    .AddCondition(new HasTargetCondition())
    .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 400f));
AIController.AddRule(approachRule);
```

**Behavior:**
- Uses firecloak_fireball (15 damage, 700 range)
- Uses firecloak_dash (25 damage, 500 range)
- Strafes at medium range
- Approaches if too far
- Keeps AttackSwitcher for complex attack patterns

#### RockAnt
**Type:** Underground digger with melee

**AI Setup:**
```csharp
AIController = new AIController();
AIController.Entity = this;
AddChild(AIController);

// Load abilities
var digAbility = GalatimeGlobals.GetAbilityById("rockant_dig");
AddAbility(digAbility, 0);

var meleeAbility = GalatimeGlobals.GetAbilityById("rockant_melee");
AddAbility(meleeAbility, 1);

// Melee when close (priority 50)
var meleeRule = new AIRule("MeleeAttack", new MeleeAttackBehavior(stopDistance: 80f), priority: 50)
    .AddCondition(new HasTargetCondition())
    .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 150f));
AIController.AddRule(meleeRule);

// Approach if too far (priority 40)
var approachRule = new AIRule("Approach", new MeleeAttackBehavior(stopDistance: 100f), priority: 40)
    .AddCondition(new HasTargetCondition())
    .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 150f));
AIController.AddRule(approachRule);
```

**Behavior:**
- Uses rockant_dig (20 damage, 300 range)
- Uses rockant_melee (12 damage, 80 range)
- Melee attacks when close
- Approaches when far
- Keeps AttackSwitcher for dig/melee coordination

### Allies

#### TestCharacter (includes Arthur - main player character)
**Type:** Playable/AI-controlled ally

**AI Setup (Only active when NOT possessed):**
```csharp
AIController = new AIController();
AIController.Entity = this;
AIController.DebugMode = false;
AIController.Enabled = !Possessed; // Disable if possessed
AddChild(AIController);

// Conserve stamina when low (priority 90)
var conserveStaminaRule = new AIRule("ConserveStamina", new FleeBehavior(300f, cooldown: 2f), priority: 90)
    .AddCondition(new LowStaminaCondition(0.3f))
    .AddCondition(new HasTargetCondition())
    .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 150f));
AIController.AddRule(conserveStaminaRule);

// Use abilities with varying probabilities
var ability0Rule = new AIRule("UseAbility0", new RangedAttackBehavior(0, true, 300f, cooldown: 1f), priority: 70, probability: 0.7f)
    .AddCondition(new HasTargetCondition())
    .AddCondition(new AbilityReadyCondition(0))
    .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.GreaterThan, 200f));
AIController.AddRule(ability0Rule);

// Follow player when no enemies (priority 10)
var followRule = new AIRule("FollowPlayer", new FollowPlayerBehavior(120f), priority: 10)
    .AddCondition(new NoTargetCondition());
AIController.AddRule(followRule);
```

**Behavior:**
- **When possessed (player-controlled)**: Works exactly as before, AI disabled
- **When not possessed (AI-controlled)**:
  - Conserves stamina when low
  - Uses abilities intelligently (70%, 60%, 50% probabilities)
  - Follows player when no enemies
  - Uses melee and ranged combat strategies
- **Arthur specifically**: Main character, fully functional with player control

**Important:** The Possessed property now controls AI:
```csharp
public bool Possessed
{
    get => possessed;
    set
    {
        possessed = value;
        if (value) AttackTimer.Stop();
        // Disable AI Controller when possessed
        if (AIController != null) AIController.Enabled = !value;
    }
}
```

## Integration Patterns

### Adding AI to New Enemy

```csharp
public partial class NewEnemy : Entity
{
    public AIController AIController;
    public TargetController TargetController;
    
    public override void _Ready()
    {
        base._Ready();
        
        Body = this;
        TargetController = GetNode<TargetController>("TargetController");
        
        SetupAI();
    }
    
    private void SetupAI()
    {
        // Create controller
        AIController = new AIController();
        AIController.Entity = this;
        AddChild(AIController);
        
        // Load abilities
        var ability = GalatimeGlobals.GetAbilityById("enemy_ability_id");
        if (ability != null)
        {
            AddAbility(ability, 0);
        }
        
        // Add rules (highest priority first)
        var attackRule = new AIRule("Attack", new MeleeAttackBehavior(50f), priority: 50)
            .AddCondition(new HasTargetCondition());
        AIController.AddRule(attackRule);
        
        var idleRule = new AIRule("Idle", new IdleBehavior(), priority: 0)
            .AddCondition(new NoTargetCondition());
        AIController.AddRule(idleRule);
        
        // Integrate with entity AI
        AddAIBehavior((delta) => AIController.Process(delta));
    }
}
```

### Adding AI to New Ally

```csharp
public partial class NewAlly : HumanoidCharacter
{
    public AIController AIController;
    public TargetController TargetController;
    
    private bool possessed;
    public bool Possessed
    {
        get => possessed;
        set
        {
            possessed = value;
            if (AIController != null) AIController.Enabled = !value;
        }
    }
    
    public override void _Ready()
    {
        base._Ready();
        
        Body = this;
        TargetController = GetNode<TargetController>("TargetController");
        
        SetupAI();
    }
    
    private void SetupAI()
    {
        AIController = new AIController();
        AIController.Entity = this;
        AIController.Enabled = !Possessed;
        AddChild(AIController);
        
        // Add rules for ally behavior
        var combatRule = new AIRule("Combat", new RangedAttackBehavior(0), priority: 70)
            .AddCondition(new HasTargetCondition())
            .AddCondition(new AbilityReadyCondition(0));
        AIController.AddRule(combatRule);
        
        var followRule = new AIRule("Follow", new FollowPlayerBehavior(100f), priority: 10)
            .AddCondition(new NoTargetCondition());
        AIController.AddRule(followRule);
        
        // Only active when not possessed
        AddAIBehavior((delta) => {
            if (!Possessed && AIController != null)
            {
                AIController.Process(delta);
            }
        });
    }
}
```

## Backward Compatibility

### Existing Systems Preserved

1. **AttackSwitcher** - Still works for complex attack patterns (Firecloak, RockAnt)
2. **Timer-based systems** - ShootingBuddy still uses timer for projectiles
3. **Manual movement logic** - Slime keeps existing Move() method
4. **Possession system** - TestCharacter possession works exactly as before

### Adding to Existing Code

The AI Controller is **additive**, not replacement:
- Existing behaviors continue to work
- AI Controller adds intelligent decision-making
- Both systems can coexist

Example:
```csharp
// Existing behavior (still works)
AddAIBehavior(CustomBehavior);

// New AI Controller (adds on top)
AddAIBehavior((delta) => AIController.Process(delta));
```

## Available AI Components

### Conditions

- `HasTargetCondition()` - Entity has a target
- `NoTargetCondition()` - Entity has no target
- `LowHealthCondition(threshold)` - Health below %
- `LowManaCondition(threshold)` - Mana below % (HumanoidCharacter)
- `LowStaminaCondition(threshold)` - Stamina below % (HumanoidCharacter)
- `TargetDistanceCondition(type, distance)` - Distance checks
- `AbilityReadyCondition(index)` - Ability off cooldown

### Behaviors

- `MeleeAttackBehavior(stopDistance, cooldown)` - Move to melee range
- `RangedAttackBehavior(abilityIndex, strafe, optimalDistance, cooldown)` - Use abilities
- `StrafeBehavior(optimalDistance, clockwise, cooldown)` - Circle target
- `DodgeBehavior(distance, consumeStamina, staminaCost, cooldown)` - Dodge away
- `FleeBehavior(fleeDistance, cooldown)` - Run away
- `FollowPlayerBehavior(followDistance, cooldown)` - Follow player
- `IdleBehavior(cooldown)` - Do nothing

## Testing Checklist

### Arthur (Main Character)
- [ ] Player can control Arthur normally
- [ ] Movement, combat, abilities work
- [ ] Possession system works
- [ ] No AI interference when controlled

### Other Allies (TestCharacter instances)
- [ ] AI works when not possessed
- [ ] Uses abilities intelligently
- [ ] Follows player when no enemies
- [ ] AI disables when possessed
- [ ] AI re-enables when unpossessed

### Enemies
- [ ] Slime attacks in melee
- [ ] ShootingBuddy shoots and strafes
- [ ] Firecloak uses fireball and dash
- [ ] RockAnt uses dig and melee
- [ ] All enemies respond to player presence
- [ ] Behavior is varied and intelligent

## Troubleshooting

### AI Not Working

**Check:**
1. Is AIController enabled? `AIController.Enabled = true`
2. Is entity's AI disabled? `entity.DisableAI = false`
3. Are conditions being met?
4. Is behavior on cooldown?

**Debug:**
```csharp
AIController.DebugMode = true; // Logs rule executions
```

### Ally AI When Possessed

**Issue:** AI interfering with player control

**Solution:**
```csharp
public bool Possessed
{
    set
    {
        possessed = value;
        if (AIController != null) AIController.Enabled = !value;
    }
}
```

### Abilities Not Working

**Check:**
1. Is ability loaded? `ability != null` after `GetAbilityById()`
2. Is ability in abilities.json?
3. Is ability ID correct?

## Summary

All characters and enemies now use:
- ✅ AI Controller for intelligent behavior
- ✅ Centralized ability system (JSON-based)
- ✅ Condition-based decision making
- ✅ Priority and probability for variety
- ✅ Backward compatibility maintained

**Arthur (main character)** works exactly as before when player-controlled, with enhanced AI when not controlled.

**Result:** Smarter, more varied NPCs with easier to configure behavior!
