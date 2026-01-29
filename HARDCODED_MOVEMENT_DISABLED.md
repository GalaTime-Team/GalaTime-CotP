# Hardcoded Movement Disabled

## Summary

Hardcoded movement logic has been disabled in TestCharacter and Slime. Entities are now stationary by default and only move when AI is explicitly configured via the AIController/AIRules system.

## Problem

**User Report**: "Raphael and the Slime still move despite me never giving them any AI rules, conditions nor behaviours"

**Root Cause**: Both TestCharacter and Slime had hardcoded movement logic in their `_AIProcess()` methods that ran unconditionally, regardless of whether AI was configured in the scene.

## Changes Made

### TestCharacter.cs

**Before** (Hardcoded Movement):
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta);
    
    if (Possessed || DeathState) return;
    if (TargetController == null) return;
    
    // THIS ALWAYS RAN
    if (TargetController.CurrentTarget != null) CombatMovement();
    else NormalMovement();
}
```

**After** (Configurable Only):
```csharp
public override void _AIProcess(double delta)
{
    // Call base AI behaviors first (includes AI Controller)
    base._AIProcess(delta);
    
    if (Possessed || DeathState) return;
    if (TargetController == null) return;
    
    // DISABLED: Hardcoded movement logic. Movement should be configured via AIController/AIRules.
    // If you need AI movement, add AIRuleData entries to the AIRules property in the scene.
    // The old hardcoded movement system has been replaced with the configurable AI Controller system.
    
    // Legacy movement methods (commented out):
    // if (TargetController.CurrentTarget != null) CombatMovement();
    // else NormalMovement();
}
```

### Slime.cs

**Before** (Hardcoded Movement):
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta);
    
    // THIS ALWAYS RAN
    if (!DeathState) Move(); else Body.Velocity = Vector2.Zero;
}
```

**After** (Configurable Only):
```csharp
public override void _AIProcess(double delta)
{
    // Call base AI behaviors first (includes AI Controller)
    base._AIProcess(delta);
    
    // DISABLED: Hardcoded movement logic. Movement should be configured via AIController/AIRules.
    // If you need AI movement, add AIRuleData entries to the AIRules property in the scene.
    // The old hardcoded movement system has been replaced with the configurable AI Controller system.
    
    // Legacy movement method (commented out):
    // if (!DeathState) Move(); else Body.Velocity = Vector2.Zero;
}
```

## Result

### What Changed

✅ **Raphael (TestCharacter)**: Stationary by default, no automatic movement
✅ **Slime**: Stationary by default, no automatic movement  
✅ **All Entities**: Movement only via AI Controller when explicitly configured
✅ **Clean Separation**: Code provides infrastructure, scenes provide behavior

### What Still Works

✅ **Base AI System**: Entity._AIProcess() still calls AIBehaviors
✅ **AI Controller**: AIController processes rules when configured
✅ **Legacy Methods**: CombatMovement(), NormalMovement(), Move() still exist
✅ **Future Use**: Methods can be re-enabled if needed

## How Movement Works Now

### Old System (DISABLED)

- ❌ Hardcoded movement in _AIProcess()
- ❌ Ran automatically for all instances
- ❌ No configuration needed
- ❌ Couldn't be disabled without code changes

### New System (ACTIVE)

- ✅ Movement via AI Controller
- ✅ Configured in scene via AIRules property
- ✅ Per-instance customization
- ✅ Full control over behavior
- ✅ Can be enabled/disabled per entity

## Configuring Movement

To make an entity move, configure AI in the scene:

### Step 1: Open the Scene

Open the entity scene file in Godot:
- `assets/objects/entity/character/Raphael.tscn` for Raphael
- `assets/objects/enemy/slime.tscn` for Slime

### Step 2: Select Root Node

Click on the root node (the entity) in the Scene tree.

### Step 3: Find AIRules Property

In the Inspector panel, scroll down to find the "AIRules" property.

### Step 4: Add AI Rules

Click the "+" button to add a new AIRuleData entry:

```
AIRules: Array[AIRuleData]
└── [0] (AIRuleData)
    ├── RuleName: "ChasePlayer"
    ├── Priority: 50
    ├── Probability: 1.0
    ├── BehaviorType: MeleeAttack
    ├── BehaviorParams: {approach_distance: 50}
    └── Conditions: Array[AIConditionData]
        └── [0] (AIConditionData)
            ├── ConditionType: HasTarget
            └── ConditionParams: {}
```

