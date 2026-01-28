# EntityStats Refactor Guide

## Overview

EntityStats has been refactored to use 9 fixed stat properties instead of resizable arrays. This provides a cleaner, more intuitive interface in the Godot editor while maintaining full backward compatibility.

## What Changed

### Before: Array-Based System

**Old Structure**:
```csharp
public class EntityStats
{
    [Export] public Array<EntityStatType> StatsNames;  // Resizable
    [Export] public Array<float> StatsValues;          // Resizable
}
```

**Problems**:
- Two separate arrays that needed to stay synchronized
- Could accidentally add/remove stats
- Confusing to configure (which index is which?)
- Needed helper methods to keep arrays in sync

### After: Fixed Property System

**New Structure**:
```csharp
public class EntityStats
{
    [Export] public float Health = 100f;
    [Export] public float Mana = 100f;
    [Export] public float Stamina = 100f;
    [Export] public float Agility = 0f;
    [Export] public float PhysicalAttack = 0f;
    [Export] public float MagicalAttack = 0f;
    [Export] public float PhysicalDefense = 0f;
    [Export] public float MagicalDefense = 0f;
    [Export] public float KnockbackResistance = 0f;
}
```

**Benefits**:
- 9 fixed properties, one for each stat type
- Cannot add/remove stats accidentally
- Each stat clearly labeled by name
- Simple, intuitive configuration
- No array synchronization needed

## Using EntityStats in Godot Editor

### Configuring Stats

1. Select an Entity node in your scene
2. In the Inspector, find the "Stats" property
3. You'll see 9 fixed stat properties:

```
Stats (EntityStats)
├── Health: 100
├── Mana: 100
├── Stamina: 100
├── Agility: 0
├── PhysicalAttack: 0
├── MagicalAttack: 0
├── PhysicalDefense: 0
├── MagicalDefense: 0
└── KnockbackResistance: 0
```

4. Adjust values as needed
5. Save the scene

### Example Configurations

**Basic Melee Enemy (Slime)**:
```
Health: 50
Mana: 0
Stamina: 0
Agility: 0
PhysicalAttack: 10
MagicalAttack: 0
PhysicalDefense: 2
MagicalDefense: 0
KnockbackResistance: 1
```

**Mage Enemy**:
```
Health: 80
Mana: 150
Stamina: 50
Agility: 5
PhysicalAttack: 5
MagicalAttack: 25
PhysicalDefense: 3
MagicalDefense: 10
KnockbackResistance: 2
```

**Tank Ally**:
```
Health: 200
Mana: 50
Stamina: 100
Agility: 2
PhysicalAttack: 15
MagicalAttack: 0
PhysicalDefense: 20
MagicalDefense: 15
KnockbackResistance: 8
```

## Using EntityStats in Code

### Creating EntityStats

```csharp
// Create with defaults
var stats = new EntityStats();
stats.InitializeStats();

// Create with custom values
var stats = new EntityStats
{
    Health = 150f,
    Mana = 100f,
    PhysicalAttack = 20f,
    PhysicalDefense = 10f
};
stats.InitializeStats();
```

### Accessing Stats

**Direct Property Access**:
```csharp
// Get property value
float health = stats.Health;
float attack = stats.PhysicalAttack;

// Set property value
stats.Health = 200f;
stats.InitializeStats(); // Re-initialize to update dictionary
```

**Dictionary Access** (Backward Compatible):
```csharp
// Access via dictionary (runtime values)
float health = stats[EntityStatType.Health].Value;
float attack = stats[EntityStatType.PhysicalAttack].Value;

// Modify via dictionary
stats[EntityStatType.Health].Value = 150f;
```

### Modifying Stats at Runtime

```csharp
// Method 1: Modify dictionary (preferred for runtime changes)
entity.Stats[EntityStatType.Health].Value += 50f;
entity.Stats[EntityStatType.PhysicalAttack].Value *= 1.2f;

// Method 2: Modify properties and re-initialize (less common)
entity.Stats.Health += 50f;
entity.Stats.InitializeStats();
```

### Enumerating Stats

```csharp
// Iterate through all stats
foreach (var stat in stats)
{
    GD.Print($"{stat.Type}: {stat.Value}");
}

// Access by index
for (int i = 0; i < stats.Count; i++)
{
    var stat = stats[i];
    GD.Print($"Stat {i}: {stat.Type} = {stat.Value}");
}
```

## Backward Compatibility

### What Still Works

✅ **Dictionary Access**:
```csharp
stats[EntityStatType.Health].Value  // Still works
```

✅ **Enumerators**:
```csharp
foreach (var stat in stats) { }     // Still works
```

✅ **Indexers**:
```csharp
stats[0]                             // Still works
stats[EntityStatType.Mana]          // Still works
```

✅ **Events**:
```csharp
stats.OnStatsChanged += handler;    // Still works
```

✅ **Count Property**:
```csharp
int count = stats.Count;            // Still works (always 10)
```

### What Changed

❌ **Array Properties Removed**:
```csharp
// These no longer exist:
stats.StatsNames
stats.StatsValues
stats.MatchSize()
stats.RemoveDuplicates()
```

❌ **FixedEntityStats Class Removed**:
```csharp
// This class no longer exists:
FixedEntityStats fixedStats;
```

## Migration Guide

### Migrating Existing Code

**If you used arrays directly**:
```csharp
// Old code:
stats.StatsNames.Add(EntityStatType.Health);
stats.StatsValues.Add(100f);

// New code:
stats.Health = 100f;
stats.InitializeStats();
```

**If you used dictionary access**:
```csharp
// No changes needed! This still works:
float health = stats[EntityStatType.Health].Value;
```

