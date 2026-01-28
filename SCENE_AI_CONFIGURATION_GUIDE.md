# Scene AI Configuration Guide

## Overview

This guide documents the AI configuration added to entity scene files. All specified entities now have abilities configured and are ready for AI Controller integration.

## Configured Entities

### 1. Slime (slime.tscn)

**File**: `assets/objects/enemy/slime.tscn`
**Type**: Simple Melee Enemy

**Configuration**:
```gdscript
DefaultAbilityIds = Array[String](["slime_melee"])
```

**Ability Details**:
- **slime_melee**: Physical attack, 10 damage, 60 range
- Simple melee combat ability
- Works with existing slime movement AI

**Suggested AI Rules** (can be added in Godot editor):
```gdscript
AIRules:
  - RuleName: "Melee Attack"
    Priority: 50
    Probability: 1.0
    BehaviorType: MeleeAttack
    BehaviorParams: {}
    Conditions:
      - ConditionType: HasTarget
        ConditionParams: {}
  
  - RuleName: "Idle"
    Priority: 0
    BehaviorType: Idle
    Conditions:
      - ConditionType: NoTarget
```

**Behavior**: Slime will automatically load its melee ability and can use AI Controller for intelligent combat decisions.

---

### 2. RockAnt (RockAnt.tscn)

**File**: `assets/objects/enemy/RockAnt.tscn`
**Type**: Dig + Melee Enemy

**Configuration**:
```gdscript
DefaultAbilityIds = Array[String](["rockant_dig", "rockant_melee"])
```

**Ability Details**:
- **rockant_dig**: Physical attack, 20 damage, 300 range (underground attack)
- **rockant_melee**: Physical attack, 12 damage, 80 range (close combat)
- Works with existing AttackSwitcher system

**Suggested AI Rules** (can be added in Godot editor):
```gdscript
AIRules:
  - RuleName: "Dig Attack"
    Priority: 60
    Probability: 0.8
    BehaviorType: RangedAttack
    AbilityId: "rockant_dig"
    BehaviorParams: {ability_index: 0, optimal_distance: 200}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "Between", distance_min: 150, distance_max: 300}
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 0}
  
  - RuleName: "Melee Attack"
    Priority: 50
    BehaviorType: RangedAttack
    AbilityId: "rockant_melee"
    BehaviorParams: {ability_index: 1}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "LessThan", distance: 100}
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 1}
  
  - RuleName: "Approach"
    Priority: 40
    BehaviorType: MeleeAttack
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "GreaterThan", distance: 300}
  
  - RuleName: "Idle"
    Priority: 0
    BehaviorType: Idle
    Conditions:
      - ConditionType: NoTarget
```

**Behavior**: RockAnt can intelligently choose between dig attack (medium range) and melee attack (close range) based on distance to target.

---

### 3. Firecloak (Firecloak.tscn)

**File**: `assets/objects/enemy/Firecloak.tscn`
**Type**: Advanced Ranged Enemy

**Configuration**:
```gdscript
DefaultAbilityIds = Array[String](["firecloak_fireball", "firecloak_dash"])
```

**Ability Details**:
- **firecloak_fireball**: Magical attack, 15 damage, 700 range (projectile)
- **firecloak_dash**: Physical attack, 25 damage, 500 range (dash attack)
- Works with existing AttackSwitcher and combat systems

**Suggested AI Rules** (can be added in Godot editor):
```gdscript
AIRules:
  - RuleName: "Flee When Low Health"
    Priority: 100
    Probability: 1.0
    BehaviorType: Flee
    BehaviorParams: {flee_distance: 400}
    Conditions:
      - ConditionType: LowHealth
        ConditionParams: {threshold: 0.25}
      - ConditionType: HasTarget
  
  - RuleName: "Dodge Close Enemies"
    Priority: 80
    Probability: 0.7
    BehaviorType: Dodge
    BehaviorParams: {dodge_distance: 150, consume_stamina: false}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "LessThan", distance: 100}
  
  - RuleName: "Dash Attack When Close"
    Priority: 70
    Probability: 0.7
    BehaviorType: RangedAttack
    AbilityId: "firecloak_dash"
    BehaviorParams: {ability_index: 1, optimal_distance: 300}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "LessThan", distance: 150}
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 1}
  
  - RuleName: "Fireball When Far"
    Priority: 60
    Probability: 0.8
    BehaviorType: RangedAttack
    AbilityId: "firecloak_fireball"
    BehaviorParams: {ability_index: 0, optimal_distance: 400}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "GreaterThan", distance: 200}
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 0}
  
  - RuleName: "Strafe Around Target"
    Priority: 50
    Probability: 0.5
    BehaviorType: Strafe
    BehaviorParams: {strafe_distance: 250, clockwise: true}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: TargetDistance
        ConditionParams: {distance_type: "Between", distance_min: 150, distance_max: 400}
  
  - RuleName: "Idle"
    Priority: 0
    BehaviorType: Idle
    Conditions:
      - ConditionType: NoTarget
```

**Behavior**: Firecloak exhibits sophisticated tactical AI:
- Flees when health is low
- Dodges when player gets too close
- Uses dash attack at close-medium range
- Uses fireball at long range
- Strafes to maintain optimal distance

---

### 4. Raphael (Raphael.tscn)

**File**: `assets/objects/entity/character/Raphael.tscn`
**Type**: Ally Character (Player-Controllable)

**Configuration**:
```gdscript
DefaultAbilityIds = Array[String](["flamethrower", "flamethrower", "flamethrower", "flamethrower"])
```

**Status**: ✅ Already configured with abilities

**Ability Details**:
- **flamethrower**: Magical attack, 2 damage per tick, 300 range (continuous fire)
- Multiple slots for flexibility
- AI only active when character is NOT possessed by player

