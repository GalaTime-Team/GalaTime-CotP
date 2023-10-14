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

    [Tool, GlobalClass, Icon("res://sprites/editoricons/stats.svg")]
    public partial class EntityStats : Resource, IEnumerable<EntityStat>
    {   
        private Dictionary<EntityStatType, EntityStat> Stats = new()
        {
            { EntityStatType.PhysicalAttack, new EntityStat(EntityStatType.PhysicalAttack, "physicalAttack", "Physical Attack", "Gives more power to a physical attack. Most often needed for swords", "res://sprites/gui/stats/physical-attack.png") },
            { EntityStatType.MagicalAttack, new EntityStat(EntityStatType.MagicalAttack, "magicalAttack", "Magical Attack", "Gives more power to magical attack. Most often needed for abilities", "res://sprites/gui/stats/magicall-attack.png") },
            { EntityStatType.PhysicalDefense, new EntityStat(EntityStatType.PhysicalDefense, "physicalDefence", "Physical Defence", "Gives protection against physical attacks", "res://sprites/gui/stats/physical-defence-icon.png") },
            { EntityStatType.MagicalDefense, new EntityStat(EntityStatType.MagicalDefense, "magicalDefence", "Magical Defence", "Gives protection against magical attacks", "res://sprites/gui/stats/magical-defence-icon.png") },
            { EntityStatType.Health, new EntityStat(EntityStatType.Health, "health", "Health", "Gives maximum amount of health", "res://sprites/gui/stats/health-icon.png") },
            { EntityStatType.Mana, new EntityStat(EntityStatType.Mana, "mana", "Mana", "Gives maximum amount of mana", "res://sprites/gui/stats/mana-icon.png") },
            { EntityStatType.Stamina, new EntityStat(EntityStatType.Stamina, "stamina", "Stamina", "Gives maximum amount of stamina", "res://sprites/gui/stats/stamina-icon.png") },
            { EntityStatType.Agility, new EntityStat(EntityStatType.Agility, "agility", "Agility", "Gives less time to reload and increases speed. DON'T EVEN THINK ABOUT UPGRADING IT! IT WON'T DO YOU ANY GOOD FOR NOW", "res://sprites/gui/stats/agility-icon.png") }
        };
        public Action<EntityStats> OnStatsChanged;

        public EntityStats() : base () {
            foreach (var item in this) item.StatChanged += _onStatChanged;
        }

        public override Variant _Get(StringName property)
        {
            foreach (var item in this) {
                if (item.Name == property) return item.Value;
            }
            return default;
        }

        public override bool _Set(StringName property, Variant value)
        {
            foreach (var item in this) {
                if (item.Name == property.ToString()) item.Value = value.AsInt16();
            }
            return true;
        }

        public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
        {
            var properties = new Godot.Collections.Array<Godot.Collections.Dictionary>();
            foreach (var item in this) {
                var property = new Godot.Collections.Dictionary() {
                    { "name", item.Name },
                    { "type", (int)Variant.Type.Int },
                    { "hint", (int)PropertyHint.Range },
                    { "hint_string", "0,999" }
                };
                properties.Add(property);
            }
            return properties;
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
        public Action<EntityStat> StatChanged;

        public EntityStat(EntityStatType type, string id, string name, string description = "", string iconPath = "res://sprites/gui/stats/health-icon.png") =>
            (Type, Name, Description, IconPath, ID) = (type, name, description, iconPath, id);

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