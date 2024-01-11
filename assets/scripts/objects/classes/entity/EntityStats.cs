using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Galatime;

/// <summary> Represents the types of entity stats. </summary>
public enum EntityStatType
{
    Unsigned,
    /// <summary> Represents the health stat. Determines the maximum amount of health. </summary>
    Health,
    /// <summary> Represents the mana stat. Determines the maximum amount of mana. </summary>
    Mana,
    /// <summary> Represents the stamina stat. Determines the maximum amount of stamina. </summary>
    Stamina,
    /// <summary> Represents the agility stat. Increases the reload speed. </summary>
    Agility,
    /// <summary> Represents the physical attack stat. Increases the power of physical attacks. </summary>
    PhysicalAttack,
    /// <summary> Represents the magical attack stat. Increases the power of magical attacks. </summary>
    MagicalAttack,
    /// <summary> Represents the physical defense stat. Increases the protection against physical attacks. </summary>
    PhysicalDefense,
    /// <summary> Represents the magical defense stat. Increases the protection against magical attacks. </summary>
    MagicalDefense,
    /// <summary> Represents the knockback resistance stat. Decreases the knockback from the damage. </summary>
    KnockbackResistance
}

/// <summary> This class represents a collection of entity stats, such as health, mana, stamina, etc. </summary>
[GlobalClass, Tool, Icon("res://sprites/editoricons/stats.svg")]
public partial class EntityStats : Resource, IEnumerable<EntityStat>
{
    private Godot.Collections.Array<EntityStatType> statsNames = new();
    [Export]
    public Godot.Collections.Array<EntityStatType> StatsNames
    {
        get => statsNames; set
        {
            statsNames = value;
            RemoveDuplicates(statsNames);
            MatchSize(statsValues, statsNames);
        }
    }
    private Godot.Collections.Array<float> statsValues = new();
    [Export]
    public Godot.Collections.Array<float> StatsValues
    {
        get => statsValues; set
        {
            statsValues = value;
            MatchSize(statsValues, statsNames);
            InitializeStats();
        }
    }

    // Matches the size of two arrays by adding default values to the smaller array or removing values from the larger array.
    public void MatchSize<[MustBeVariant] T, [MustBeVariant] T2>(Godot.Collections.Array<T> a, Godot.Collections.Array<T2> b)
    {
        while (a.Count < b.Count) a.Add(default);
        while (a.Count > b.Count) a.RemoveAt(a.Count - 1);
    }

    public Godot.Collections.Array<EntityStatType> RemoveDuplicates(Godot.Collections.Array<EntityStatType> a)
    {
        var tempArray = a;
        for (int i = 0; i < tempArray.Count; i++)
        {
            for (int j = i + 1; j < tempArray.Count; j++)
            {
                if (tempArray[i] == tempArray[j] && tempArray[i] != EntityStatType.Unsigned) a.RemoveAt(j);
            }
        }
        return a;
    }

    /// <summary> This dictionary stores the stats by their type as the key and the EntityStat object as the value. </summary>
    public Dictionary<EntityStatType, EntityStat> Stats { get; set; } = new();
    /// <summary> This event is triggered when any of the stats changes its value. </summary>
    public Action<EntityStats> OnStatsChanged;

    public void InitializeStats()
    {
        // Initialize the dictionary by enum values.
        Stats = new();
        foreach (EntityStatType stat in Enum.GetValues(typeof(EntityStatType)))
        {
            Stats.Add(stat, new EntityStat(stat, 0));

            // Subscribe to the StatChanged event of the EntityStat object.
            Stats[stat].StatChanged += OnStatChanged;
        }

        // Initialize the dictionary by editor values.
        for (int i = 0; i < StatsNames.Count; i++)
        {
            EntityStatType statType = StatsNames[i];
            Stats[statType] = new EntityStat(statType, (int)StatsValues[i]);
        }
    }

    private void OnStatChanged(EntityStat stat) => OnStatsChanged?.Invoke(this);
    public IEnumerator<EntityStat> GetEnumerator() => Stats.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary> Gets or sets the <see cref="EntityStat"/> with the specified <see cref="EntityStatType"/>. </summary>
    /// <param name="type">The <see cref="EntityStatType"/> of the <see cref="EntityStat"/> to get or set.</param>
    /// <returns>The <see cref="EntityStat"/> with the specified <see cref="EntityStatType"/>.</returns>
    public EntityStat this[EntityStatType type]
    {
        get => Stats[type];
        set
        {
            Stats[type] = value;
            OnStatsChanged?.Invoke(this);
        }
    }

    /// <summary>  Gets or sets the <see cref="EntityStat"/> at the specified index. </summary>
    /// <param name="index">The zero-based index of the <see cref="EntityStat"/> to get or set.</param>
    /// <returns>The <see cref="EntityStat"/> at the specified index.</returns>
    public EntityStat this[int index]
    {
        get => Stats.ElementAt(index).Value;
        set
        {
            Stats[Stats.ElementAt(index).Key] = value;
            OnStatsChanged?.Invoke(this);
        }
    }

    /// <summary> Current count of stats. </summary>
    public int Count => Stats.Count;
}

public class EntityStat
{
    /// <summary> This event is triggered when stat changes its value </summary>
    public Action<EntityStat> StatChanged;

    public EntityStat(EntityStatType type, int value) => (Type, Value) = (type, value);

    private float value;
    /// <summary> Value of stat </summary>
    public float Value
    {
        get => value;
        set
        {
            this.value = value;
            StatChanged?.Invoke(this);
        }
    }
    /// <summary> Type/Name of stat </summary>
    public EntityStatType Type;
}

public enum DamageType
{
    Physical,
    Magical
}