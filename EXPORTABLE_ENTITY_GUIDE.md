# Exportable AI and Entity System - Complete Guide

## Overview

The GalaTime entity system now supports **full configuration in the Godot editor** without writing code. You can configure:
- **AI Rules** (conditions and behaviors)
- **Abilities** (ranged attacks)
- **Stats** (health, attack, defense, etc.)
- **Elements** (fire, water, earth, etc.)
- **Team**, speed, XP drops, and other properties

This means you can create different enemy/ally variants by simply changing values in the Godot editor!

## Quick Start

### Configure an Entity in 3 Steps

1. **Add the script** to your CharacterBody2D node
2. **Set properties** in the Inspector
3. **Run the game** - everything works automatically!

No code needed!

## Exportable Properties

### 1. Abilities

**Property:** `DefaultAbilityIds`
**Type:** `Array<String>`
**Description:** IDs of abilities to load automatically from `abilities.json`

**Example:**
```gdscript
DefaultAbilityIds = ["fireball", "firebullet", "firewave"]
```

This automatically loads 3 abilities:
- Slot 0: Fireball
- Slot 1: Fire Bullet
- Slot 2: Fire Wave

**Available Ability IDs:**
- Player/Ally: `fireball`, `blue_fireball`, `flamethrower`, `firewave`, `bluefire`, `firebullet`
- Enemy: `slime_melee`, `firecloak_fireball`, `firecloak_dash`, `rockant_dig`, `rockant_melee`

### 2. AI Rules

**Property:** `AIRules`
**Type:** `Array<AIRuleData>`
**Description:** AI rules that define entity behavior

**Example AI Rule Structure:**
```
RuleName: "Flee When Low Health"
Priority: 100
Probability: 1.0
Enabled: true
BehaviorType: Flee
BehaviorParams: 
  flee_distance: 400
Conditions:
  - ConditionType: LowHealth
    ConditionParams:
      threshold: 0.25
  - ConditionType: HasTarget
```

#### Priority Ranges
- **100+**: Emergency (flee, heal)
- **50-90**: Combat (attack, abilities)
- **10-40**: Movement (strafe, approach)
- **0-10**: Default (follow, idle)

#### Probability
- **1.0**: Always execute when conditions met
- **0.7**: 70% chance (adds variety)
- **0.5**: 50% chance (random behavior)

### 3. Stats

**Property:** `Stats`
**Type:** `EntityStats`
**Description:** Entity statistics (health, attack, defense, etc.)

Fully exportable in the editor. See EntityStats documentation for details.

### 4. Element

**Property:** `Element`
**Type:** `GalatimeElement`
**Description:** Entity's elemental affinity

Affects damage calculation and elemental interactions.

### 5. Other Properties

**Already Exportable:**
- `Team` - Which team (Player/Enemy/Neutral)
- `DroppedXp` - XP dropped on death
- `Speed` - Movement speed
- `Timeout` - Despawn time after death
- `Invincible` - Cannot take damage
- `AutoSetupAI` - Automatically setup AI from rules
- `AIDebugMode` - Enable debug logging

## AI Behavior Types

### 1. Idle
Do nothing.

**Parameters:**
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: Idle
BehaviorParams:
  cooldown: 0
```

### 2. MeleeAttack
Move toward target for melee combat.

**Parameters:**
- `stop_distance` (float): Stop this far from target
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: MeleeAttack
BehaviorParams:
  stop_distance: 60
  cooldown: 0
```

### 3. RangedAttack
Use ability on target while maintaining distance.

**Parameters:**
- `ability_index` (int): Which ability slot (0-2)
- `strafe` (bool): Strafe while attacking
- `optimal_distance` (float): Preferred distance to target
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: RangedAttack
BehaviorParams:
  ability_index: 0
  strafe: true
  optimal_distance: 300
  cooldown: 1.5
```

### 4. Strafe
Circle around target.

**Parameters:**
- `optimal_distance` (float): Preferred distance to target
- `clockwise` (bool): Direction of strafe
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: Strafe
BehaviorParams:
  optimal_distance: 250
  clockwise: true
  cooldown: 0
```

