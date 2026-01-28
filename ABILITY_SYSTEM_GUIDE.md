# Centralized Ability System Documentation

## Overview

The ability system has been centralized and enhanced to support both player/ally abilities and enemy attacks through a unified JSON-based configuration system. All abilities are now defined in `assets/data/json/abilities.json`.

## New Ability Properties

### Core Properties

- **name** (string): Display name of the ability
- **id** (string): Unique identifier (MUST be unique)
- **description** (string): Description shown in UI
- **element** (string): Element type (Ignis, Aqua, Terra, etc.)
- **ability_type** (enum): Type of ability
  - `Physical` - Physical damage ability
  - `Magical` - Magical damage ability
  - `SelfBuff` - Self-inflicted buff
  - `TargetBuff` - Buff applied to target
  - `Heal` - Healing ability
  - `Hybrid` - Combination of types

### Damage & Healing

- **attack** (float): Base attack damage amount
- **heal** (float): Healing amount (0 if not a healing ability)

### Resource Costs

- **costs.mana** (int): Mana cost to use the ability
- **costs.stamina** (int): Stamina cost to use the ability

### Timing

- **reload** (float): Cooldown time in seconds
- **duration** (float): How long the ability effect lasts

### Range & Area

- **range** (float): Maximum range in pixels
- **area_of_effect** (float): AOE radius (0 = single target)
- **projectile_speed** (float): Speed of projectile (0 = instant/melee)

### Special Properties

- **can_crit** (bool): Whether this ability can critical hit
- **stat_modifiers** (array): List of stat modifications

### Stat Modifiers

Each stat modifier has:
- **stat** (string): Stat to modify
  - "speed"
  - "physical_attack"
  - "magical_attack"
  - "defense"
  - "health"
  - etc.
- **value** (float): Amount to modify (can be negative)
- **duration** (float): How long the modifier lasts (0 = permanent)

Example:
```json
"stat_modifiers": [
  {
    "stat": "magical_attack",
    "value": 10,
    "duration": 30
  },
  {
    "stat": "speed",
    "value": -20,
    "duration": 5
  }
]
```

### Legacy Properties (for player abilities)

- **scene** (string): Path to the ability scene file
- **icon** (string): Path to the icon texture
- **cost** (int): XP cost to unlock
- **charges** (int): Number of charges before reload
- **required** (array): IDs of required abilities

## Included Abilities

### Player/Ally Abilities

1. **Fireball** (`fireball`)
   - Type: Magical
   - Attack: 20
   - Range: 800, AOE: 80
   - Mana: 10, Reload: 2s
   - 3 charges

2. **Blue Fireball** (`blue_fireball`)
   - Type: Magical
   - Attack: 25
   - Range: 800, AOE: 80
   - Mana: 10, Stamina: 5, Reload: 10s

3. **Flamethrower** (`flamethrower`)
   - Type: Magical
   - Attack: 2 (continuous)
   - Range: 300, AOE: 150
   - Mana: 10, Reload: 5s

4. **Fire Wave** (`firewave`)
   - Type: Magical
   - Attack: 30
   - Range: 600, AOE: 200
   - Mana: 10, Reload: 2s

5. **Blue Fire** (`bluefire`)
   - Type: SelfBuff
   - Stat Modifiers: +10 magical attack, +10 physical attack for 30s
   - Mana: 30, Reload: 90s

6. **Fire Bullet** (`firebullet`)
   - Type: Magical
   - Attack: 30
   - Range: 1000, Single target
   - Mana: 15, Reload: 2s
   - 3 charges

### Enemy Abilities

1. **Slime Melee** (`slime_melee`)
   - Type: Physical
   - Attack: 10
   - Range: 60, AOE: 50
   - Reload: 1s

2. **Firecloak Fireball** (`firecloak_fireball`)
   - Type: Magical
   - Attack: 15
   - Range: 700, AOE: 60
   - Reload: 2s

3. **Firecloak Dash** (`firecloak_dash`)
   - Type: Physical
   - Attack: 25
   - Range: 500, AOE: 80
   - Reload: 5s

4. **RockAnt Dig** (`rockant_dig`)
   - Type: Physical
   - Attack: 20
   - Range: 300, AOE: 100
   - Reload: 4s

5. **RockAnt Melee** (`rockant_melee`)
   - Type: Physical
   - Attack: 12
   - Range: 80, AOE: 60
   - Reload: 1.5s

## Adding New Abilities

### Step 1: Add to abilities.json

```json
{
  "name": "Ice Blast",
  "id": "ice_blast",
  "description": "Freezes enemies and slows them down",
  "element": "Aqua",
  "ability_type": "Magical",
  "attack": 15,
  "heal": 0,
  "reload": 3,
  "costs": {
    "mana": 15,
    "stamina": 0
  },
  "duration": 2,
  "range": 600,
  "area_of_effect": 100,
  "projectile_speed": 300,
  "can_crit": true,
  "stat_modifiers": [
    {
      "stat": "speed",
      "value": -30,
      "duration": 3
    }
  ],
  "scene": "res://assets/objects/abilities/IceBlast.tscn",
  "icon": "res://assets/sprites/gui/abilities/aqua/ice_blast.png",
  "cost": 200,
  "charges": 2
}
```

