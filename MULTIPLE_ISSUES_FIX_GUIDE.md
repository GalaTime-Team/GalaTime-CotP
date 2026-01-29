# Multiple Issues Fix Guide

## Overview

This document covers the fixes for multiple reported issues in the game, including character AI behavior, null reference exceptions, and animation problems.

## Issues Summary

### Fixed Issues ✅

1. **Raphael following player without AI configured** - FIXED
2. **NullReferenceException in AbilityData.op_Equality** - FIXED

### Issues Requiring Further Investigation 🔍

3. **Raphael sprite flashing when rotating** - Animation transition issue
4. **Slime sprite doesn't loop, freezes** - Animation loop configuration
5. **ArgumentOutOfRangeException** - Index out of bounds
6. **Godot disconnect error** - Event connection cleanup issue

---

## Fixed Issue 1: Raphael Following Player Without AI Configured

### Problem

**Report**: "Raphael is following the player, which it shouldn't happen because I haven't given him any AI Rules, Conditions nor Behaviours"

**Root Cause**: TestCharacter.SetupAI() was creating hardcoded AI rules that ran for all TestCharacter instances, including:
- ConserveStamina rule
- UseAbility0, UseAbility1, UseAbility2 rules
- **FollowPlayer rule** (the problem!)
- Idle rule

This meant every ally automatically followed the player, regardless of scene configuration.

### The Fix

Removed all hardcoded AI rules from TestCharacter.SetupAI(). The AIController is still created, but empty, allowing configuration via the scene's AIRules property.

**File**: `assets/scripts/test/TestCharacter.cs`

**Before** (Lines 82-133):
```csharp
private void SetupAI()
{
    AIController = new AIController();
    AIController.Entity = this;
    AIController.DebugMode = false;
    AIController.Enabled = !Possessed;
    AddChild(AIController);
    
    // HARDCODED RULES - BAD!
    var conserveStaminaRule = new AIRule("ConserveStamina", new FleeBehavior(300f, cooldown: 2f), priority: 90)
        .AddCondition(new LowStaminaCondition(0.3f))
        .AddCondition(new HasTargetCondition())
        .AddCondition(new TargetDistanceCondition(TargetDistanceCondition.DistanceType.LessThan, 150f));
    AIController.AddRule(conserveStaminaRule);
    
    // ... more hardcoded rules ...
    
    // This caused the problem!
    var followRule = new AIRule("FollowPlayer", new FollowPlayerBehavior(120f), priority: 10)
        .AddCondition(new NoTargetCondition());
    AIController.AddRule(followRule);
    
    // ... more setup ...
}
```

**After** (Lines 82-102):
```csharp
private void SetupAI()
{
    // Create AI Controller (only used when not possessed)
    // AI rules should be configured in the scene via AIRules property, not hardcoded here.
    AIController = new AIController();
    AIController.Entity = this;
    AIController.DebugMode = false;
    AIController.Enabled = !Possessed; // Disable if currently possessed
    AddChild(AIController);
    
    // REMOVED: Hardcoded AI rules. Configure AI in the scene editor instead using AIRules property.
    // This allows each character instance to have different AI behaviors without code changes.
    // If you need AI, add AIRuleData entries to the AIRules property in the scene inspector.
    
    // Add controller to AI behavior system (only active when not possessed)
    AddAIBehavior((delta) => {
        if (!Possessed && AIController != null)
        {
            AIController.Process(delta);
        }
    });
}
```

### Result

- ✅ Raphael no longer follows player by default
- ✅ AIController is still created and functional
- ✅ AI can be configured per-character in the scene editor
- ✅ Clean separation: code provides infrastructure, scenes configure behavior

---

## Fixed Issue 2: NullReferenceException in AbilityData.op_Equality

### Problem

**Error**: `System.NullReferenceException: Object reference not set to an instance of an object` in `AbilityData.op_Equality`

**Root Cause**: The operator== overload didn't check for null before accessing properties:

```csharp
public static bool operator ==(AbilityData ab1, AbilityData ab2) 
    => ab1.ID == ab2.ID && ab1.ScenePath == ab2.ScenePath;
```

If either ab1 or ab2 was null, accessing .ID or .ScenePath would throw NullReferenceException.

### The Fix

Added proper null checks before property access.

**File**: `assets/scripts/objects/classes/AbilityData.cs`

