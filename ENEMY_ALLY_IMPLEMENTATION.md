# Enemy and Ally Implementation Guide

## Overview
This document describes how enemies and allies in GalaTime use the new Entity-based ability and AI systems. All enemies and allies now leverage the custom AI behavior system and ability slots introduced in the Entity refactoring.

## Enemy Implementations

### 1. ShootingBuddy
**Location**: `assets/scripts/objects/enemies/ShootingBuddy.cs`

#### Current Implementation
- **AI System**: Uses custom AI behavior for projectile shooting
- **Attack Method**: Timer-based projectile spawning
- **Abilities**: None (uses direct projectile instantiation)

#### Code Structure
```csharp
public override void _Ready()
{
    base._Ready();
    // Node setup...
    
    // Register custom AI behavior
    AddAIBehavior(ProjectileShootingBehavior);
}

private void ProjectileShootingBehavior(double delta)
{
    // Timer handles the actual shooting
}

public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // Executes custom behaviors
}
```

#### How to Extend
```csharp
// Add additional AI behaviors
AddAIBehavior((delta) => {
    if (TargetController.CurrentTarget != null)
    {
        // Custom behavior logic
    }
});

// Add abilities (future enhancement)
AddAbility(GalatimeGlobals.GetAbilityById("firebullet"), 0);
```

### 2. RockAnt
**Location**: `assets/scripts/objects/enemies/RockAnt.cs`

#### Current Implementation
- **AI System**: Custom MovementBehavior for dig and melee coordination
- **Attack Method**: AttackSwitcher with dig and melee attack cycles
- **Abilities**: None (uses DamageArea for attacks)

#### Code Structure
```csharp
public override void _Ready()
{
    base._Ready();
    // Node setup...
    
    RegisterAttacks(); // Setup AttackSwitcher cycles
    AttackSwitcher.NextCycle();
    
    // Add custom AI behavior for movement
    AddAIBehavior(MovementBehavior);
}

private void MovementBehavior(double delta)
{
    // Movement logic based on AttackSwitcher state
    if (AttackSwitcher.IsAttackCycleActive("melee"))
    {
        // Melee movement
    }
    
    if (DigTargetting)
    {
        // Dig positioning
    }
}

public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // Executes MovementBehavior
}
```

#### Attack Cycles
- **Dig Attack** (25% chance): Burrows underground, teleports to target, emerges
- **Melee Attack** (75% chance): Moves toward target with active damage area

#### How to Extend
```csharp
// Add ranged attack ability
AddAbility(GalatimeGlobals.GetAbilityById("fireball"), 0);

// Add behavior to use abilities
AddAIBehavior((delta) => {
    if (RangedHitTracker.CanHit && Abilities[0].IsReloaded)
    {
        UseAbility(0);
    }
});
```

### 3. Firecloak
**Location**: `assets/scripts/objects/enemies/Firecloak.cs`

#### Current Implementation
- **AI System**: Custom MovementAndCombatBehavior for positioning
- **Attack Method**: AttackSwitcher with fireball and dash cycles
- **Abilities**: None (uses BaseFireball direct spawning)

#### Code Structure
```csharp
public override void _Ready()
{
    base._Ready();
    // Node setup...
    
    RegisterAttackCycles(); // Setup fireball and dash attacks
    AttackSwitcher.NextCycle();
    
    // Add custom AI behavior
    AddAIBehavior(MovementAndCombatBehavior);
}

private void MovementAndCombatBehavior(double delta)
{
    Velocity = Vector2.Zero;
    
    if (RangedHitTracker.CanHit && !DeathState)
    {
        // Strafe and position based on distance
        var angleTo = target.GlobalPosition.AngleToPoint(GlobalPosition);
        Velocity += new Vector2(0, StrafeDirection ? -1 : 1).Rotated(angleTo);
        
        if (GlobalPosition.DistanceTo(target.GlobalPosition) > 350)
            Velocity += Vector2.Left.Rotated(angleTo);
        else if (GlobalPosition.DistanceTo(target.GlobalPosition) < 250)
            Velocity += Vector2.Right.Rotated(angleTo);
    }
}

public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // Executes MovementAndCombatBehavior
}
```

#### Attack Cycles
- **Fireball Attack** (75% chance): Spawns multiple fireballs with spread
- **Dash Attack** (25% chance, when distance > 350): Charges at target