**Suggested AI Rules** (can be added in Godot editor):
```gdscript
AIRules:
  - RuleName: "Conserve Stamina"
    Priority: 90
    BehaviorType: Idle
    Conditions:
      - ConditionType: LowStamina
        ConditionParams: {threshold: 0.2}
  
  - RuleName: "Use Ability 0"
    Priority: 70
    Probability: 0.7
    BehaviorType: RangedAttack
    BehaviorParams: {ability_index: 0, optimal_distance: 250}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 0}
  
  - RuleName: "Use Ability 1"
    Priority: 65
    Probability: 0.6
    BehaviorType: RangedAttack
    BehaviorParams: {ability_index: 1, optimal_distance: 250}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 1}
  
  - RuleName: "Use Ability 2"
    Priority: 60
    Probability: 0.5
    BehaviorType: RangedAttack
    BehaviorParams: {ability_index: 2, optimal_distance: 250}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 2}
  
  - RuleName: "Follow Player"
    Priority: 10
    BehaviorType: FollowPlayer
    BehaviorParams: {follow_distance: 150}
    Conditions:
      - ConditionType: NoTarget
  
  - RuleName: "Idle"
    Priority: 0
    BehaviorType: Idle
```

**Behavior**: 
- When possessed by player: Full player control, AI disabled
- When not possessed (autonomous): Uses abilities intelligently, follows player, conserves stamina

**Note**: Arthur.tscn uses the same TestCharacter.cs script and can be configured similarly.

---

## Adding AI Rules in Godot Editor

### Step-by-Step Process

1. **Open Scene in Godot**:
   - Navigate to the .tscn file in Godot
   - Open the scene

2. **Select Root Node**:
   - Click on the entity node (Slime, RockAnt, Firecloak, or Raphael)

3. **In Inspector, Find AIRules**:
   - Scroll to the "AIRules" property
   - It's an Array[AIRuleData]

4. **Add New Rule**:
   - Click the "+" button to add a new AIRuleData
   - Configure the rule:
     - **RuleName**: Descriptive name
     - **Priority**: 0-100 (higher = more important)
     - **Probability**: 0.0-1.0 (chance to execute)
     - **BehaviorType**: Select from dropdown
     - **AbilityId**: Optional, specify ability by ID
     - **AbilityIndex**: Optional, specify ability by index
     - **BehaviorParams**: Dictionary of parameters

5. **Add Conditions**:
   - Expand the rule
   - Add conditions to the Conditions array
   - Configure each condition:
     - **ConditionType**: Select from dropdown
     - **ConditionParams**: Dictionary of parameters

6. **Save Scene**:
   - Save the scene file
   - AI will be configured on next load

---

## Testing the Configuration

### Verify Abilities Load

**In Godot:**
1. Run the scene containing the entity
2. Check console for any ability loading errors
3. Verify entity has abilities in slots

**Expected Console Output:**
```
Entity: Loading ability 'slime_melee'
Entity: Added ability to slot 0
Entity: Setup AI from 2 rules
```

### Verify AI Behavior

**Test Enemy AI:**
1. Load a scene with the enemy
2. Spawn or encounter the enemy
3. Observe behavior:
   - Should react to player presence
   - Should use abilities appropriately
   - Should follow AI rule priorities

**Test Ally AI:**
1. Load a scene with Raphael
2. Don't possess the character
3. Observe autonomous behavior:
   - Should use abilities on enemies
   - Should follow player when safe
   - Should conserve resources

---

## Troubleshooting

### Abilities Not Loading

**Problem**: Entity has no abilities
**Solutions**:
- Check ability IDs match abilities.json
- Verify abilities.json is properly formatted
- Check console for loading errors
- Ensure AutoSetupAI is true (default)

### AI Not Working

**Problem**: Entity doesn't respond intelligently
**Solutions**:
- Verify AIRules are configured in scene
- Check rule priorities (higher = more important)
- Verify conditions are properly set
- Enable AIDebugMode to see rule evaluation
- Check that Entity._AIProcess is being called

### Wrong Ability Used

**Problem**: Entity uses wrong ability
**Solutions**:
- Check AbilityId matches abilities.json
- Verify AbilityIndex is correct (0-2)
- Check ability is loaded in correct slot
- Verify condition requirements are met

---

## Technical Details

### Automatic Setup Process

When an entity with DefaultAbilityIds loads:

1. **Entity._Ready() is called**
2. **LoadDefaultAbilities() runs**:
   - Iterates through DefaultAbilityIds
   - Calls GalatimeGlobals.GetAbilityById() for each
   - Adds ability to entity slots via AddAbility()
3. **SetupAIFromRules() runs** (if AutoSetupAI = true):
   - Creates AIController instance
   - Converts each AIRuleData to AIRule via AIRuleFactory
   - Adds rules to controller
   - Sets up _AIProcess integration
4. **Entity is ready with abilities and AI**

### Scene File Format

Example from slime.tscn:
```gdscript
[node name="Slime" type="CharacterBody2D" ...]
script = ExtResource("1_ufwr7")
DefaultAbilityIds = Array[String](["slime_melee"])
Stats = SubResource("Resource_ygl1b")
Element = SubResource("Resource_lywks")
Team = 1
```

The `DefaultAbilityIds` property is now set directly in the scene file, making it visible and editable in the Godot inspector.

---

## Summary

✅ **All 4 entities configured with abilities**
✅ **Ready for AI Controller integration**
✅ **Backward compatible with existing systems**
✅ **Easily configurable in Godot editor**
✅ **Comprehensive AI rule examples provided**

Entities can now be fully configured through the Godot editor without touching code!
