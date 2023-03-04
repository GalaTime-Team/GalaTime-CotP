using Godot;
using System;
using System.Collections.Generic;

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

    public struct EntityStats
    {
        public EntityStat physicalAttack = new EntityStat(EntityStatType.physicalAttack, 51, "physicalAttack", "Physical Attack",
            "Gives more power to a physical attack. Most often needed for swords", "res://sprites/gui/stats/physical-attack.png");
        public EntityStat magicalAttack = new EntityStat(EntityStatType.magicalAttack, 22, "magicalAttack", "Magical Attack",
            "Gives more power to magical attack. Most often needed for abilities", "res://sprites/gui/stats/magicall-attack.png");
        public EntityStat physicalDefence = new EntityStat(EntityStatType.physicalDefence, 51, "physicalDefence", "Physical Defence",
            "Gives protection against physical attacks", "res://sprites/gui/stats/physical-defence-icon.png");
        public EntityStat magicalDefence = new EntityStat(EntityStatType.magicalDefence, 34, "magicalDefence", "Magical Defence",
            "Gives protection against magical attacks", "res://sprites/gui/stats/magical-defence-icon.png");
        public EntityStat health = new EntityStat(EntityStatType.health, 19, "health", "Health",
            "Gives maximum amount of health", "res://sprites/gui/stats/health-icon.png");
        public EntityStat mana = new EntityStat(EntityStatType.mana, 15, "mana", "Mana",
            "Gives maximum amount of mana", "res://sprites/gui/stats/mana-icon.png");
        public EntityStat stamina = new EntityStat(EntityStatType.stamina, 12, "stamina", "Stamina",
            "Gives maximum amount of stamina", "res://sprites/gui/stats/stamina-icon.png");
        public EntityStat agility = new EntityStat(EntityStatType.agility, 46, "agility", "Agility",
            "Gives less time to reload and increases speed. DON'T EVEN THINK ABOUT UPGRADING IT! IT WON'T DO YOU ANY GOOD FOR NOW", "res://sprites/gui/stats/agility-icon.png");
        public EntityStats(float physicalAttack = 51, float magicalAttack = 22,
            float physicalDefence = 51, float magicalDefence = 34,
            float health = 19, float mana = 15,
            float stamina = 12, float agility = 46)
        {
            this.physicalAttack.value = physicalAttack;
            this.magicalAttack.value = magicalAttack;
            this.physicalDefence.value = physicalDefence;
            this.magicalDefence.value = magicalDefence;
            this.health.value = health;
            this.mana.value = mana;
            this.stamina.value = stamina;
            this.agility.value = agility;
        }
    }

    public struct EntityStat
    {
        public EntityStat(EntityStatType type, float value, string id, string name, string description = "", string iconPath = "res://sprites/gui/stats/health-icon.png")
        {
            this.type = type;
            this.value = value;
            this.name = name;
            this.description = description;
            this.iconPath = iconPath;
            this.id = id;
        }
        public string name;
        public string id;
        public string description;
        public string iconPath = "res://sprites/gui/stats/health-icon.png";
        public float value = 0;
        public EntityStatType type;
    }

    public enum DamageType
    {
        physical,
        magical
    }
}