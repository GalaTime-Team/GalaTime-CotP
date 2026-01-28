# AI Rule Editor Fixes and Enhanced Configuration Guide

## Overview

This document explains the fixes for AI rule editor issues and new features for entity configuration.

## Issues Fixed

### 1. AIConditionData Not Recognized in Godot Editor ✅

**Problem**: When trying to add Conditions to AIRuleData in the Godot editor, the error "Cannot get class 'AIConditionData'." appeared.

**Root Cause**: AIConditionData was defined in the same file as AIRuleData, which sometimes causes Godot's editor to have trouble recognizing nested resource classes.

**Solution**: Separated AIConditionData into its own file (`AIConditionData.cs`) for better Godot editor recognition.

**Result**: AIConditionData is now properly recognized in the Godot Inspector when adding conditions to AI rules.

### 2. EntityStats Resizable Arrays ✅

**Problem**: EntityStats used 2 separate resizable arrays (StatsNames and StatsValues) which could be accidentally modified, leading to:
- Inconsistent stat counts
- Accidentally added or removed stats
- Confusing interface

**Desired Behavior**: Fixed 9 rows (one for each stat type) that cannot be added or removed.

**Solution**: Created `FixedEntityStats` resource class with exactly 9 fixed properties:

```csharp
[GlobalClass]
public partial class FixedEntityStats : Resource
{
    [Export] public EntityStatEntry Health;
    [Export] public EntityStatEntry Mana;
    [Export] public EntityStatEntry Stamina;
    [Export] public EntityStatEntry Agility;
    [Export] public EntityStatEntry PhysicalAttack;
    [Export] public EntityStatEntry MagicalAttack;
    [Export] public EntityStatEntry PhysicalDefense;
    [Export] public EntityStatEntry MagicalDefense;
    [Export] public EntityStatEntry KnockbackResistance;
}
```

**Usage in Godot Inspector**:
```
Entity Node
├── FixedStats (FixedEntityStats)
│   ├── Health: (Value: 100)
│   ├── Mana: (Value: 50)
│   ├── Stamina: (Value: 80)
│   ├── Agility: (Value: 10)
│   ├── PhysicalAttack: (Value: 15)
│   ├── MagicalAttack: (Value: 20)
│   ├── PhysicalDefense: (Value: 5)
│   ├── MagicalDefense: (Value: 8)
│   └── KnockbackResistance: (Value: 2)
```

**Benefits**:
- ✅ Cannot accidentally add or remove stats
- ✅ All 9 stats visible by name
- ✅ Cleaner interface
- ✅ Automatic conversion to EntityStats on entity load
- ✅ Backward compatible (can still use EntityStats if preferred)

### 3. Per-Rule Ability Selection ✅

**Suggestion**: Allow AI rules to select which specific ability to use.

**Example Use Case**: Firecloak enemy should:
- Use "firecloak_fireball" when target is far away
- Use "firecloak_dash" when target is close

**Solution**: Added ability selection to AIRuleData:

```csharp
public partial class AIRuleData : Resource
{
    [Export] public AIBehaviorType BehaviorType { get; set; }
    
    // NEW: Specify ability by ID
    [Export] public string AbilityId { get; set; } = "";
    
    // NEW: Or specify by index (0-2)
    [Export] public int AbilityIndex { get; set; } = -1;
}
```

**How It Works**:
1. Set `AbilityId` to ability ID string (e.g., "firecloak_fireball")
2. AIRuleFactory automatically finds the ability in entity's ability list
3. Falls back to `AbilityIndex` if ID not found
4. Uses the specified ability for RangedAttack behavior

## Configuration Examples

### Example 1: Fixed Stats Configuration

```gdscript
# Slime Enemy
DefaultAbilityIds: ["slime_melee"]
FixedStats:
  Health: 50
  Mana: 0
  Stamina: 0
  Agility: 5
  PhysicalAttack: 10
  MagicalAttack: 0
  PhysicalDefense: 2
  MagicalDefense: 1
  KnockbackResistance: 0
```

### Example 2: Firecloak with Ability Selection

