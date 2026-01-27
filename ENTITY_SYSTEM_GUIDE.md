# Entity System Guide

## Overview
The Entity system has been refactored to support custom AI behaviors and abilities for all entity types (Players, Allies, Enemies, NPCs). This guide explains how to use these new features.

## Key Features

### 1. Player Movement ✅
- **Location**: `Player.cs`
- **Method**: `SetMove()` in `_PhysicsProcess`
- Player movement is handled through input actions and is fully functional
- Controls: WASD or arrow keys for movement
- Movement respects `CanMove` and frozen states

### 2. Abilities System (Ranged Attacks)
All entities can now have up to 3 abilities (ranged attacks):

#### Adding Abilities to Entities
```csharp
// Add an ability to any entity
entity.AddAbility(abilityData, index); // index 0-2

// Remove an ability
entity.RemoveAbility(index);

// Use an ability
bool success = entity.UseAbility(index);
```

#### Example: Adding Abilities to an Enemy
```csharp
public override void _Ready()
{
    base._Ready();
    
    // Add fireball ability at slot 0
    var fireballAbility = GalatimeGlobals.GetAbilityById("fireball");
    AddAbility(fireballAbility, 0);
    
    // Add ice shard at slot 1
    var iceShardAbility = GalatimeGlobals.GetAbilityById("ice_shard");
    AddAbility(iceShardAbility, 1);
}
```

### 3. Custom AI Combinations
Entities can have multiple AI behaviors assigned dynamically:

#### Adding Custom AI Behaviors
```csharp
// Define a custom AI behavior
System.Action<double> aggressiveBehavior = (delta) => {
    if (TargetController.CurrentTarget != null)
    {
        // Move aggressively toward target
        var direction = GlobalPosition.DirectionTo(TargetController.CurrentTarget.GlobalPosition);
        Body.Velocity = direction * Speed * 1.5f;
    }
};

// Add the behavior to an entity
entity.AddAIBehavior(aggressiveBehavior);

// Remove a behavior
entity.RemoveAIBehavior(aggressiveBehavior);

// Clear all custom behaviors
entity.ClearAIBehaviors();
```

#### Example: Combining Multiple AI Behaviors
```csharp
public override void _Ready()
{
    base._Ready();
    
    // Add patrol behavior
    AddAIBehavior(PatrolBehavior);
    
    // Add ability usage behavior
    AddAIBehavior(UseAbilitiesBehavior);
    
    // Add flee when low health behavior
    AddAIBehavior(FleeWhenLowHealthBehavior);
}

private void PatrolBehavior(double delta)
{
    // Patrol logic here
}

private void UseAbilitiesBehavior(double delta)
{
    // Ability usage logic here
}

private void FleeWhenLowHealthBehavior(double delta)
{
    if (Health < Stats[EntityStatType.Health].Value * 0.3f)
    {
        // Flee logic here
    }
}
```

### 4. Using NPCharacter

NPCharacter is a ready-to-use entity class that supports:
- Configurable AI (follow player, combat, custom behaviors)
- Up to 3 abilities
- Automatic ability usage in combat

#### Basic NPCharacter Setup
```csharp
// In your scene or code:
var npc = new NPCharacter();
npc.Team = Teams.Allies; // or Teams.Enemies
npc.FollowPlayer = true; // Will follow player when not in combat
npc.AbilityUseDelay = 2f; // Use abilities every 2 seconds

// Add abilities
npc.AddAbility(fireballAbility, 0);
npc.AddAbility(healAbility, 1);
npc.AddAbility(buffAbility, 2);

// Add custom AI if needed
npc.AddAIBehavior((delta) => {
    // Custom behavior here
});
```

## Architecture

### Entity Base Class
- **Location**: `assets/scripts/objects/classes/entity/Entity.cs`
- **Features**:
  - `Abilities` list (List<AbilityData>)
  - `AIBehaviors` list (List<Action<double>>)
  - `AddAbility()`, `UseAbility()`, `RemoveAbility()` methods
  - `AddAIBehavior()`, `RemoveAIBehavior()`, `ClearAIBehaviors()` methods
  - `_AIProcess(double delta)` - executes all custom AI behaviors

### HumanoidCharacter
- **Location**: `assets/scripts/objects/HumanoidCharacter.cs`
- Extends Entity with humanoid-specific features
- Overrides ability methods to add UI integration
- Used for player-controlled and allied characters

### TestCharacter
- **Location**: `assets/scripts/test/TestCharacter.cs`
- Extends HumanoidCharacter
- Has built-in combat and follow AI
- Calls `base._AIProcess()` to support custom AI behaviors

