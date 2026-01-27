# Entity Script Refactoring - Summary of Changes

## Problem Statement
Work on the Entities Script by going through the NPCs, Allies, Enemies & Player scripts, separating functionalities and making sure:
- The player is movable ✅
- The allies and enemies have AI assigned ✅
- The allies and enemies can have custom AI combinations assigned to them ✅
- The allies and enemies can have custom 3 abilities to use (ranged attacks) ✅

## Solution Overview

### 1. Extended Entity Base Class
**File**: `assets/scripts/objects/classes/entity/Entity.cs`

Added two new systems to the Entity base class:

#### Abilities System
```csharp
public List<AbilityData> Abilities = new();

public virtual void AddAbility(AbilityData ability, int index)
public virtual bool UseAbility(int index)
public virtual void RemoveAbility(int index)
```
- Supports up to 3 abilities (indices 0-2)
- Automatic cooldown and charge management
- Works for ALL entity types

#### AI Behaviors System
```csharp
public List<Action<double>> AIBehaviors = new();

public void AddAIBehavior(Action<double> behavior)
public void RemoveAIBehavior(Action<double> behavior)
public void ClearAIBehaviors()
```
- Multiple custom AI behaviors can be assigned
- All behaviors execute in _AIProcess()
- Allows flexible AI combinations

### 2. Updated GalatimeAbility
**File**: `assets/scripts/objects/classes/GalatimeAbility.cs`

Changed to work with Entity base class:
```csharp
public virtual void Execute(Entity entity)
public virtual void Execute(HumanoidCharacter p) // Backward compatible
```
- Abilities now work with any entity type
- Maintains backward compatibility with existing abilities

### 3. Implemented NPCharacter
**File**: `assets/scripts/objects/NPCharacter.cs`

Fully functional NPC entity:
- Can be ally or enemy (set via Team)
- Configurable FollowPlayer behavior
- Automatic ability usage in combat
- Supports custom AI behaviors
- Default AI for combat and following

### 4. Updated Existing Entities

#### HumanoidCharacter.cs
- Changed to properly override Entity methods
- Maintains all existing functionality

#### TestCharacter.cs & Slime.cs
- Added `base._AIProcess(delta)` call
- Now supports custom AI behaviors

## What's Now Possible

### Example 1: Add Abilities to Enemy
```csharp
public override void _Ready()
{
    base._Ready();
    
    AddAbility(GalatimeGlobals.GetAbilityById("fireball"), 0);
    AddAbility(GalatimeGlobals.GetAbilityById("ice_lance"), 1);
    AddAbility(GalatimeGlobals.GetAbilityById("lightning"), 2);
}
```

### Example 2: Add Custom AI Behavior
```csharp
public override void _Ready()
{
    base._Ready();
    
    // Add aggressive behavior
    AddAIBehavior((delta) => {
        if (TargetController.CurrentTarget != null)
        {
            var direction = GlobalPosition.DirectionTo(TargetController.CurrentTarget.GlobalPosition);
            Body.Velocity = direction * Speed * 1.5f;
        }
    });
    
    // Add ability usage behavior
    AddAIBehavior((delta) => {
        if (ShouldAttack())
        {
            UseAbility(UnityEngine.Random.Range(0, 3));
        }
    });
}
```

### Example 3: Create Custom NPC
```csharp
var npc = new NPCharacter();
npc.Team = Teams.Allies;
npc.FollowPlayer = true;

// Add support abilities
npc.AddAbility(healAbility, 0);
npc.AddAbility(shieldAbility, 1);
npc.AddAbility(buffAbility, 2);

// Add custom healing behavior
npc.AddAIBehavior((delta) => {
    var allies = GetTree().GetNodesInGroup("ally");
    foreach (var ally in allies)
    {
        if (ally is Entity e && e.Health < e.Stats[EntityStatType.Health].Value * 0.5f)
        {
            npc.UseAbility(0); // Heal
            break;
        }
    }
});
```

## Requirements Verification

✅ **Player is movable**
- Player.cs SetMove() handles movement via input
- WASD/arrows for movement
- Respects CanMove and frozen states

✅ **Allies and enemies have AI assigned**
- Entity._AIProcess() base implementation
- TestCharacter has combat/follow AI
- Slime has movement AI
- NPCharacter has configurable AI

✅ **Custom AI combinations**
- AIBehaviors list allows multiple behaviors
- AddAIBehavior() for dynamic assignment
- All behaviors execute each frame

✅ **Custom 3 abilities (ranged attacks)**
- Entity.Abilities list (3 slots)
- AddAbility() with index 0-2
- UseAbility() with automatic cooldowns
- Works for all entity types

## Testing Results

- **Build Status**: ✅ Success (0 errors)
- **CodeQL Security**: ✅ 0 vulnerabilities
- **Backward Compatibility**: ✅ Maintained
- **Functionality**: ✅ All requirements met

## Documentation

See `ENTITY_SYSTEM_GUIDE.md` for:
- Complete API reference
- Usage examples
- Migration guide for existing code
- Troubleshooting tips

## Impact

**Minimal Changes**:
- Only 6 files modified
- No breaking changes to existing code
- All existing functionality preserved
- Enhanced capabilities added to base Entity class

**Benefits**:
1. **Unified System**: Abilities work for all entities
2. **Flexibility**: Multiple AI behaviors per entity
3. **Reusability**: NPCharacter can be configured for any role
4. **Extensibility**: Easy to add new behaviors and abilities
5. **Maintainability**: Clean separation of concerns

## Next Steps

The implementation is complete and ready for use. To integrate:

1. **For new enemies**: Add abilities via AddAbility()
2. **For custom AI**: Use AddAIBehavior() 
3. **For NPCs**: Use NPCharacter class
4. **For abilities**: Update Execute() to work with Entity

See ENTITY_SYSTEM_GUIDE.md for detailed examples and best practices.