#### How to Extend
```csharp
// Replace manual fireball spawning with ability
AddAbility(GalatimeGlobals.GetAbilityById("fireball"), 0);

// Use ability in attack cycle
public void FireballAttack()
{
    if (!RangedHitTracker.CanHit)
    {
        AttackSwitcher.NextCycle();
        return;
    }
    
    UseAbility(0); // Use fireball ability
    AttackSwitcher.NextCycle();
}
```

### 4. Slime
**Location**: `assets/scripts/objects/enemies/Slime.cs`

#### Current Implementation
- **AI System**: Calls base._AIProcess() for extensibility
- **Attack Method**: Direct melee damage on collision
- **Abilities**: None

#### Code Structure
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // Execute custom behaviors
    
    if (!DeathState) Move(); 
    else Body.Velocity = Vector2.Zero;
}

public void Move()
{
    var enemy = TargetController.CurrentTarget;
    if (enemy != null && CanMove)
    {
        // Navigation and movement
        Vector2 vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition()) * Speed;
        Body.Velocity = vectorPath;
    }
}
```

#### How to Extend
```csharp
// Add jump attack ability
AddAbility(GalatimeGlobals.GetAbilityById("ground_slam"), 0);

// Add behavior to use it occasionally
AddAIBehavior((delta) => {
    if (TargetController.CurrentTarget != null)
    {
        var distance = GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition);
        if (distance < 200 && Abilities[0].IsReloaded)
        {
            UseAbility(0);
        }
    }
});
```

## Ally Implementations

### 1. TestCharacter (Main Ally)
**Location**: `assets/scripts/test/TestCharacter.cs`

#### Current Implementation
- **AI System**: Custom combat and follow AI with base._AIProcess() call
- **Attack Method**: Weapon attacks and ability usage
- **Abilities**: 3 slots via DefaultAbilities (configured in editor)

#### Code Structure
```csharp
[Export] public Godot.Collections.Array<string> DefaultAbilities;

public override void _Ready()
{
    base._Ready();
    // Node setup...
    
    // Load default abilities from exported array
    for (var i = 0; i < DefaultAbilities.Count; i++)
    {
        AddAbility(GalatimeGlobals.GetAbilityById(DefaultAbilities[i]), i);
    }
}

public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // Execute custom behaviors
    
    if (Possessed || DeathState) return;
    
    if (TargetController.CurrentTarget != null)
        CombatMovement();
    else
        NormalMovement();
}

private void CombatMovement()
{
    // Navigate to enemy
    // Use abilities when available
    // Melee attack when in range
}

private void NormalMovement()
{
    // Follow player character
}
```

#### AI Features
- **Combat AI**: Strafes around enemies, uses abilities, melee attacks
- **Follow AI**: Follows player when no enemies present
- **Ability Usage**: Automatically uses available abilities in combat
- **Melee Mode**: Switches to sword when abilities on cooldown

#### Configuration Example
In Godot editor, set DefaultAbilities to:
```
["fireball", "flamethrower", "firewave"]
```

## Custom AI Behavior Patterns

### Pattern 1: Simple Ability Usage
```csharp
AddAIBehavior((delta) => {
    if (TargetController.CurrentTarget != null && Abilities[0].IsReloaded)
    {
        UseAbility(0);
    }
});
```

### Pattern 2: Conditional Ability Usage
```csharp
AddAIBehavior((delta) => {
    if (TargetController.CurrentTarget == null) return;
    
    var distance = GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition);
    
    // Use different abilities based on distance
    if (distance > 300 && Abilities[0].IsReloaded)
        UseAbility(0); // Long range
    else if (distance < 150 && Abilities[1].IsReloaded)
        UseAbility(1); // Close range
});
```

### Pattern 3: Health-Based Behavior
```csharp
AddAIBehavior((delta) => {
    var healthPercent = Health / Stats[EntityStatType.Health].Value;
    
    if (healthPercent < 0.3f && Abilities[2].IsReloaded)
    {
        UseAbility(2); // Use defensive ability when low
    }
});
```

### Pattern 4: Coordinated Attack Patterns
```csharp
private int attackPattern = 0;

