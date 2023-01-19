using Godot;
using System;

namespace Galatime
{
    public struct EntityStats
    {
        public EntityStats()
        {

        }
        public float physicalAttack = 21;
        public float magicalAttack = 22;
        public float physicalDefence = 10;
        public float magicalDefense = 34;
        public float health = 19;
        public float mana = 15;
        public float stamina = 12;
        public float agility = 46;
    }
    public enum DamageType 
    { 
        physical,
        magical
    }
}