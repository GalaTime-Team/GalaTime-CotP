using Godot;
using System;

namespace Galatime;

/// <summary>
/// Fixed entry for entity stat with type and value.
/// Used for a fixed-size stat configuration in the Godot editor.
/// </summary>
[GlobalClass]
public partial class EntityStatEntry : Resource
{
	/// <summary> The type of stat. </summary>
	[Export] public EntityStatType StatType { get; set; } = EntityStatType.Unsigned;
	
	/// <summary> The value of the stat. </summary>
	[Export] public float Value { get; set; } = 0f;
	
	public EntityStatEntry() { }
	
	public EntityStatEntry(EntityStatType type, float value)
	{
		StatType = type;
		Value = value;
	}
}

/// <summary>
/// Fixed-size entity stats with exactly 9 entries (one for each stat type).
/// Cannot add or remove stats, only modify values.
/// </summary>
[GlobalClass, Tool, Icon("res://assets/sprites/editoricons/stats.svg")]
public partial class FixedEntityStats : Resource
{
	// Fixed 9 stats - one for each EntityStatType (excluding Unsigned)
	[Export] public EntityStatEntry Health { get; set; } = new(EntityStatType.Health, 100f);
	[Export] public EntityStatEntry Mana { get; set; } = new(EntityStatType.Mana, 100f);
	[Export] public EntityStatEntry Stamina { get; set; } = new(EntityStatType.Stamina, 100f);
	[Export] public EntityStatEntry Agility { get; set; } = new(EntityStatType.Agility, 0f);
	[Export] public EntityStatEntry PhysicalAttack { get; set; } = new(EntityStatType.PhysicalAttack, 0f);
	[Export] public EntityStatEntry MagicalAttack { get; set; } = new(EntityStatType.MagicalAttack, 0f);
	[Export] public EntityStatEntry PhysicalDefense { get; set; } = new(EntityStatType.PhysicalDefense, 0f);
	[Export] public EntityStatEntry MagicalDefense { get; set; } = new(EntityStatType.MagicalDefense, 0f);
	[Export] public EntityStatEntry KnockbackResistance { get; set; } = new(EntityStatType.KnockbackResistance, 0f);
	
	/// <summary> Convert to the standard EntityStats format. </summary>
	public EntityStats ToEntityStats()
	{
		var stats = new EntityStats();
		
		// Add all non-zero stats
		if (Health.Value > 0) AddStat(stats, Health);
		if (Mana.Value > 0) AddStat(stats, Mana);
		if (Stamina.Value > 0) AddStat(stats, Stamina);
		if (Agility.Value > 0) AddStat(stats, Agility);
		if (PhysicalAttack.Value > 0) AddStat(stats, PhysicalAttack);
		if (MagicalAttack.Value > 0) AddStat(stats, MagicalAttack);
		if (PhysicalDefense.Value > 0) AddStat(stats, PhysicalDefense);
		if (MagicalDefense.Value > 0) AddStat(stats, MagicalDefense);
		if (KnockbackResistance.Value > 0) AddStat(stats, KnockbackResistance);
		
		stats.InitializeStats();
		return stats;
	}
	
	private void AddStat(EntityStats stats, EntityStatEntry entry)
	{
		stats.StatsNames.Add(entry.StatType);
		stats.StatsValues.Add(entry.Value);
	}
	
	/// <summary> Create from standard EntityStats. </summary>
	public static FixedEntityStats FromEntityStats(EntityStats stats)
	{
		var fixedStats = new FixedEntityStats();
		
		for (int i = 0; i < stats.StatsNames.Count; i++)
		{
			var type = stats.StatsNames[i];
			var value = stats.StatsValues[i];
			
			switch (type)
			{
				case EntityStatType.Health:
					fixedStats.Health.Value = value;
					break;
				case EntityStatType.Mana:
					fixedStats.Mana.Value = value;
					break;
				case EntityStatType.Stamina:
					fixedStats.Stamina.Value = value;
					break;
				case EntityStatType.Agility:
					fixedStats.Agility.Value = value;
					break;
				case EntityStatType.PhysicalAttack:
					fixedStats.PhysicalAttack.Value = value;
					break;
				case EntityStatType.MagicalAttack:
					fixedStats.MagicalAttack.Value = value;
					break;
				case EntityStatType.PhysicalDefense:
					fixedStats.PhysicalDefense.Value = value;
					break;
				case EntityStatType.MagicalDefense:
					fixedStats.MagicalDefense.Value = value;
					break;
				case EntityStatType.KnockbackResistance:
					fixedStats.KnockbackResistance.Value = value;
					break;
			}
		}
		
		return fixedStats;
	}
}
