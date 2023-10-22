using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Godot;

namespace Galatime
{
    /// <summary> Represents the types of entity stats. </summary>
    public enum EntityStatType
    {
        /// <summary> Represents the physical attack stat. Increases the power of physical attacks. </summary>
        PhysicalAttack,
        /// <summary> Represents the magical attack stat. Increases the power of magical attacks. </summary>
        MagicalAttack,
        /// <summary> Represents the physical defence stat. Increases the protection against physical attacks. </summary>
        PhysicalDefense,
        /// <summary> Represents the magical defence stat. Increases the protection against magical attacks. </summary>
        MagicalDefense,
        /// <summary> Represents the health stat. Determines the maximum amount of health. </summary>
        Health,
        /// <summary> Represents the mana stat. Determines the maximum amount of mana. </summary>
        Mana,
        /// <summary> Represents the stamina stat. Determines the maximum amount of stamina. </summary>
        Stamina,
        /// <summary> Represents the agility stat. Increases the reload speed. </summary>
        Agility
    }

    /// <summary> Represents the stats of an entity. Sorry about implementation. </summary>
    public partial class EntityStats : IEnumerable<EntityStat>
    {   
        private Dictionary<EntityStatType, EntityStat> Stats = new() {
            { EntityStatType.PhysicalAttack, new(EntityStatType.PhysicalAttack, 0) },
            { EntityStatType.MagicalAttack, new(EntityStatType.MagicalAttack, 0) },
            { EntityStatType.PhysicalDefense, new(EntityStatType.PhysicalDefense, 0) },
            { EntityStatType.MagicalDefense, new(EntityStatType.MagicalDefense, 0) },
            { EntityStatType.Health, new(EntityStatType.Health, 0) },
            { EntityStatType.Mana, new(EntityStatType.Mana, 0) },
            { EntityStatType.Stamina, new(EntityStatType.Stamina, 0) },
            { EntityStatType.Agility, new(EntityStatType.Agility, 0) }
        };
        public Action<EntityStats> OnStatsChanged;

        public EntityStats(int PhysicalAttack = 0, int MagicalAttack = 0 , int PhysicalDefense = 0, int MagicalDefense = 0, int Health = 0, int Mana = 0, int Stamina = 0, int Agility = 0) : base() {
            Stats[EntityStatType.PhysicalAttack].Value = PhysicalAttack;
            Stats[EntityStatType.MagicalAttack].Value = MagicalAttack;
            Stats[EntityStatType.PhysicalDefense].Value = PhysicalDefense;
            Stats[EntityStatType.MagicalDefense].Value = MagicalDefense;
            Stats[EntityStatType.Health].Value = Health;
            Stats[EntityStatType.Mana].Value = Mana;
            Stats[EntityStatType.Stamina].Value = Stamina;
            Stats[EntityStatType.Agility].Value = Agility;
            foreach (var item in this) item.StatChanged += OnStatChanged;
        }

        private void OnStatChanged(EntityStat stat)
        {
            OnStatsChanged?.Invoke(this);
        }

        public IEnumerator<EntityStat> GetEnumerator()
        {
            return Stats.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the <see cref="EntityStat"/> with the specified <see cref="EntityStatType"/>.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the <see cref="EntityStat"/> at the specified index.
        /// </summary>
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

        public int Count => Stats.Count;
    }

    public class EntityStat
    {
        public Action<EntityStat> StatChanged;

        public EntityStat(EntityStatType type, int value) => (Type, Value) = (type, value);

        private float value;
        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                StatChanged?.Invoke(this);
            }
        }
        public EntityStatType Type;
    }

    public enum DamageType
    {
        physical,
        magical
    }
}