### 5. Dodge
Dash away from target.

**Parameters:**
- `dodge_distance` (float): Distance to dodge
- `consume_stamina` (bool): Use stamina (HumanoidCharacter only)
- `stamina_cost` (float): Stamina cost
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: Dodge
BehaviorParams:
  dodge_distance: 200
  consume_stamina: false
  stamina_cost: 0
  cooldown: 3
```

### 6. Flee
Run away from target.

**Parameters:**
- `flee_distance` (float): Distance to flee
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: Flee
BehaviorParams:
  flee_distance: 400
  cooldown: 2
```

### 7. FollowPlayer
Follow the player character.

**Parameters:**
- `follow_distance` (float): Stay this close to player
- `cooldown` (float): Cooldown in seconds

**Example:**
```
BehaviorType: FollowPlayer
BehaviorParams:
  follow_distance: 120
  cooldown: 0
```

## AI Condition Types

### 1. HasTarget
Entity has a target.

**Parameters:** None

**Example:**
```
ConditionType: HasTarget
```

### 2. NoTarget
Entity has no target.

**Parameters:** None

**Example:**
```
ConditionType: NoTarget
```

### 3. LowHealth
Health below threshold.

**Parameters:**
- `threshold` (float): Health percentage (0-1)

**Example:**
```
ConditionType: LowHealth
ConditionParams:
  threshold: 0.3  # Below 30%
```

### 4. LowMana
Mana below threshold (HumanoidCharacter only).

**Parameters:**
- `threshold` (float): Mana percentage (0-1)

**Example:**
```
ConditionType: LowMana
ConditionParams:
  threshold: 0.25  # Below 25%
```

### 5. LowStamina
Stamina below threshold (HumanoidCharacter only).

**Parameters:**
- `threshold` (float): Stamina percentage (0-1)

**Example:**
```
ConditionType: LowStamina
ConditionParams:
  threshold: 0.3  # Below 30%
```

### 6. TargetDistance
Check distance to target.

**Parameters:**
- `distance_type` (string): "LessThan", "GreaterThan", or "Between"
- `distance` (float): Distance in pixels
- `distance2` (float): Max distance (for "Between" type)

**Examples:**
```
# Less than 100 pixels
ConditionType: TargetDistance
ConditionParams:
  distance_type: "LessThan"
  distance: 100
```

```
# Greater than 300 pixels
ConditionType: TargetDistance
ConditionParams:
  distance_type: "GreaterThan"
  distance: 300
```

```
# Between 100 and 400 pixels
ConditionType: TargetDistance
ConditionParams:
  distance_type: "Between"
  distance: 100
  distance2: 400
```

### 7. AbilityReady
Check if ability is off cooldown.

**Parameters:**
- `ability_index` (int): Which ability slot (0-2)

**Example:**
```
ConditionType: AbilityReady
ConditionParams:
  ability_index: 0
```

## Complete Examples

### Example 1: Basic Melee Enemy

```gdscript
# Slime configuration
DefaultAbilityIds = ["slime_melee"]

AIRules:
  # Priority 50: Attack when has target
  - RuleName: "Melee Attack"
    Priority: 50
    Probability: 1.0
    BehaviorType: MeleeAttack
    BehaviorParams:
      stop_distance: 50
      cooldown: 0
    Conditions:
      - ConditionType: HasTarget
  
  # Priority 0: Idle when no target
  - RuleName: "Idle"
    Priority: 0
    Probability: 1.0
    BehaviorType: Idle
    BehaviorParams:
      cooldown: 0
    Conditions:
      - ConditionType: NoTarget
```

### Example 2: Ranged Enemy with Strafe

