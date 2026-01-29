# NullReferenceException Fixes Guide

## Overview

This guide documents the fixes applied to resolve three critical NullReferenceException errors that occurred during character spawning in the game.

## Issues Fixed

### 1. HumanoidCharacter.SetDirectionByWeapon()

**Error**:
```
E 0:00:49:960   void HumanoidCharacter.SetDirectionByWeapon(): 
                System.NullReferenceException: Object reference not set to an instance of an object.
```

**Root Cause**: The `SetDirectionByWeapon()` method accessed `Weapon.RotationDegrees` before the `Weapon` node was initialized.

**Location**: `assets/scripts/objects/HumanoidCharacter.cs`, line 273

**Fix**: Added null check for `Weapon` before accessing its properties.

### 2. Player.SetMove()

**Error**:
```
E 0:00:49:960   void Galatime.Player.SetMove(): 
                System.NullReferenceException: Object reference not set to an instance of an object.
```

**Root Cause**: The `SetMove()` method accessed `CurrentCharacter.Weapon` before the character was fully initialized. Additionally, there was a logic bug using `||` instead of `&&`.

**Location**: `assets/scripts/objects/Player.cs`, line 116

**Fix**: Fixed logical operator and added null checks for `CurrentCharacter.Weapon`.

### 3. Slime.Move()

**Error**:
```
E 0:00:33:751   void Slime.Move(): 
                System.NullReferenceException: Object reference not set to an instance of an object.
```

**Root Cause**: The `Move()` method accessed `Navigation`, `TargetController`, and `Weapon` nodes before they were initialized in `_Ready()`.

**Location**: `assets/scripts/objects/enemies/Slime.cs`, line 159-166

**Fix**: Added null checks for all required nodes before accessing them.

## Root Cause Analysis

### The Initialization Problem

The errors occurred due to Godot's node initialization order. Here's what happens:

**Execution Sequence**:
```
1. Parent class _Ready() is called (e.g., HumanoidCharacter)
2. Physics/Process methods CAN be called here ⚠️
3. Child class _Ready() is called (e.g., TestCharacter)
4. Nodes are initialized via GetNode() calls
```

**The Issue**: If `_MoveProcess()`, `_PhysicsProcess()`, or `_AIProcess()` is called between steps 2 and 3, nodes accessed in those methods haven't been initialized yet, causing NullReferenceExceptions.

### Why This Happened with Character Spawning

When characters were added to the save file and spawned:
1. Multiple characters spawn simultaneously (Arthur and Raphael)
2. Each character goes through the initialization sequence
3. The game loop runs between initialization steps
4. Process methods are called before nodes are ready

## Code Changes

### HumanoidCharacter.cs

#### Change 1: _MoveProcess() Method

**Before**:
```csharp
public override void _MoveProcess(double delta)
{
    // Required for the rotate character animation.
    SetDirectionByWeapon();

    // Switching between idle and walk state.
    if (IsWalk) State = Body.Velocity.Length() <= 20 ? HumanoidStates.Idle : HumanoidStates.Walk;

    // Set the animation based on the velocity and the state.
    if (!DisableHumanoidDoll) HumanoidDoll.SetAnimation(VectorRotation, State);

    // Set the trail particles texture to the same as the sprite texture.
    TrailParticles.Texture = Sprite?.Texture;
}
```

**After**:
```csharp
public override void _MoveProcess(double delta)
{
    // Check if required nodes are initialized
    if (Weapon == null || HumanoidDoll == null) return;

    // Required for the rotate character animation.
    SetDirectionByWeapon();

    // Switching between idle and walk state.
    if (IsWalk) State = Body.Velocity.Length() <= 20 ? HumanoidStates.Idle : HumanoidStates.Walk;

    // Set the animation based on the velocity and the state.
    if (!DisableHumanoidDoll) HumanoidDoll.SetAnimation(VectorRotation, State);

    // Set the trail particles texture to the same as the sprite texture.
    if (TrailParticles != null && Sprite != null) TrailParticles.Texture = Sprite.Texture;
}
```

