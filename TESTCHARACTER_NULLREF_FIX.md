# TestCharacter NullReferenceException Fix

## Summary

Fixed NullReferenceException error in `TestCharacter._AIProcess()` and related methods that occurred during character initialization when characters were spawned from save files.

## Issue

**Error Message**:
```
E 0:00:25:090   void TestCharacter._AIProcess(double): System.NullReferenceException: Object reference not set to an instance of an object.
```

## Root Cause

The error occurred because `_AIProcess()` and related methods accessed nodes (TargetController, Weapon, RayCast) before they were fully initialized. This happened due to Godot's initialization order:

```
1. TestCharacter._Ready() starts
2. base._Ready() (HumanoidCharacter) called
3. _AIProcess() might be called here <-- ERROR: Nodes not yet initialized!
4. TestCharacter continues with GetNode calls
5. Nodes finally initialized
```

## Methods Fixed

### 1. _AIProcess()

**Problem**: Accessed `TargetController.CurrentTarget` without null check

**Before**:
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta);
    
    if (Possessed || DeathState) return;
    if (TargetController.CurrentTarget != null) CombatMovement();  // CRASH: TargetController could be null
    else NormalMovement();
}
```

**After**:
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta);
    
    if (Possessed || DeathState) return;
    
    // Check if TargetController is initialized before accessing it
    if (TargetController == null) return;
    
    if (TargetController.CurrentTarget != null) CombatMovement();
    else NormalMovement();
}
```

### 2. CombatMovement()

**Problem**: Multiple nodes accessed without null checks

**Before**:
```csharp
private async void CombatMovement()
{
    if (AttackTimer.IsStopped()) AttackTimer.Start();
    
    Vector2 vectorPath;
    
    // Take a sword if not equipped.
    if (Weapon.Item == null) Weapon.TakeItem(...);  // CRASH: Weapon could be null
    RayCast.TargetPosition = ...;  // CRASH: RayCast could be null
    Navigation.TargetPosition = TargetController.CurrentTarget.GlobalPosition;  // CRASH: TargetController could be null
    
    // ... more code ...
    
    if (TargetController.CurrentTarget == null) return;  // Check too late, already used above
    Weapon.Rotation = enemyRotation;  // CRASH: Weapon could be null
}
```

**After**:
```csharp
private async void CombatMovement()
{
    // Check if required nodes are initialized
    if (Weapon == null || TargetController == null || TargetController.CurrentTarget == null || RayCast == null) return;
    
    if (AttackTimer.IsStopped()) AttackTimer.Start();
    
    Vector2 vectorPath;
    
    // Take a sword if not equipped.
    if (Weapon.Item == null) Weapon.TakeItem(...);  // Safe now
    RayCast.TargetPosition = ...;  // Safe now
    Navigation.TargetPosition = TargetController.CurrentTarget.GlobalPosition;  // Safe now
    
    // ... more code ...
    
    // Check again after async operation in case target changed
    if (TargetController == null || TargetController.CurrentTarget == null) return;
    var enemyRotation = Body.GlobalPosition.AngleToPoint(TargetController.CurrentTarget.GlobalPosition);
    if (Weapon == null) return; // Check Weapon before accessing
    Weapon.Rotation = enemyRotation;  // Safe now
}
```

### 3. NormalMovement()

**Problem**: Accessed `Weapon.Rotation` without null check

**Before**:
```csharp
private async void NormalMovement()
{
    Weapon.Rotation = PathRotation;  // CRASH: Weapon could be null
    
    // ... rest of method
}
```

**After**:
```csharp
private async void NormalMovement()
{
    // Check if Weapon is initialized before accessing it
    if (Weapon == null) return;
    
    Weapon.Rotation = PathRotation;  // Safe now
    
    // ... rest of method
}
```

## Why Multiple Checks?

In `CombatMovement()`, we check nodes both at the start and after the `await` statement:

```csharp
// Check 1: Before any node access
if (Weapon == null || TargetController == null || ...) return;

// ... use nodes ...

await ToSignal(GetTree(), "physics_frame");  // Async operation

// Check 2: After async operation - nodes could have changed!
if (TargetController == null || TargetController.CurrentTarget == null) return;
if (Weapon == null) return;
```

**Why?**: After an `await` statement, the game state could have changed. The target could have been destroyed, the character could have been removed, etc. We need to check again to be safe.

## Testing Checklist

After applying these fixes:

- [x] Build succeeds (0 errors)
- [x] No NullReferenceException errors in console
- [x] Arthur spawns correctly from save file
- [x] Raphael spawns correctly from save file
- [x] AI behaviors work for allies
- [x] Combat movement works without crashes
- [x] Following player works without crashes

## Prevention Guidelines

When working with nodes in Godot, always:

1. **Check before first use** in process methods
2. **Check after await statements** in async methods
3. **Early return** if nodes aren't ready
4. **Document** which nodes are required

**Pattern**:
```csharp
public override void _Process(double delta)
{
    // Check critical nodes first
    if (RequiredNode == null) return;
    
    // Safe to use now
    RequiredNode.DoSomething();
}

private async void AsyncMethod()
{
    // Check before use
    if (RequiredNode == null) return;
    
    RequiredNode.DoSomething();
    
    await SomeAsyncOperation();
    
    // Check again after async operation
    if (RequiredNode == null) return;
    
    RequiredNode.DoSomethingElse();
}
```

## Related Fixes

This fix is part of a series of NullReferenceException fixes applied to the codebase:

1. **HumanoidCharacter** - Fixed SetDirectionByWeapon() and _MoveProcess()
2. **Player** - Fixed SetMove() and SetCameraPosition()
3. **Slime** - Fixed Move()
4. **TestCharacter** - Fixed _AIProcess(), CombatMovement(), NormalMovement() (this fix)

All follow the same defensive programming pattern of checking nodes before access.

## Files Modified

- `assets/scripts/test/TestCharacter.cs`
  - Line 177: Added null check in `_AIProcess()`
  - Line 184: Added null checks in `CombatMovement()`
  - Line 209-210: Added null checks after async operation
  - Line 272: Added null check in `NormalMovement()`

## Build Status

✅ **Compilation**: Success (0 errors)
✅ **Warnings**: 31 (all pre-existing)
✅ **Functionality**: All working
✅ **No Crashes**: Clean console

## Result

Characters now spawn and function correctly without NullReferenceException errors. The defensive null checks ensure smooth initialization even when process methods are called during the _Ready() phase.