```gdscript
# Firecloak Enemy
DefaultAbilityIds: ["firecloak_fireball", "firecloak_dash"]
FixedStats:
  Health: 120
  Mana: 100
  Stamina: 50
  Agility: 15
  PhysicalAttack: 10
  MagicalAttack: 25
  PhysicalDefense: 5
  MagicalDefense: 10
  KnockbackResistance: 3

AIRules:
  # Flee when low health
  - RuleName: "Flee When Hurt"
    Priority: 100
    Probability: 1.0
    BehaviorType: Flee
    BehaviorParams: {flee_distance: 400}
    Conditions:
      - ConditionType: LowHealth
        ConditionParams: {threshold: 0.3}
  
  # Use dash attack when close (70% chance)
  - RuleName: "Dash Attack Close"
    Priority: 80
    Probability: 0.7
    BehaviorType: RangedAttack
    AbilityId: "firecloak_dash"
    BehaviorParams: {optimal_distance: 150}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "LessThan", distance: 200}
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 1}
  
  # Use fireball when far
  - RuleName: "Fireball Attack Far"
    Priority: 70
    Probability: 0.8
    BehaviorType: RangedAttack
    AbilityId: "firecloak_fireball"
    BehaviorParams: {optimal_distance: 400}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "GreaterThan", distance: 200}
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 0}
  
  # Strafe around target (50% chance)
  - RuleName: "Strafe Movement"
    Priority: 50
    Probability: 0.5
    BehaviorType: Strafe
    BehaviorParams: {optimal_distance: 300, clockwise: true}
    Conditions:
      - ConditionType: HasTarget
  
  # Idle when no target
  - RuleName: "Idle"
    Priority: 0
    BehaviorType: Idle
    Conditions:
      - ConditionType: NoTarget
```

### Example 3: Ally with Multiple Abilities

```gdscript
# Ally Character
DefaultAbilityIds: ["fireball", "firebullet", "firewave"]
FixedStats:
  Health: 150
  Mana: 200
  Stamina: 100
  Agility: 20
  PhysicalAttack: 15
  MagicalAttack: 30
  PhysicalDefense: 8
  MagicalDefense: 12
  KnockbackResistance: 5

AIRules:
  # Conserve stamina when low
  - RuleName: "Conserve Stamina"
    Priority: 90
    BehaviorType: Idle
    Conditions:
      - ConditionType: LowStamina
        ConditionParams: {threshold: 0.2}
  
  # Use firewave (powerful AoE) when ready (50% chance)
  - RuleName: "Use FireWave"
    Priority: 75
    Probability: 0.5
    BehaviorType: RangedAttack
    AbilityId: "firewave"
    BehaviorParams: {optimal_distance: 250}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 2}
  
  # Use firebullet (high damage) when ready (60% chance)
  - RuleName: "Use FireBullet"
    Priority: 70
    Probability: 0.6
    BehaviorType: RangedAttack
    AbilityId: "firebullet"
    BehaviorParams: {optimal_distance: 300}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 1}
  
  # Use fireball (basic attack) when ready (70% chance)
  - RuleName: "Use Fireball"
    Priority: 65
    Probability: 0.7
    BehaviorType: RangedAttack
    AbilityId: "fireball"
    BehaviorParams: {optimal_distance: 350}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 0}
  
  # Follow player when no enemies
  - RuleName: "Follow Player"
    Priority: 10
    BehaviorType: FollowPlayer
    BehaviorParams: {follow_distance: 120}
    Conditions:
      - ConditionType: NoTarget
  
  # Idle as fallback
  - RuleName: "Idle"
    Priority: 0
    BehaviorType: Idle
```

## Ability Selection Methods

### Method 1: By Ability ID (Recommended)

```gdscript
AbilityId: "firecloak_fireball"
```

**Pros**:
- Clear and readable
- Independent of ability order
- Self-documenting

**Cons**:
- Requires ability to be loaded in entity

### Method 2: By Ability Index

```gdscript
AbilityIndex: 0  # First ability (0-2)
```

**Pros**:
- Direct index access
- Faster lookup

**Cons**:
- Depends on ability order
- Less clear what ability is being used

### Method 3: In BehaviorParams (Legacy)

```gdscript
BehaviorParams: {ability_index: 0}
```