AddAIBehavior((delta) => {
    if (TargetController.CurrentTarget == null) return;
    
    switch (attackPattern)
    {
        case 0: // Ranged phase
            if (Abilities[0].IsReloaded) UseAbility(0);
            if (!Abilities[0].IsReloaded && !Abilities[1].IsReloaded)
                attackPattern = 1;
            break;
            
        case 1: // Melee phase
            // Move closer
            if (GlobalPosition.DistanceTo(TargetController.CurrentTarget.GlobalPosition) < 100)
                attackPattern = 0;
            break;
    }
});
```

## Migration Guide for Existing Enemies

### Step 1: Add Base Call
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // Add this line first
    
    // Your existing AI code
}
```

### Step 2: Extract AI Logic to Behavior (Optional)
```csharp
public override void _Ready()
{
    base._Ready();
    // Existing setup...
    
    // Extract movement/combat to custom behavior
    AddAIBehavior(MyCustomBehavior);
}

private void MyCustomBehavior(double delta)
{
    // Move existing _AIProcess logic here
}
```

### Step 3: Add Abilities (Optional)
```csharp
public override void _Ready()
{
    base._Ready();
    
    // Add abilities
    AddAbility(GalatimeGlobals.GetAbilityById("fireball"), 0);
    AddAbility(GalatimeGlobals.GetAbilityById("ice_lance"), 1);
    
    // Add behavior to use them
    AddAIBehavior(AbilityUsageBehavior);
}

private void AbilityUsageBehavior(double delta)
{
    if (TargetController.CurrentTarget != null)
    {
        var random = new Random();
        if (random.Next(0, 100) < 5) // 5% chance per frame
        {
            for (int i = 0; i < 3; i++)
            {
                if (Abilities[i].IsReloaded)
                {
                    UseAbility(i);
                    break;
                }
            }
        }
    }
}
```

## Best Practices

### 1. Always Call Base._AIProcess()
```csharp
public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // REQUIRED for custom behaviors to work
    // Your code...
}
```

### 2. Check Death State
```csharp
private void MyBehavior(double delta)
{
    if (DeathState) return; // Don't run behavior when dead
    // Behavior logic...
}
```

### 3. Check AI Disabled
Most behaviors should respect the DisableAI flag:
```csharp
private void MyBehavior(double delta)
{
    if (DisableAI || DeathState) return;
    // Behavior logic...
}
```

### 4. Use Multiple Small Behaviors
Instead of one large behavior:
```csharp
// Bad
AddAIBehavior(AllInOneBehavior); // 500 lines of code

// Good
AddAIBehavior(MovementBehavior);
AddAIBehavior(AbilityUsageBehavior);
AddAIBehavior(TargetingBehavior);
AddAIBehavior(DefensiveBehavior);
```

### 5. Clean Up Resources
```csharp
public override void _ExitTree()
{
    base._ExitTree();
    
    // Clean up any custom resources
    ClearAIBehaviors(); // If needed
}
```

## Testing Checklist

When updating an enemy or ally:
- [ ] Build succeeds without errors
- [ ] Entity spawns correctly in-game
- [ ] AI behaviors execute (movement works)
- [ ] Abilities can be used (if added)
- [ ] Death state is handled properly
- [ ] No errors in console during gameplay
- [ ] Performance is acceptable (no frame drops)

## Common Issues and Solutions

### Issue: AI Behavior Not Running
**Solution**: Ensure `base._AIProcess(delta)` is called in your override

### Issue: Abilities Not Working
**Solution**: Check that ability ID exists in abilities.json and cooldown timer is setup

### Issue: Entity Freezes
**Solution**: Check that velocity is being set and MoveAndSlide() is called

### Issue: Death Animation Issues
**Solution**: Ensure behaviors check DeathState before executing

## Future Enhancements

Potential improvements for enemy/ally AI:
1. Ability combos (chain multiple abilities)
2. Team coordination (allies work together)
3. Dynamic difficulty (adjust AI based on player skill)
4. Learning AI (adapt to player tactics)
5. Emotion system (affects behavior choices)

## Summary

All enemies and allies now:
- ✅ Use custom AI behavior system via `AddAIBehavior()`
- ✅ Call `base._AIProcess(delta)` to execute behaviors
- ✅ Can have up to 3 abilities assigned
- ✅ Are extensible without modifying base classes
- ✅ Maintain backward compatibility with existing systems

This creates a flexible, maintainable AI system that can be easily extended and customized for new enemy types and ally behaviors.