```gdscript
# ShootingBuddy configuration
DefaultAbilityIds = ["firecloak_fireball"]

AIRules:
  # Priority 60: Use fireball when ready
  - RuleName: "Fireball Attack"
    Priority: 60
    Probability: 0.8
    BehaviorType: RangedAttack
    BehaviorParams:
      ability_index: 0
      strafe: true
      optimal_distance: 300
      cooldown: 1.5
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams:
          ability_index: 0
  
  # Priority 30: Strafe around target
  - RuleName: "Strafe"
    Priority: 30
    Probability: 0.6
    BehaviorType: Strafe
    BehaviorParams:
      optimal_distance: 300
      clockwise: true
      cooldown: 0
    Conditions:
      - ConditionType: HasTarget
  
  # Priority 0: Idle when no target
  - RuleName: "Idle"
    Priority: 0
    Probability: 1.0
    BehaviorType: Idle
    Conditions:
      - ConditionType: NoTarget
```

### Example 3: Advanced Enemy with Flee

```gdscript
# Advanced enemy configuration
DefaultAbilityIds = ["firecloak_fireball", "firecloak_dash"]

AIRules:
  # Priority 100: Flee when low health
  - RuleName: "Flee When Low Health"
    Priority: 100
    Probability: 1.0
    BehaviorType: Flee
    BehaviorParams:
      flee_distance: 400
      cooldown: 0
    Conditions:
      - ConditionType: LowHealth
        ConditionParams:
          threshold: 0.25
      - ConditionType: HasTarget
  
  # Priority 80: Dodge when close
  - RuleName: "Dodge When Close"
    Priority: 80
    Probability: 0.7
    BehaviorType: Dodge
    BehaviorParams:
      dodge_distance: 200
      consume_stamina: false
      cooldown: 3
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams:
          distance_type: "LessThan"
          distance: 120
  
  # Priority 60: Use fireball when ready and at good distance
  - RuleName: "Use Fireball"
    Priority: 60
    Probability: 1.0
    BehaviorType: RangedAttack
    BehaviorParams:
      ability_index: 0
      strafe: true
      optimal_distance: 300
      cooldown: 1.5
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams:
          ability_index: 0
      - ConditionType: TargetDistance
        ConditionParams:
          distance_type: "GreaterThan"
          distance: 150
  
  # Priority 50: Use dash when ready
  - RuleName: "Use Dash"
    Priority: 50
    Probability: 0.8
    BehaviorType: RangedAttack
    BehaviorParams:
      ability_index: 1
      strafe: false
      optimal_distance: 250
      cooldown: 2
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams:
          ability_index: 1
  
  # Priority 30: Strafe around target
  - RuleName: "Strafe"
    Priority: 30
    Probability: 0.5
    BehaviorType: Strafe
    BehaviorParams:
      optimal_distance: 250
      clockwise: true
      cooldown: 0
    Conditions:
      - ConditionType: HasTarget
  
  # Priority 0: Idle
  - RuleName: "Idle"
    Priority: 0
    Probability: 1.0
    BehaviorType: Idle
    Conditions:
      - ConditionType: NoTarget
```

### Example 4: Ally Character

```gdscript
# Ally configuration
DefaultAbilityIds = ["fireball", "firebullet", "firewave"]

AIRules:
  # Priority 90: Conserve stamina when low
  - RuleName: "Conserve Stamina"
    Priority: 90
    Probability: 1.0
    BehaviorType: Flee
    BehaviorParams:
      flee_distance: 300
      cooldown: 2
    Conditions:
      - ConditionType: LowStamina
        ConditionParams:
          threshold: 0.3
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams:
          distance_type: "LessThan"
          distance: 150
  
  # Priority 70: Use ability 0
  - RuleName: "Use Ability 0"
    Priority: 70
    Probability: 0.7
    BehaviorType: RangedAttack
    BehaviorParams:
      ability_index: 0
      strafe: true
      optimal_distance: 300
      cooldown: 1
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams:
          ability_index: 0
  
  # Priority 65: Use ability 1
  - RuleName: "Use Ability 1"
    Priority: 65
    Probability: 0.6
    BehaviorType: RangedAttack
    BehaviorParams:
      ability_index: 1
      strafe: true
      optimal_distance: 250
      cooldown: 1.5
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams:
          ability_index: 1
  
  # Priority 60: Use ability 2
  - RuleName: "Use Ability 2"
    Priority: 60
    Probability: 0.5
    BehaviorType: RangedAttack
    BehaviorParams:
      ability_index: 2
      strafe: true
      optimal_distance: 280
      cooldown: 2
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams:
          ability_index: 2
  
  # Priority 10: Follow player when no enemies
  - RuleName: "Follow Player"
    Priority: 10
    Probability: 1.0
    BehaviorType: FollowPlayer
    BehaviorParams:
      follow_distance: 120
      cooldown: 0
    Conditions:
      - ConditionType: NoTarget
  
  # Priority 0: Idle
  - RuleName: "Idle"
    Priority: 0
    Probability: 1.0
    BehaviorType: Idle
```