### Migrating Scene Files

Scene files (.tscn) with old array format will need to be updated:

**Old Format**:
```
[sub_resource type="Resource" id="EntityStats_xyz"]
script = ExtResource("...")
StatsNames = [1, 2, 3]  # Health, Mana, Stamina
StatsValues = [100.0, 100.0, 100.0]
```

**New Format**:
```
[sub_resource type="Resource" id="EntityStats_xyz"]
script = ExtResource("...")
Health = 100.0
Mana = 100.0
Stamina = 100.0
Agility = 0.0
PhysicalAttack = 0.0
MagicalAttack = 0.0
PhysicalDefense = 0.0
MagicalDefense = 0.0
KnockbackResistance = 0.0
```

You can update scenes manually or:
1. Open scene in Godot editor
2. Select entity node
3. Reconfigure Stats in inspector
4. Save scene

## Common Patterns

### Setting Up Player Stats

```csharp
var playerStats = new EntityStats
{
    Health = 100f,
    Mana = 100f,
    Stamina = 100f,
    Agility = 10f,
    PhysicalAttack = 15f,
    MagicalAttack = 15f,
    PhysicalDefense = 5f,
    MagicalDefense = 5f,
    KnockbackResistance = 3f
};
playerStats.InitializeStats();
```

### Scaling Enemy Stats by Level

```csharp
public EntityStats GetEnemyStats(int level)
{
    return new EntityStats
    {
        Health = 50f + (level * 10f),
        PhysicalAttack = 10f + (level * 2f),
        PhysicalDefense = 2f + (level * 0.5f),
        KnockbackResistance = 1f + (level * 0.2f)
    };
}
```

### Applying Stat Modifiers

```csharp
// Temporary buff (+20% attack for 10 seconds)
float originalAttack = entity.Stats[EntityStatType.PhysicalAttack].Value;
entity.Stats[EntityStatType.PhysicalAttack].Value *= 1.2f;

await ToSignal(GetTree().CreateTimer(10.0), "timeout");

entity.Stats[EntityStatType.PhysicalAttack].Value = originalAttack;
```

### Displaying Stats in UI

```csharp
public void UpdateStatsUI(EntityStats stats)
{
    healthLabel.Text = $"HP: {stats.Health:F0}";
    manaLabel.Text = $"MP: {stats.Mana:F0}";
    staminaLabel.Text = $"SP: {stats.Stamina:F0}";
    attackLabel.Text = $"ATK: {stats.PhysicalAttack:F0}";
    defenseLabel.Text = $"DEF: {stats.PhysicalDefense:F0}";
}
```

## Technical Details

### Internal Structure

EntityStats maintains:
1. **9 Export Properties** - For Godot editor configuration
2. **Stats Dictionary** - For runtime access and modification
3. **Event System** - For stat change notifications

### Initialization Process

```csharp
public void InitializeStats()
{
    // 1. Create empty dictionary with all stat types
    Stats = new Dictionary<EntityStatType, EntityStat>();
    
    // 2. Add all enum values with 0
    foreach (EntityStatType stat in Enum.GetValues(typeof(EntityStatType)))
    {
        Stats.Add(stat, new EntityStat(stat, 0));
    }
    
    // 3. Override with property values
    Stats[EntityStatType.Health] = new EntityStat(EntityStatType.Health, (int)Health);
    // ... for all 9 stats
    
    // 4. Subscribe to change events
    foreach (var stat in Stats.Values)
    {
        stat.StatChanged += OnStatChanged;
    }
}
```

### Why Properties Instead of Arrays?

**Design Goals**:
1. **Simplicity** - Each stat clearly named
2. **Safety** - Cannot add/remove stats accidentally
3. **Discoverability** - All stats visible in one place
4. **Maintainability** - No array synchronization needed
5. **Compatibility** - Dictionary access unchanged

## Troubleshooting

### Stats Not Updating

**Problem**: Changed property but stats don't reflect in game

**Solution**: Call `InitializeStats()` after changing properties:
```csharp
stats.Health = 200f;
stats.InitializeStats();  // Don't forget this!
```

### Scene Shows Old Array Format

**Problem**: Scene file still has `StatsNames` and `StatsValues`

**Solution**: 
1. Open scene in Godot editor
2. Select entity node
3. In Inspector, reconfigure Stats
4. Save scene

### Can't Find FixedEntityStats

**Problem**: Code references `FixedEntityStats` class

**Solution**: Use `EntityStats` directly:
```csharp
// Old:
[Export] public FixedEntityStats FixedStats;

// New:
[Export] public EntityStats Stats;
```

### Stats Dictionary is Empty

**Problem**: `Stats` dictionary has no values

**Solution**: Call `InitializeStats()` after creating or loading:
```csharp
var stats = new EntityStats();
stats.InitializeStats();  // Important!
```

## Best Practices

1. **Always Initialize**: Call `InitializeStats()` after creating or modifying properties
2. **Use Dictionary at Runtime**: Modify `stats[type].Value` during gameplay
3. **Use Properties for Setup**: Set properties in editor or during initialization
4. **Subscribe to Events**: Use `OnStatsChanged` for UI updates
5. **Validate Values**: Check stat values are reasonable (health > 0, etc.)

## Summary

EntityStats now has:
- ✅ 9 fixed, clearly named properties
- ✅ Clean Godot editor interface
- ✅ No resizable arrays
- ✅ Full backward compatibility
- ✅ Simpler initialization
- ✅ Better type safety

**Result**: A cleaner, more intuitive stats system that's easier to use in both the editor and code!