#### Change 2: SetDirectionByWeapon() Method

**Before**:
```csharp
protected void SetDirectionByWeapon()
{
    var r = Mathf.Wrap(Weapon.RotationDegrees, 0, 360);
    VectorRotation = r switch
    {
        <= 45 or >= 320 => Vector2.Right,
        >= 45 and <= 135 => Vector2.Down,
        >= 135 and <= 220 => Vector2.Left,
        >= 220 and <= 320 => Vector2.Up,
        _ => Vector2.Zero
    };
}
```

**After**:
```csharp
protected void SetDirectionByWeapon()
{
    // Check if Weapon is initialized before accessing it
    if (Weapon == null) return;
    
    var r = Mathf.Wrap(Weapon.RotationDegrees, 0, 360);
    VectorRotation = r switch
    {
        <= 45 or >= 320 => Vector2.Right,
        >= 45 and <= 135 => Vector2.Down,
        >= 135 and <= 220 => Vector2.Left,
        >= 220 and <= 320 => Vector2.Up,
        _ => Vector2.Zero
    };
}
```

### Player.cs

#### Change 1: SetMove() Method

**Before**:
```csharp
private void SetMove()
{
    Vector2 inputVelocity = Vector2.Zero;

    // Don't move if the player is not event exist.
    if (CurrentCharacter != null || IsInstanceValid(CurrentCharacter))  // ❌ Logic bug: || should be &&
    {
        if (Input.IsActionPressed("game_move_up")) inputVelocity.Y -= 1;
        // ... input handling ...
        
        CurrentCharacter?.Weapon.LookAt(GetGlobalMousePosition());  // ❌ Can still be null
        SetCameraPosition();
    }
}
```

**After**:
```csharp
private void SetMove()
{
    Vector2 inputVelocity = Vector2.Zero;

    // Don't move if the player is not event exist.
    if (CurrentCharacter != null && IsInstanceValid(CurrentCharacter) && CurrentCharacter.Weapon != null)  // ✅ Fixed
    {
        if (Input.IsActionPressed("game_move_up")) inputVelocity.Y -= 1;
        // ... input handling ...
        
        CurrentCharacter.Weapon.LookAt(GetGlobalMousePosition());  // ✅ Safe now
        SetCameraPosition();
    }
}
```

#### Change 2: SetCameraPosition() Method

**Before**:
```csharp
private void SetCameraPosition()
{
    var c = CurrentCharacter;
    var cpos = c.Weapon.GlobalPosition;  // ❌ Can be null
    Camera.GlobalPosition = Camera.GlobalPosition.Lerp(cpos + ((GetGlobalMousePosition() - c.Weapon.GlobalPosition) / 5 + CameraOffset), 0.05f);
}
```

**After**:
```csharp
private void SetCameraPosition()
{
    // Check if CurrentCharacter and Weapon are initialized
    if (CurrentCharacter == null || CurrentCharacter.Weapon == null) return;  // ✅ Safe guard
    
    var c = CurrentCharacter;
    var cpos = c.Weapon.GlobalPosition;
    Camera.GlobalPosition = Camera.GlobalPosition.Lerp(cpos + ((GetGlobalMousePosition() - c.Weapon.GlobalPosition) / 5 + CameraOffset), 0.05f);
}
```

### Slime.cs

#### Change: Move() Method

**Before**:
```csharp
public void Move()
{
    var enemy = TargetController.CurrentTarget;  // ❌ Can be null
    if (enemy != null && CanMove)
    {
        Vector2 vectorPath = Vector2.Zero;
        Navigation.TargetPosition = enemy.GlobalPosition;  // ❌ Navigation can be null
        vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition()) * Speed;
        float rotation = Body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
        Weapon.Rotation = rotation;  // ❌ Weapon can be null
        float rotationDeg = Mathf.RadToDeg(rotation);
        float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
        Sprite.FlipH = rotationDegPositive <= 90;  // ❌ Sprite can be null
        Body.Velocity = vectorPath;
    }
    else Body.Velocity = Vector2.Zero;
}
```