### NPCharacter
- **Location**: `assets/scripts/objects/NPCharacter.cs`
- Generic NPC entity with configurable AI
- Can be used for allies or enemies
- Automatic ability usage in combat
- Optional player-following behavior

### GalatimeAbility
- **Location**: `assets/scripts/objects/classes/GalatimeAbility.cs`
- Now works with Entity base class
- Backward compatible with HumanoidCharacter
- `Execute(Entity entity)` method

## Migration Guide

### For Existing Enemies
Add abilities to existing enemy classes:

```csharp
public override void _Ready()
{
    base._Ready();
    
    // Add your abilities
    var ability = GalatimeGlobals.GetAbilityById("your_ability");
    AddAbility(ability, 0);
}

public override void _AIProcess(double delta)
{
    base._AIProcess(delta); // IMPORTANT: Call base first
    
    // Your existing AI code
    if (TargetController.CurrentTarget != null)
    {
        // Use abilities randomly
        if (ShouldUseAbility())
        {
            UseAbility(0);
        }
    }
}
```

### For New Abilities
Abilities now work with all entities:

```csharp
public override void Execute(Entity entity)
{
    // Your ability code
    // Works with any entity (Player, Ally, Enemy, NPC)
    var projectile = ProjectileScene.Instantiate<Projectile>();
    projectile.GlobalPosition = entity.GlobalPosition;
    // ...
}
```

## Requirements Checklist

✅ **Player is movable**
- Implemented in Player.cs via SetMove()
- Input-driven movement with WASD/arrows

✅ **Allies and enemies have AI assigned**
- All entities inherit _AIProcess from Entity
- TestCharacter has combat/follow AI
- Enemies have movement AI
- NPCharacter has configurable AI

✅ **Custom AI combinations**
- AIBehaviors list allows multiple behaviors
- AddAIBehavior() for dynamic AI assignment
- All behaviors execute in _AIProcess

✅ **Custom 3 abilities (ranged attacks)**
- Entity base class has Abilities list
- AddAbility() supports 0-2 indices (3 slots)
- UseAbility() works for all entities
- Automatic cooldown management

## Examples

### Example 1: Aggressive Mage Enemy
```csharp
public class AggressiveMage : Entity
{
    public override void _Ready()
    {
        base._Ready();
        
        // Add 3 different spells
        AddAbility(GalatimeGlobals.GetAbilityById("fireball"), 0);
        AddAbility(GalatimeGlobals.GetAbilityById("ice_lance"), 1);
        AddAbility(GalatimeGlobals.GetAbilityById("lightning"), 2);
        
        // Add aggressive movement AI
        AddAIBehavior((delta) => {
            if (TargetController?.CurrentTarget != null)
            {
                var direction = GlobalPosition.DirectionTo(TargetController.CurrentTarget.GlobalPosition);
                Body.Velocity = direction * Speed;
                
                // Try to use random ability
                var random = new Random();
                if (random.Next(0, 100) < 5) // 5% chance per frame
                {
                    UseAbility(random.Next(0, 3));
                }
            }
        });
    }
}
```

### Example 2: Support Ally with Healing
```csharp
public class SupportAlly : NPCharacter
{
    public override void _Ready()
    {
        base._Ready();
        
        Team = Teams.Allies;
        FollowPlayer = true;
        
        // Add support abilities
        AddAbility(GalatimeGlobals.GetAbilityById("heal"), 0);
        AddAbility(GalatimeGlobals.GetAbilityById("shield"), 1);
        AddAbility(GalatimeGlobals.GetAbilityById("buff"), 2);
        
        // Custom AI: Heal nearby allies
        AddAIBehavior((delta) => {
            var allies = GetTree().GetNodesInGroup("ally");
            foreach (var ally in allies)
            {
                if (ally is Entity e && e.Health < e.Stats[EntityStatType.Health].Value * 0.5f)
                {
                    UseAbility(0); // Use heal
                    break;
                }
            }
        });
    }
}
```

## Troubleshooting

### Abilities Not Working
- Ensure ability has been added via `AddAbility()`
- Check that ability has charges remaining
- Verify ability cooldown is complete
- Make sure ability ScenePath is valid

### AI Not Running
- Verify `DisableAI` is false
- Check that `_AIProcess` calls `base._AIProcess(delta)`
- Ensure entity is not in death state

### Custom AI Behaviors Not Executing
- Call `base._AIProcess(delta)` at the start of your override
- Check that behaviors were added via `AddAIBehavior()`
- Verify entity is alive and AI is enabled
