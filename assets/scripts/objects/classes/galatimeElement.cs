using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Galatime
{
    public enum DamageDifferenceType
    {
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

    [Tool]
    public partial class GalatimeElement : Resource
    {
        public string Name = "Default";
        public string Description = "No description";

        public float AttackPhysics = 1.0f;
        public float AttackMagic = 1.0f;
        public float DefensePhysics = 1.0f;
        public float DefenseMagic = 1.0f;
        public float HpMultiplier = 1.0f;
        public float ManaMultiplier = 1.0f;
        public float StaminaMultiplier = 1.0f;
        public float AgilityMultiplier = 1.0f;

        public Dictionary<string, float> DamageMultipliers = new();

        /// <summary>
        /// Gets damage in float from the source of the damage, depending on its element
        /// </summary>
        public GalatimeElementDamageResult GetReceivedDamage(GalatimeElement e, float amount)
        {
            GalatimeElementDamageResult result = new();
            if (!DamageMultipliers.ContainsKey(e.Name))
            {
                GD.Print("No element is found, the standard multiplier will be used (1x)");
                result.Damage = amount;
                return result;
            }
            float multiplier = (float)DamageMultipliers[e.Name];
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
                Name = a.Name + " + " + b.Name
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
                    Name = "Ignis",
                    Description = "This element has fiery abilities. Don't get burned!",
                    AttackMagic = 1.25f,
                    DefenseMagic = 1.25f,
                    ManaMultiplier = 0.8f,
                    StaminaMultiplier = 0.8f
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
                    Name = "Chaos",
                    Description = "The element that forces destruction. A very destructive thing",
                    AttackMagic = 0.75f,
                    StaminaMultiplier = 1.25f,
                    AgilityMultiplier = 0.75f
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
                    Name = "Aqua",
                    Description = "This element has the power of water. Do not drown!",

                    AttackMagic = 0.75f,
                    StaminaMultiplier = 1.25f,
                    AgilityMultiplier = 0.75f
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
            foreach (GalatimeElement e in Elements) if (e.Name == name) return e;
            return null;
        }
    }
}
