using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Galatime
{
    public enum EntityStatType
    {
        physicalAttack,
        magicalAttack,
        physicalDefence,
        magicalDefence,
        health,
        mana,
        stamina,
        agility
    }

    public class EntityStats : IEnumerable<EntityStat>
    {
        private Dictionary<EntityStatType, EntityStat> stats;

        public delegate void EntityStatChangedDelegate(EntityStats stats);
        public event EntityStatChangedDelegate statsChanged;

        public EntityStats(float physicalAttack = 51, float magicalAttack = 22,
            float physicalDefence = 51, float magicalDefence = 34,
            float health = 19, float mana = 15,
            float stamina = 12, float agility = 46)
        {
            stats = new Dictionary<EntityStatType, EntityStat>
            {
                { EntityStatType.physicalAttack, new EntityStat(EntityStatType.physicalAttack, physicalAttack, "physicalAttack", "Physical Attack", "Gives more power to a physical attack. Most often needed for swords", "res://sprites/gui/stats/physical-attack.png") },
                { EntityStatType.magicalAttack, new EntityStat(EntityStatType.magicalAttack, magicalAttack, "magicalAttack", "Magical Attack", "Gives more power to magical attack. Most often needed for abilities", "res://sprites/gui/stats/magicall-attack.png") },
                { EntityStatType.physicalDefence, new EntityStat(EntityStatType.physicalDefence, physicalDefence, "physicalDefence", "Physical Defence", "Gives protection against physical attacks", "res://sprites/gui/stats/physical-defence-icon.png") },
                { EntityStatType.magicalDefence, new EntityStat(EntityStatType.magicalDefence, magicalDefence, "magicalDefence", "Magical Defence", "Gives protection against magical attacks", "res://sprites/gui/stats/magical-defence-icon.png") },
                { EntityStatType.health, new EntityStat(EntityStatType.health, health, "health", "Health", "Gives maximum amount of health", "res://sprites/gui/stats/health-icon.png") },
                { EntityStatType.mana, new EntityStat(EntityStatType.mana, mana, "mana", "Mana", "Gives maximum amount of mana", "res://sprites/gui/stats/mana-icon.png") },
                { EntityStatType.stamina, new EntityStat(EntityStatType.stamina, stamina, "stamina", "Stamina", "Gives maximum amount of stamina", "res://sprites/gui/stats/stamina-icon.png") },
                { EntityStatType.agility, new EntityStat(EntityStatType.agility, agility, "agility", "Agility", "Gives less time to reload and increases speed. DON'T EVEN THINK ABOUT UPGRADING IT! IT WON'T DO YOU ANY GOOD FOR NOW", "res://sprites/gui/stats/agility-icon.png") }
            };
            foreach (var item in this)
            {
                item.statChanged += _onStatChanged;
            }
        }

        /// <summary>
        /// Forces the invocation of the StatsChanged event.
        /// </summary>
        /// <remarks>
        /// This method can be used to manually trigger the <see cref="statsChanged"/> event, which is normally emitted when an <see cref="EntityStats"/> has changed.
        /// </remarks>
        public void ForceInvokeStatsChangedEvent()
        {
            statsChanged?.Invoke(this);
        }

        private void _onStatChanged(EntityStat stat)
        {
            statsChanged?.Invoke(this);
        }

        public IEnumerator<EntityStat> GetEnumerator()
        {
            return stats.Values.GetEnumerator();
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
            get => stats[type];
            set
            {
                stats[type] = value;
                statsChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EntityStat"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the <see cref="EntityStat"/> to get or set.</param>
        /// <returns>The <see cref="EntityStat"/> at the specified index.</returns>
        public EntityStat this[int index]
        {
            get => stats.ElementAt(index).Value;
            set
            {
                stats[stats.ElementAt(index).Key] = value;
                statsChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Gets the <see cref="EntityStat"/> with the specified name.
        /// </summary>
        /// <param name="statName">The name of the <see cref="EntityStat"/> to get.</param>
        /// <returns>The <see cref="EntityStat"/> with the specified name.</returns>
        public EntityStat this[string statName]
        {
            get => stats.Values.FirstOrDefault(stat => stat.id == statName);
            set
            {
                var stat = stats.Values.FirstOrDefault(stat => stat.id == statName);
                stat = value;
                statsChanged?.Invoke(this);
            }
        }

        public int Count => stats.Count;
    }

    public class EntityStat
    {
        public delegate void EntityStatChangedDelegate(EntityStat stat);
        public event EntityStatChangedDelegate statChanged;

        public EntityStat(EntityStatType type, float value, string id, string name, string description = "", string iconPath = "res://sprites/gui/stats/health-icon.png")
        {
            this.type = type;
            this.Value = value;
            this.name = name;
            this.description = description;
            this.iconPath = iconPath;
            this.id = id;
        }
        public string name;
        public string id;
        public string description;
        public string iconPath = "res://sprites/gui/stats/health-icon.png";
        private float _value = 0;
        public float Value
        {
            get
            {
                return _value; 
            }
            set
            {
                _value = value;
                statChanged?.Invoke(this);
            }
        }
        public EntityStatType type;
    }

    public enum DamageType
    {
        physical,
        magical
    }
}