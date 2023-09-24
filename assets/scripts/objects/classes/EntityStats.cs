using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    /// 
    public class EntityStats : IEnumerable<EntityStat>
    {
        private readonly Dictionary<EntityStatType, EntityStat> Stats;
        public delegate void EntityStatChangedDelegate(EntityStats stats);
        public event EntityStatChangedDelegate OnStatsChanged;

        public EntityStats(float physicalAttack = 51, float magicalAttack = 22,
            float physicalDefence = 51, float magicalDefence = 34,
            float health = 19, float mana = 15,
            float stamina = 12, float agility = 46)
        {
            Stats = new Dictionary<EntityStatType, EntityStat>
            {
                { EntityStatType.PhysicalAttack, new EntityStat(EntityStatType.PhysicalAttack, physicalAttack, "physicalAttack", "Physical Attack", "Gives more power to a physical attack. Most often needed for swords", "res://sprites/gui/stats/physical-attack.png") },
                { EntityStatType.MagicalAttack, new EntityStat(EntityStatType.MagicalAttack, magicalAttack, "magicalAttack", "Magical Attack", "Gives more power to magical attack. Most often needed for abilities", "res://sprites/gui/stats/magicall-attack.png") },
                { EntityStatType.PhysicalDefense, new EntityStat(EntityStatType.PhysicalDefense, physicalDefence, "physicalDefence", "Physical Defence", "Gives protection against physical attacks", "res://sprites/gui/stats/physical-defence-icon.png") },
                { EntityStatType.MagicalDefense, new EntityStat(EntityStatType.MagicalDefense, magicalDefence, "magicalDefence", "Magical Defence", "Gives protection against magical attacks", "res://sprites/gui/stats/magical-defence-icon.png") },
                { EntityStatType.Health, new EntityStat(EntityStatType.Health, health, "health", "Health", "Gives maximum amount of health", "res://sprites/gui/stats/health-icon.png") },
                { EntityStatType.Mana, new EntityStat(EntityStatType.Mana, mana, "mana", "Mana", "Gives maximum amount of mana", "res://sprites/gui/stats/mana-icon.png") },
                { EntityStatType.Stamina, new EntityStat(EntityStatType.Stamina, stamina, "stamina", "Stamina", "Gives maximum amount of stamina", "res://sprites/gui/stats/stamina-icon.png") },
                { EntityStatType.Agility, new EntityStat(EntityStatType.Agility, agility, "agility", "Agility", "Gives less time to reload and increases speed. DON'T EVEN THINK ABOUT UPGRADING IT! IT WON'T DO YOU ANY GOOD FOR NOW", "res://sprites/gui/stats/agility-icon.png") }
            };
            foreach (var item in this) item.StatChanged += _onStatChanged;
        }

        /// <summary>
        /// Forces the invocation of the StatsChanged event.
        /// </summary>
        /// <remarks>
        /// This method can be used to manually trigger the <see cref="OnStatsChanged"/> event, which is normally emitted when an <see cref="EntityStats"/> has changed.
        /// </remarks>
        public void ForceInvokeStatsChangedEvent()
        {
            OnStatsChanged?.Invoke(this);
        }

        private void _onStatChanged(EntityStat stat)
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
        /// <summary>
        /// Gets the <see cref="EntityStat"/> with the specified name.
        /// </summary>
        /// <param name="statName">The name of the <see cref="EntityStat"/> to get.</param>
        /// <returns>The <see cref="EntityStat"/> with the specified name.</returns>
        public EntityStat this[string statName]
        {
            get => Stats.Values.FirstOrDefault(stat => stat.ID == statName);
            set
            {
                var stat = Stats.Values.FirstOrDefault(stat => stat.ID == statName);
                stat = value;
                OnStatsChanged?.Invoke(this);
            }
        }

        public int Count => Stats.Count;
    }

    public class EntityStat
    {
        public delegate void EntityStatChangedDelegate(EntityStat stat);
        public event EntityStatChangedDelegate StatChanged;

        public EntityStat(EntityStatType type, float value, string id, string name, string description = "", string iconPath = "res://sprites/gui/stats/health-icon.png") =>
            (Type, Value, Name, Description, IconPath, ID) = (type, value, name, description, iconPath, id);

        public string Name;
        public string ID;
        public string Description;
        public string IconPath = "res://sprites/gui/stats/health-icon.png";
        private float value = 0;
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