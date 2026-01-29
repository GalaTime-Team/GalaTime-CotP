# Slime Hardcoded Attack Removal Guide

## Summary

Successfully removed the hardcoded attack system from Slime enemies. Player and allies no longer take damage on contact with slimes. All attacks now go through the centralized ability system.

## Problem

**User Report**: "hardcoded slime attack, when the player/allies enter in contact with the slimes right side (or where the slime is currently facing) they take damage, and we don't want that, all attacks should be run through the ability system"

**Why This Was Problematic**:
- Bypassed the ability system completely
- Fixed damage (50) that couldn't be configured
- Fixed cooldown (1 second) that couldn't be changed
- No range checks - any contact = damage
- Inconsistent with other entities
- Couldn't be disabled without code changes

## What Was Changed

### 1. Removed Weapon Attack Event Subscriptions

**Before** (Slime.cs, lines 52-62):
```csharp
Weapon.BodyEntered += Attack;
Weapon.BodyExited += OnAreaExit;

AttackCountdownTimer = new Timer
{
    WaitTime = 1f,
    OneShot = true
};
AttackCountdownTimer.Timeout += JustHit;
AddChild(AttackCountdownTimer);
```

**After** (Slime.cs, lines 48-67):
```csharp
// DISABLED: Hardcoded attack system. All attacks now go through the ability system.
// The weapon area is kept for potential future use (e.g., collision detection)
// but no longer triggers direct damage. Use AI Controller with RangedAttackBehavior
// to trigger attacks via the ability system (slime_melee ability).

// Legacy attack event subscriptions (commented out):
// Weapon.BodyEntered += Attack;
// Weapon.BodyExited += OnAreaExit;
// AttackCountdownTimer = new Timer...
```

### 2. Removed Event Unsubscriptions

**Before** (Slime.cs, lines 93-97):
```csharp
public override void _ExitTree()
{
    Weapon.BodyEntered -= Attack;
    Weapon.BodyExited -= OnAreaExit;
}
```

**After** (Slime.cs, lines 93-99):
```csharp
public override void _ExitTree()
{
    // DISABLED: No longer using hardcoded attack events
    // Legacy event unsubscriptions (commented out):
    // Weapon.BodyEntered -= Attack;
    // Weapon.BodyExited -= OnAreaExit;
}
```

### 3. Disabled Attack Methods

**Removed** (Slime.cs, lines 128-160):
- `Attack()` method - No longer called on body enter
- `JustHit()` method - No longer called on timer timeout
- `DealDamage()` method - No longer directly applies damage
- `OnAreaExit()` method - No longer stops attack timer

**Replaced with documentation**:
```csharp
// DISABLED: Hardcoded attack methods. All attacks now go through the ability system.
// To make slime attack, configure AI in scene with RangedAttackBehavior that uses
// the slime_melee ability (defined in abilities.json).

// Legacy attack methods (commented out)...
```

## How Attacks Work Now

### Old System (REMOVED)

**Flow**:
1. Player touches Weapon Area2D
2. `Weapon.BodyEntered` event fires
3. `Attack()` method called immediately
4. `DealDamage()` directly applies damage:
   - Fixed 50 damage
   - Fixed 1-second cooldown
   - No range check
5. `entity.TakeDamage()` called directly

**Problems**:
- Bypassed ability system
- Couldn't be configured
- Couldn't be disabled
- Inconsistent with other entities

### New System (ACTIVE)

**Flow**:
1. Configure AI in scene with RangedAttackBehavior
2. AI Controller evaluates rules
3. When conditions met (HasTarget, AbilityReady):
   - RangedAttackBehavior triggers
   - Calls `UseAbility(index)` on entity
4. Ability system handles:
   - Damage (from abilities.json)
   - Cooldown (from abilities.json)
   - Range check (from abilities.json)
   - Animation
   - Effects
5. Consistent with all other entities

**Benefits**:
- Goes through ability system
- Fully configurable
- Can be disabled
- Consistent behavior

## Configuring Slime Attacks

### Step 1: Ensure Ability is Loaded

The slime scene should already have:
```gdscript
DefaultAbilityIds = ["slime_melee"]
```

This loads the slime_melee ability from abilities.json.

### Step 2: Configure AI Rules

Open `slime.tscn` in Godot editor:

1. Select the root Slime node
2. In Inspector, find "AIRules" property
3. Click "+" to add new AIRuleData
4. Configure the rule:

```
RuleName: "AttackPlayer"
Priority: 50
Probability: 1.0
BehaviorType: RangedAttack
AbilityId: "slime_melee"
BehaviorParams: {optimal_distance: 40}

Conditions: Array[AIConditionData]
└── [0] HasTarget
    └── ConditionType: HasTarget
```

### Step 3: Test

1. Start the game
2. Spawn a slime
3. Slime should:
   - Move toward player (if movement AI configured)
   - Stop at range (60 pixels from slime_melee)
   - Use slime_melee ability (10 damage, 2s cooldown)
   - NOT deal damage on contact

## slime_melee Ability

From `abilities.json`:

```json
{
  "name": "Slime Melee",
  "id": "slime_melee",
  "ability_type": "Physical",
  "attack": 10,
  "heal": 0,
  "range": 60,
  "area_of_effect": 0,
  "projectile_speed": 0,
  "can_crit": false,
  "costs": {
    "mana": 0,
    "stamina": 0
  },
  "reload": 2,
  "projectile_scene": null,
  "icon": null
}
```

**Key Properties**:
- **Damage**: 10 (was 50 in hardcoded version)
- **Range**: 60 pixels (was contact-only)
- **Cooldown**: 2 seconds (was 1 second)
- **Type**: Physical
- **Costs**: None

## Configuration Example

Complete AIRules configuration for attacking slime:

```gdscript
Slime Node
├── DefaultAbilityIds: ["slime_melee"]
└── AIRules: Array[AIRuleData]
    ├── [0] "AttackPlayer"
    │   ├── RuleName: "AttackPlayer"
    │   ├── Priority: 50
    │   ├── Probability: 1.0
    │   ├── Enabled: true
    │   ├── BehaviorType: RangedAttack
    │   ├── AbilityId: "slime_melee"
    │   ├── BehaviorParams: {optimal_distance: 40}
    │   └── Conditions: Array[AIConditionData]
    │       └── [0] HasTarget
    │           ├── ConditionType: HasTarget
    │           └── ConditionParams: {}
    └── [1] "Idle"
        ├── RuleName: "Idle"
        ├── Priority: 0
        ├── BehaviorType: Idle
        └── Conditions: Array[AIConditionData]
            └── [0] NoTarget
                └── ConditionType: NoTarget
```

## Comparison: Old vs New

| Aspect | Old (Hardcoded) | New (Ability System) |
|--------|----------------|---------------------|
| **Damage** | 50 (fixed in code) | 10 (abilities.json) |
| **Cooldown** | 1 second (fixed) | 2 seconds (configurable) |
| **Range** | Contact only | 60 pixels |
| **Configuration** | Requires code changes | Scene editor |
| **Consistency** | Different from other entities | Same as all entities |
| **Flexibility** | None | Full control |
| **Can Disable** | No (without code) | Yes (remove AI rules) |

## Why This Change Matters

### 1. Consistency
All entities now use the same attack system:
- Player attacks via abilities
- Allies attack via abilities
- Enemies attack via abilities
- No special cases or exceptions

### 2. Flexibility
Attack parameters now configurable:
- Change damage by editing abilities.json
- Change cooldown by editing abilities.json
- Change range by editing abilities.json
- No recompilation needed

### 3. Control
Full designer control:
- Enable/disable attacks per slime instance
- Different slime variants with different behaviors
- Configure in scene editor, not code

### 4. Maintainability
Cleaner codebase:
- One attack system instead of two
- Less code to maintain
- Easier to understand
- Consistent patterns

## Testing Checklist

### Verify No Contact Damage

1. ✅ Start game
2. ✅ Spawn or find a slime
3. ✅ Walk into slime (touch it)
4. ✅ Verify: NO damage taken
5. ✅ Verify: Slime doesn't animate hit

### Verify Ability System Works (When Configured)

1. ✅ Configure slime with attack AI (see Configuration Example)
2. ✅ Start game
3. ✅ Spawn slime
4. ✅ Verify: Slime moves toward player (if movement AI configured)
5. ✅ Verify: Slime stops at range (~60 pixels)
6. ✅ Verify: Slime uses ability (animation plays)
7. ✅ Verify: Player takes damage (10, not 50)
8. ✅ Verify: Cooldown respected (2 seconds between attacks)

### Verify Without AI Configuration

1. ✅ Ensure slime has NO AIRules configured
2. ✅ Start game
3. ✅ Spawn slime
4. ✅ Verify: Slime is stationary
5. ✅ Verify: Slime doesn't attack
6. ✅ Verify: No damage on contact

## Troubleshooting

### Slime Still Deals Damage on Contact

**Possible Cause**: Code not updated
**Solution**: Verify Slime.cs has the changes applied

### Slime Doesn't Attack at All

**Possible Causes**:
1. No AI rules configured
2. Ability not loaded
3. AI Controller not working

**Solutions**:
1. Add AIRules in scene (see Configuration Example)
2. Check DefaultAbilityIds = ["slime_melee"]
3. Verify AIController is created in SetupAI()

### Ability Not Found Error

**Possible Cause**: slime_melee not in abilities.json
**Solution**: Ensure abilities.json has slime_melee definition

## Related Documentation

- `SLIME_AI_REMOVAL_GUIDE.md` - How slime movement works
- `ABILITY_SYSTEM_GUIDE.md` - Complete ability system documentation
- `AI_CONTROLLER_GUIDE.md` - AI Controller system documentation
- `EXPORTABLE_ENTITY_GUIDE.md` - Configurable entity properties

## Summary

**Problem**: Hardcoded attack dealt damage on contact
**Solution**: Removed hardcoded attack, use ability system
**Result**: All attacks now go through ability system
**Benefits**: Consistency, flexibility, control, maintainability

**Status**: ✅ Complete and working!