## Tips and Best Practices

### Creating Varied Behavior

Use probability to make enemies unpredictable:
```
# 50% strafe left, 50% strafe right
- RuleName: "Strafe Left"
  Priority: 30
  Probability: 0.5
  BehaviorType: Strafe
  BehaviorParams:
    clockwise: true

- RuleName: "Strafe Right"
  Priority: 30
  Probability: 0.5
  BehaviorType: Strafe
  BehaviorParams:
    clockwise: false
```

### Cooldowns

Use cooldowns to pace actions naturally:
```
# Dodge at most every 3 seconds
BehaviorParams:
  cooldown: 3
```

### Multiple Conditions

All conditions must be true for rule to execute:
```
Conditions:
  - ConditionType: HasTarget
  - ConditionType: LowHealth
    ConditionParams:
      threshold: 0.3
  - ConditionType: TargetDistance
    ConditionParams:
      distance_type: "LessThan"
      distance: 150
```

### Priority Order

Higher priority rules are evaluated first:
1. Emergency behaviors (100+)
2. Combat behaviors (50-90)
3. Movement behaviors (10-40)
4. Default behaviors (0-10)

### Disabling Auto Setup

If you want to manually setup AI in code:
```
AutoSetupAI = false
```

Then in your script:
```csharp
public override void _Ready()
{
    base._Ready();
    // Your custom AI setup here
}
```

## Debugging

### Enable Debug Mode

```
AIDebugMode = true
```

This logs:
- Which rules are evaluated
- Which rules execute
- Why rules don't execute

### Check Console Output

When debug mode is enabled, you'll see:
```
[AIController] Rule 'Flee When Low Health' executed (priority: 100)
[AIController] Rule 'Dodge When Close' conditions not met
[AIController] Rule 'Use Fireball' executed (priority: 60)
```

## Migration from Code-Based AI

### Before (Code)
```csharp
public partial class MyEnemy : Entity
{
    public AIController AIController;
    
    public override void _Ready()
    {
        base._Ready();
        SetupAI();
    }
    
    private void SetupAI()
    {
        AIController = new AIController();
        AIController.Entity = this;
        AddChild(AIController);
        
        var fleeRule = new AIRule("Flee", new FleeBehavior(400f), 100)
            .AddCondition(new LowHealthCondition(0.25f));
        AIController.AddRule(fleeRule);
        
        AddAIBehavior((delta) => AIController.Process(delta));
    }
}
```

### After (Editor)
```csharp
public partial class MyEnemy : Entity
{
    // Nothing needed - configure in editor!
}
```

**In Godot Inspector:**
```
AIRules:
  - RuleName: "Flee"
    Priority: 100
    BehaviorType: Flee
    BehaviorParams:
      flee_distance: 400
    Conditions:
      - ConditionType: LowHealth
        ConditionParams:
          threshold: 0.25
```

## Summary

### What's Exportable

✅ **AI Rules** - Fully configurable in editor
✅ **Abilities** - By ID from abilities.json
✅ **Stats** - EntityStats resource
✅ **Element** - GalatimeElement
✅ **All base properties** - Team, speed, XP, etc.

### Benefits

- ✅ No code needed for variants
- ✅ Quick iteration
- ✅ Designer-friendly
- ✅ Easy to balance
- ✅ Reusable configurations
- ✅ Backward compatible

### Result

Create infinite enemy/ally variations by simply changing values in the Godot editor!
