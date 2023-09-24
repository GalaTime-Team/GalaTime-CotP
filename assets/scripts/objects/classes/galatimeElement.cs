using Godot;
using System.Collections.Generic;

namespace Galatime
{
    public enum DamageDifferenceType {
        equal,
        plus,
        minus,
        heal
    }   

    public partial class GalatimeElementDamageResult
    {
        public float Damage = 0;
        public float Multiplier = 1;
        public DamageDifferenceType Type = DamageDifferenceType.equal;
    }

    public partial class GalatimeElement : Godot.Node
    {
        public int id = 0;
        public string name = "Default";
        public string description = "No description";

        public float attackPhysics = 1.0f;
        public float attackMagic = 1.0f;
        public float defensePhysics = 1.0f;
        public float defenseMagic = 1.0f;
        public float hpMultiplier = 1.0f;
        public float manaMultiplier = 1.0f;
        public float staminaMultiplier = 1.0f;
        public float agilityMultiplier = 1.0f;

        public Dictionary<string, float> DamageMultipliers = new();

        /// <summary>
        /// Gets damage in float from the source of the damage, depending on its element
        /// </summary>
        public GalatimeElementDamageResult GetReceivedDamage(GalatimeElement e, float amount)
        {
            GalatimeElementDamageResult result = new();
            if (!DamageMultipliers.ContainsKey(e.name))
            {
                GD.Print("No element is found, the standard multiplier will be used (1x)");
                result.Damage = amount;
                return result;
            }
            float multiplier = (float)DamageMultipliers[e.name];
            float damage = amount * multiplier;
            result.Damage = damage;
            result.Type = multiplier switch
            {
                1 => DamageDifferenceType.equal,
                > 1 => DamageDifferenceType.plus,
                _ => DamageDifferenceType.minus,
            };

            return result;
        }

        public static GalatimeElement operator +(GalatimeElement a, GalatimeElement b)
        {
            var c = new GalatimeElement
            {
                name = a.name + " + " + b.name
            };
            foreach (var elem in a.DamageMultipliers.Keys)
            {
                c.DamageMultipliers[elem] = a.DamageMultipliers[elem];
            }
            foreach (var elem in b.DamageMultipliers.Keys)
            {
                if (c.DamageMultipliers.ContainsKey(elem))
                {
                    c.DamageMultipliers[elem] = (b.DamageMultipliers[elem] + c.DamageMultipliers[elem]) / 2;
                    continue;
                }
                c.DamageMultipliers[elem] = b.DamageMultipliers[elem];
            }
            return c;
        }

        /// <summary>
        /// Fire element
        /// </summary>
        public static GalatimeElement Ignis
        {
            get
            {
                GalatimeElement e = new()

                {
                    name = "Ignis",
                    description = "This element has fiery abilities. Don't get burned!",
                    attackMagic = 1.25f,
                    defenseMagic = 1.25f,
                    manaMultiplier = 0.8f,
                    staminaMultiplier = 0.8f
                };

                e.DamageMultipliers["Aqua"] = 2f;
                e.DamageMultipliers["Caeli"] = 2f;
                e.DamageMultipliers["Naturaela"] = 0.5f;
                e.DamageMultipliers["Tenerbis"] = 0.5f;
                return e;
            }
        }

        public static GalatimeElement Chaos
        {
            get
            {
                GalatimeElement e = new()
                {
                    name = "Chaos",
                    description = "The element that forces destruction. A very destructive thing",
                    attackMagic = 0.75f,
                    staminaMultiplier = 1.25f,
                    agilityMultiplier = 0.75f
                };

                e.DamageMultipliers["Caeli"] = 0.5f;
                e.DamageMultipliers["Lapis"] = 0.5f;
                e.DamageMultipliers["Lux"] = 2f;
                e.DamageMultipliers["Spatium"] = 2f;
                return e;
            }
        }

        public static GalatimeElement Aqua
        {
            get
            {
                GalatimeElement e = new()
                {
                    name = "Aqua",
                    description = "This element has the power of water. Do not drown!",

                    attackMagic = 0.75f,
                    staminaMultiplier = 1.25f,
                    agilityMultiplier = 0.75f
                };

                e.DamageMultipliers["Chaos"] = 2f;
                e.DamageMultipliers["Ignis"] = 0.75f;
                e.DamageMultipliers["Lux"] = 0.5f;
                e.DamageMultipliers["Naturaela"] = 2f;
                return e;
            }
        }

        public static List<GalatimeElement> Elements = new() {
            Ignis,
            Chaos,
            Aqua
        };

        public static GalatimeElement GetByName(string name)
        {
            foreach (GalatimeElement e in Elements) if (e.name == name) return e;
            return null;
        }
    }
}