### Step 2: Create Scene (if needed)

For player abilities, create a scene that extends `GalatimeAbility`:

```csharp
public partial class IceBlast : GalatimeAbility
{
    public override void Execute(Entity entity)
    {
        // Implementation using Data properties
        float damage = Data.Attack;
        float range = Data.Range;
        // ... rest of implementation
    }
}
```

### Step 3: Load Ability

```csharp
// For entities
var abilityData = GalatimeGlobals.GetAbilityById("ice_blast");
entity.AddAbility(abilityData, 0);

// Use ability
entity.UseAbility(0);
```

## Code Usage Examples

### Accessing Ability Properties

```csharp
var ability = GalatimeGlobals.GetAbilityById("fireball");

// Access properties
float damage = ability.Attack;
float heal = ability.Heal;
AbilityType type = ability.Type;
int manaCost = ability.Costs.Mana;
int staminaCost = ability.Costs.Stamina;
float range = ability.Range;
float aoe = ability.AreaOfEffect;
bool canCrit = ability.CanCrit;

// Stat modifiers
foreach (var modifier in ability.StatModifiers)
{
    string stat = modifier.Stat;
    float value = modifier.Value;
    float duration = modifier.Duration;
    // Apply modifier...
}
```

### Checking Ability Type

```csharp
if (ability.Type == AbilityType.Magical)
{
    // Use magical attack stat
    damage = ability.Attack + entity.Stats[EntityStatType.MagicalAttack].Value;
}
else if (ability.Type == AbilityType.Physical)
{
    // Use physical attack stat
    damage = ability.Attack + entity.Stats[EntityStatType.PhysicalAttack].Value;
}
else if (ability.Type == AbilityType.Heal)
{
    // Apply healing
    entity.Health += ability.Heal;
}
```

### Applying Stat Modifiers

```csharp
foreach (var modifier in ability.StatModifiers)
{
    // Apply stat modification for specified duration
    entity.ApplyStatModifier(modifier.Stat, modifier.Value, modifier.Duration);
}
```

## Migration Guide

### For Existing Abilities

All existing player abilities have been migrated with their new properties:
- Default `ability_type` set to "Magical"
- `attack` set from existing "power" field
- New properties filled with reasonable defaults

### For Enemies

Enemy abilities are now defined in the JSON. To use them:

1. Load the ability by ID
2. Store in entity's ability list
3. Use through entity.UseAbility() or implement custom logic

Example for Slime:
```csharp
public override void _Ready()
{
    base._Ready();
    
    // Load slime's melee attack ability
    var meleeAbility = GalatimeGlobals.GetAbilityById("slime_melee");
    AddAbility(meleeAbility, 0);
}

// In attack logic
public void Attack(Node2D body)
{
    if (body is Entity entity)
    {
        var ability = Abilities[0];
        float damage = ability.Attack + Stats[EntityStatType.PhysicalAttack].Value;
        entity.TakeDamage(ability.Attack, damage, ability.Element, 
                         ability.Type == AbilityType.Physical ? DamageType.Physical : DamageType.Magical,
                         500, rotation);
    }
}
```

## Benefits

1. **Centralized Configuration**: All ability stats in one place
2. **Easy Balancing**: Adjust damage, costs, cooldowns without code changes
3. **Consistent Format**: Same structure for player and enemy abilities
4. **Extensibility**: Easy to add new properties
5. **Type Safety**: Enum types prevent invalid values
6. **Better Tooling**: JSON can be edited in any text editor or specialized tools

## Best Practices

1. **Unique IDs**: Always use unique IDs for abilities
2. **Sensible Defaults**: Set appropriate default values for all properties
3. **Balance**: Test abilities in-game and adjust values iteratively
4. **Documentation**: Add clear descriptions for each ability
5. **Naming**: Use consistent naming conventions (lowercase_with_underscores)
6. **Versioning**: Keep backups when making major changes

## Future Enhancements

Potential additions to the ability system:
- Effect types (burn, freeze, poison, etc.)
- Multi-hit abilities
- Chain abilities (cast one after another)
- Conditional modifiers (only in certain situations)
- Target type restrictions
- Animation properties
- Sound effect references
- Particle effect references

## Troubleshooting

### Ability Not Loading

- Check that ID is unique and matches exactly (case-sensitive)
- Verify JSON syntax is valid
- Ensure all required fields are present

### Stats Not Applying

- Check that stat names match the expected values
- Verify duration is set correctly
- Check that entity has the stat being modified

### Damage Not Working

- Verify `attack` value is set
- Check `ability_type` is correct (Physical vs Magical)
- Ensure entity has appropriate attack stats

## Summary

The centralized ability system provides a flexible, data-driven approach to managing abilities for all entities in the game. By defining abilities in JSON, balance changes and new abilities can be added quickly without modifying code.