**After**:
```csharp
public void Move()
{
    // Check if required nodes are initialized
    if (Navigation == null || TargetController == null || Weapon == null) return;  // ✅ Safe guard
    
    var enemy = TargetController.CurrentTarget;
    if (enemy != null && CanMove)
    {
        Vector2 vectorPath = Vector2.Zero;
        Navigation.TargetPosition = enemy.GlobalPosition;
        vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition()) * Speed;
        float rotation = Body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
        Weapon.Rotation = rotation;
        float rotationDeg = Mathf.RadToDeg(rotation);
        float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
        if (Sprite != null) Sprite.FlipH = rotationDegPositive <= 90;  // ✅ Null check added
        Body.Velocity = vectorPath;
    }
    else Body.Velocity = Vector2.Zero;
}
```

## Prevention Guidelines

### Best Practices for Node Initialization

1. **Always initialize nodes in _Ready()** before using them:
```csharp
public override void _Ready()
{
    base._Ready();
    
    // Initialize nodes FIRST
    Weapon = GetNode<Hand>("Hand");
    Sprite = GetNode<Sprite2D>("Sprite2D");
    // ... other nodes
    
    // THEN do other initialization
}
```

2. **Add null checks in process methods** that access nodes:
```csharp
public override void _Process(double delta)
{
    // Check if critical nodes exist
    if (Weapon == null) return;
    
    // Safe to use Weapon now
    Weapon.Rotation = angle;
}
```

3. **Use defensive programming** for critical nodes:
```csharp
// Instead of:
Sprite.FlipH = true;

// Use:
if (Sprite != null) Sprite.FlipH = true;

// Or use null-conditional operator:
Sprite?.SetTexture(newTexture);
```

4. **Document node dependencies** clearly:
```csharp
/// <summary>
/// Moves the entity toward its target.
/// Requires: Navigation, TargetController, Weapon to be initialized.
/// </summary>
public void Move()
{
    if (Navigation == null || TargetController == null || Weapon == null) return;
    // ... movement logic
}
```

## Testing Checklist

To verify the fixes work correctly:

- [ ] Build compiles successfully (0 errors)
- [ ] Start new game with save file containing allies
- [ ] Verify Arthur spawns without errors
- [ ] Verify Raphael spawns without errors
- [ ] Check console for NullReferenceExceptions (should be none)
- [ ] Verify Arthur is in idle state (not attack animation)
- [ ] Verify Raphael is visible
- [ ] Test character movement
- [ ] Test character abilities
- [ ] Test character switching
- [ ] Test combat with enemies
- [ ] Verify slimes move correctly
- [ ] Check camera follows player smoothly

## Troubleshooting

### If you still see NullReferenceExceptions:

1. **Check the stack trace** to identify which node is null
2. **Find where the node should be initialized** (usually in _Ready())
3. **Verify the node path** matches the scene tree structure
4. **Add null check** before accessing the node
5. **Test the initialization order** by adding debug prints

### Common Issues:

**Issue**: Character spawns but weapon doesn't work
**Solution**: Check if Weapon node is properly initialized in _Ready()

**Issue**: Camera behaves erratically
**Solution**: Verify CurrentCharacter.Weapon exists before camera calculations

**Issue**: Enemy doesn't move
**Solution**: Check if Navigation, TargetController nodes are initialized

## Summary

All three NullReferenceException errors were caused by accessing nodes before they were initialized. The fixes add defensive null checks to ensure methods gracefully handle the initialization phase.

**Key Takeaway**: Always check if nodes exist before accessing them in process methods, especially when they're initialized in child class _Ready() methods.