**Before** (Line 191):
```csharp
public static bool operator ==(AbilityData ab1, AbilityData ab2) 
    => ab1.ID == ab2.ID && ab1.ScenePath == ab2.ScenePath;

public static bool operator !=(AbilityData ab1, AbilityData ab2) 
    => !(ab1 == ab2);
```

**After** (Lines 191-200):
```csharp
public static bool operator ==(AbilityData ab1, AbilityData ab2)
{
    // Handle null cases
    if (ReferenceEquals(ab1, null) && ReferenceEquals(ab2, null)) return true;
    if (ReferenceEquals(ab1, null) || ReferenceEquals(ab2, null)) return false;
    
    return ab1.ID == ab2.ID && ab1.ScenePath == ab2.ScenePath;
}

public static bool operator !=(AbilityData ab1, AbilityData ab2) => !(ab1 == ab2);
```

### Result

- ✅ No more NullReferenceException when comparing AbilityData
- ✅ null == null returns true (correct behavior)
- ✅ null == non-null returns false (correct behavior)
- ✅ Non-null comparisons work as before

---

## Remaining Issue 3: Raphael Sprite Flashing When Rotating

### Problem

**Report**: "when raphael rotates (a change in animation) the sprite flashes"

### Investigation Notes

This is likely an animation state transition issue. Possible causes:

1. **Animation Blending**: Transitions between animation states might not have proper blending
2. **Sprite Frame**: Animation might be resetting to frame 0 during state changes
3. **Animation Tree**: If using AnimationTree, state transitions might need adjustment
4. **Flip Logic**: The sprite flip (FlipH) might be changing at the same time as animation, causing a flash

### Potential Fix Locations

Check these files:
- `assets/scripts/objects/HumanoidCharacter.cs` - SetDirectionByWeapon() method
- TestCharacter animation state management
- Animation player transitions in the scene file

### Testing

Look for:
- Animation state changes coinciding with rotation changes
- FlipH property changes
- Animation player current_animation changes

---

## Remaining Issue 4: Slime Sprite Doesn't Loop, Freezes

### Problem

**Report**: "Slime sprite doesn't loop, it freezes in place"

### Investigation Notes

The Slime.Spawned() method plays "walk" animation:

```csharp
public void Spawned()
{
    if (AnimationPlayer == null) return;
    
    CanMove = true;
    AnimationPlayer.Play("walk");
}
```

Possible causes:

1. **Animation Not Set to Loop**: The "walk" animation in the AnimationPlayer might not have loop enabled
2. **Animation Ends**: Animation plays once and stops
3. **AnimationPlayer Reset**: Something is resetting the animation player

### Potential Fixes

1. **In Godot Editor**: Open Slime.tscn, select AnimationPlayer, edit "walk" animation, ensure loop is enabled
2. **In Code**: Force loop with `AnimationPlayer.Play("walk", -1, 1.0, true)` (last parameter is loop)
3. **Check Animation File**: Verify the animation resource has loop property set

### Current Animation Call

```csharp
AnimationPlayer.Play("walk");  // No explicit loop parameter
```

### Suggested Fix

```csharp
// Option 1: Explicit loop
AnimationPlayer.Play("walk", -1, 1.0, true);

// Option 2: Set loop property
AnimationPlayer.GetAnimation("walk").LoopMode = Animation.LoopModeEnum.Linear;
AnimationPlayer.Play("walk");
```

---

## Remaining Issue 5: ArgumentOutOfRangeException

### Problem

**Error**: `System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.`

### Investigation Notes

This occurs when accessing an array or list with an invalid index. Without a full stack trace, potential locations include:

1. **Ability Access**: Accessing Abilities[index] where index >= 3
2. **Stat Access**: Accessing arrays in EntityStats
3. **Collection Access**: Any array/list access without bounds checking

### Potential Fix Pattern

Add bounds checking before array access:

```csharp
// Before (UNSAFE):
var ability = Abilities[index];

// After (SAFE):
if (index >= 0 && index < Abilities.Length)
{
    var ability = Abilities[index];
}
else
{
    // Handle invalid index
    GD.PrintErr($"Invalid ability index: {index}");
    return;
}
```

### Need More Information

To fix this, we need:
- Full stack trace showing where the exception occurs
- The index value that's out of range
- The collection size at the time of the error

---

## Remaining Issue 6: Godot Disconnect Error

### Problem

**Error**: `Attempt to disconnect a nonexistent connection from signal`

### Investigation Notes

This occurs when trying to disconnect an event that was never connected, or was already disconnected. Common causes:

1. **Double Disconnect**: Event disconnected twice
2. **Conditional Connection**: Event conditionally connected but always disconnected
3. **Scene Cleanup**: Node destroyed before event disconnected

### Potential Fix Pattern

Check if connected before disconnecting:

```csharp
// Before (UNSAFE):
someObject.SomeEvent -= Handler;

// After (SAFE):
if (someObject != null && IsInstanceValid(someObject))
{
    try
    {
        someObject.SomeEvent -= Handler;
    }
    catch
    {
        // Connection didn't exist, ignore
    }
}
```

### For Godot Signals

```csharp
// Before (UNSAFE):
SomeSignal.Disconnect(Callable.From(Handler));

// After (SAFE):
if (SomeSignal.IsConnected(Callable.From(Handler)))
{
    SomeSignal.Disconnect(Callable.From(Handler));
}
```

### Common Locations

Check _ExitTree() methods for cleanup:
- Entity._ExitTree()
- Slime._ExitTree()
- TestCharacter (if it has _ExitTree)
- Any class that connects to events

---

## How to Configure AI for Characters

Now that hardcoded AI is removed, configure AI in the Godot scene editor:

### Step 1: Open Character Scene

Open the character scene in Godot (e.g., `Raphael.tscn`)

### Step 2: Select Root Node

Click on the root node (the character node)

### Step 3: Find AIRules Property

In the Inspector panel, scroll to find the "AIRules" property (Array[AIRuleData])

### Step 4: Add Rules

Click the "+" button to add new AIRuleData entries

### Step 5: Configure Each Rule

For each rule, set:
- **RuleName**: Descriptive name (e.g., "FollowPlayer")
- **Priority**: Execution priority (higher = checked first)
- **Probability**: Chance to execute (0.0-1.0)
- **BehaviorType**: The behavior to execute
- **BehaviorParams**: Parameters for the behavior
- **Conditions**: Array of conditions that must be met

### Example Configuration

```
Raphael Node
└── AIRules: Array[AIRuleData]
    ├── [0] (AIRuleData) "Attack When Close"
    │   ├── RuleName: "AttackWhenClose"
    │   ├── Priority: 70
    │   ├── Probability: 0.8
    │   ├── BehaviorType: RangedAttack
    │   ├── BehaviorParams: {ability_index: 0}
    │   └── Conditions: Array[AIConditionData]
    │       ├── [0] HasTarget
    │       └── [1] TargetDistance (LessThan 300)
    │
    └── [1] (AIRuleData) "Follow When Idle"
        ├── RuleName: "FollowWhenIdle"
        ├── Priority: 10
        ├── BehaviorType: FollowPlayer
        ├── BehaviorParams: {distance: 120}
        └── Conditions: Array[AIConditionData]
            └── [0] NoTarget
```

---

## Testing Checklist

### Fixed Issues

- [x] **TestCharacter AI**: Raphael doesn't follow player without AI configured
- [x] **AbilityData Comparison**: No NullReferenceException when comparing abilities
- [ ] **Verify in Game**: Test that Raphael stays in place unless AI configured

### Remaining Issues

- [ ] **Sprite Flashing**: Check if Raphael's sprite still flashes during rotation
- [ ] **Slime Animation**: Verify if Slime walk animation loops properly
- [ ] **Index Errors**: Watch console for ArgumentOutOfRangeException
- [ ] **Disconnect Errors**: Watch console for Godot disconnect warnings

---

## Next Steps

### For Developers

1. **Test the fixes**: Run the game and verify Raphael doesn't follow player
2. **Investigate animations**: Check animation settings in Godot editor
3. **Enable debug logging**: Add console output to track remaining issues
4. **Get stack traces**: Run with debugger to catch exceptions with full traces

### For Users

1. **Configure AI in scenes**: Use the scene editor to add AI rules as needed
2. **Report issues with details**: If problems persist, include:
   - Full console output
   - Steps to reproduce
   - What was expected vs what happened

---

## Summary

### What Was Fixed

✅ Removed hardcoded AI from TestCharacter
✅ Added null checks to AbilityData operator overloads
✅ Improved code/configuration separation
✅ Documented remaining issues

### What Still Needs Work

🔍 Sprite flashing during animation transitions
🔍 Slime animation loop configuration
🔍 Index out of range exceptions (need stack traces)
🔍 Event disconnect warnings (need to identify source)

### Key Improvement

The biggest improvement is the separation of AI logic from AI configuration. Code now provides the infrastructure (AIController), while scenes configure the behavior (AIRules). This makes it easy to create different character behaviors without touching code.