Still works for backward compatibility but less convenient.

## Migration Guide

### From EntityStats to FixedEntityStats

**Old Way (EntityStats)**:
```gdscript
Stats:
  StatsNames: [Health, Mana, PhysicalAttack]
  StatsValues: [100, 50, 15]
```

**New Way (FixedEntityStats)**:
```gdscript
FixedStats:
  Health: 100
  Mana: 50
  Stamina: 0
  Agility: 0
  PhysicalAttack: 15
  MagicalAttack: 0
  PhysicalDefense: 0
  MagicalDefense: 0
  KnockbackResistance: 0
```

**Note**: Can use either method. FixedStats is cleaner but EntityStats still works.

### Adding Ability Selection to Existing Rules

**Old Way (Generic)**:
```gdscript
- RuleName: "Attack"
  BehaviorType: RangedAttack
  BehaviorParams: {ability_index: 0}
```

**New Way (Specific Ability)**:
```gdscript
- RuleName: "Fireball Attack"
  BehaviorType: RangedAttack
  AbilityId: "fireball"  # Much clearer!
```

## Troubleshooting

### AIConditionData Still Not Found

1. Restart Godot editor
2. Rebuild C# project (Build → Build Solution)
3. Check that `AIConditionData.cs` file exists
4. Verify file has `[GlobalClass]` attribute

### FixedStats Not Converting

1. Ensure Entity._Ready() is being called
2. Check that FixedStats is set (not null)
3. Verify Stats property is null or will be overwritten
4. Check console for any errors during conversion

### Ability Not Found by ID

1. Verify ability ID matches exactly (case-sensitive)
2. Check that ability is in DefaultAbilityIds list
3. Ensure abilities are loaded before AI rules are set up
4. Check console for warning messages about ability lookup

## Technical Details

### File Structure
```
assets/scripts/
├── objects/
│   ├── classes/
│   │   └── entity/
│   │       ├── Entity.cs (Modified: Added FixedStats support)
│   │       ├── EntityStats.cs (Unchanged)
│   │       └── FixedEntityStats.cs (NEW)
│   └── helpers/
│       └── ai/
│           └── controller/
│               ├── AIRuleData.cs (Modified: Added AbilityId/Index)
│               ├── AIConditionData.cs (NEW: Separated from AIRuleData)
│               └── AIRuleFactory.cs (Modified: Added ability lookup)
```

### Conversion Process

1. Entity._Ready() called
2. If FixedStats set and Stats null:
   - Call FixedStats.ToEntityStats()
   - Converts 9 fixed entries to EntityStats format
   - Assigns to Stats property
3. Rest of entity initialization proceeds normally

### Ability Lookup Process

1. AIRuleFactory.CreateRule() called with AIRuleData
2. If BehaviorType is RangedAttack:
   - Check if AbilityId is set (not empty)
   - Call FindAbilityIndex(entity, abilityId)
   - Search entity.Abilities for matching ID
   - Return index if found, -1 if not
3. Fall back to AbilityIndex if ID not found
4. Fall back to BehaviorParams if index -1
5. Use determined index in RangedAttackBehavior

## Best Practices

### 1. Use FixedEntityStats for New Entities
- Cleaner interface
- Prevents accidental modifications
- All stats visible by name

### 2. Use Ability IDs for Clarity
- More readable than indices
- Self-documenting code
- Easier to maintain

### 3. Organize AI Rules by Priority
- Emergency: 100+
- Combat: 50-90
- Movement: 10-40
- Idle: 0-10

### 4. Use Probability for Variety
- 1.0 = Always execute
- 0.7-0.9 = Usually execute
- 0.5-0.6 = Sometimes execute
- Creates unpredictable behavior

### 5. Test in Godot Editor
- Set up rules in inspector
- Test with different entity configurations
- Adjust probabilities and priorities as needed

## Summary

All issues have been fixed and suggestions implemented:

✅ **AIConditionData Recognition** - Separated into own file
✅ **Fixed EntityStats** - 9 fixed, non-resizable entries  
✅ **Ability Selection** - Per-rule ability configuration

Entities can now be fully configured in the Godot editor with a clean, intuitive interface!