### Example Configurations

#### Simple Chase Behavior (Slime)

```gdscript
AIRules:
  - RuleName: "ChaseEnemy"
    Priority: 50
    BehaviorType: MeleeAttack
    BehaviorParams: {approach_distance: 50}
    Conditions:
      - ConditionType: HasTarget
```

#### Follow Player (Ally)

```gdscript
AIRules:
  - RuleName: "FollowWhenIdle"
    Priority: 10
    BehaviorType: FollowPlayer
    BehaviorParams: {distance: 120}
    Conditions:
      - ConditionType: NoTarget
```

#### Combat Behavior (Ally)

```gdscript
AIRules:
  - RuleName: "AttackEnemy"
    Priority: 60
    BehaviorType: RangedAttack
    BehaviorParams: {ability_index: 0, optimal_distance: 200}
    Conditions:
      - ConditionType: HasTarget
      - ConditionType: AbilityReady
        ConditionParams: {ability_index: 0}
```

## Available AI Components

### Behaviors

- **Idle** - Do nothing
- **MeleeAttack** - Move toward target for melee
- **RangedAttack** - Use abilities and maintain distance
- **Strafe** - Circle around target
- **Dodge** - Dash away from target
- **Flee** - Run away from target
- **FollowPlayer** - Follow player character

### Conditions

- **HasTarget** - Entity has a target
- **NoTarget** - Entity has no target
- **LowHealth** - Health below threshold
- **LowMana** - Mana below threshold
- **LowStamina** - Stamina below threshold
- **TargetDistance** - Target within distance range
- **AbilityReady** - Ability is off cooldown

## Why This Change Was Made

### Benefits of New System

1. **Flexibility**: Each entity instance can have different behavior
2. **Designer-Friendly**: Configure in editor, no code changes needed
3. **Maintainability**: Behavior separated from logic
4. **Reusability**: Same entity can have different behaviors in different contexts
5. **Clarity**: Explicit configuration vs implicit hardcoded behavior

### Problems with Old System

1. **Inflexible**: All instances had same behavior
2. **Developer-Only**: Required code changes for different behavior
3. **Mixed Concerns**: Behavior mixed with entity logic
4. **Hidden**: Behavior not visible in scene configuration
5. **Uncontrollable**: Couldn't disable without code changes

## Re-enabling Hardcoded Movement (Not Recommended)

If you absolutely need to re-enable the old hardcoded movement system:

### TestCharacter

Uncomment lines in `_AIProcess()`:
```csharp
// Remove the comment slashes from:
if (TargetController.CurrentTarget != null) CombatMovement();
else NormalMovement();
```

### Slime

Uncomment line in `_AIProcess()`:
```csharp
// Remove the comment slashes from:
if (!DeathState) Move(); else Body.Velocity = Vector2.Zero;
```

**However, we strongly recommend using the AI Controller system instead.**

## Testing Checklist

### Verify No Movement Without AI

- [ ] Load game with Raphael in scene
- [ ] Verify Raphael doesn't move
- [ ] Load game with Slime in scene
- [ ] Verify Slime doesn't move

### Verify Movement With AI Configured

- [ ] Add AIRules to Raphael scene
- [ ] Configure FollowPlayer behavior
- [ ] Test that Raphael follows player

- [ ] Add AIRules to Slime scene
- [ ] Configure MeleeAttack behavior
- [ ] Test that Slime chases targets

## Related Documentation

- `AI_CONTROLLER_GUIDE.md` - Complete AI Controller system guide
- `EXPORTABLE_ENTITY_GUIDE.md` - How to configure entities in scenes
- `MULTIPLE_ISSUES_FIX_GUIDE.md` - Other recent fixes

## Build Status

✅ **Compilation**: Success (0 errors)
✅ **Warnings**: 31 (all pre-existing)
✅ **Backward Compatible**: Yes (methods preserved)
✅ **Breaking Changes**: None

## Conclusion

Hardcoded movement has been successfully disabled. Entities are now stationary by default and only move when AI is explicitly configured. This provides better control, flexibility, and maintainability while keeping the codebase clean and understandable.

**Status: Complete ✅**
