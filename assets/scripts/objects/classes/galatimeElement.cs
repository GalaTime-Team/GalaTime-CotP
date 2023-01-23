using Godot;
using System;
using System.Collections.Generic;

namespace Galatime
{
    public class GalatimeElementDamageResult {
        public float damage = 0;
        public float multiplier = 1;
        public string type = "equal";
    }

    public class GalatimeElement : Godot.Object
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

        public Dictionary<string, float> DamageMultipliers = new Dictionary<string, float>();
        
        /// <summary>
        /// Gets damage in float from the source of the damage, depending on its element
        /// </summary>
        public GalatimeElementDamageResult getReceivedDamage(GalatimeElement e, float amount) {
            GalatimeElementDamageResult result = new GalatimeElementDamageResult();
            if (!DamageMultipliers.ContainsKey(e.name)) {
                GD.PushWarning("No element is found, the standard multiplier will be used (1x)");
                result.damage = amount;
                return result;
            }
            float multiplier = (float)DamageMultipliers[e.name];
            float damage = amount * multiplier;
            // GD.Print("Received element: " + e.name + ", Element: " + name + ". Multiplier: " + multiplier);
            result.damage = damage;
            if (multiplier == 1)
            {
                result.type = "equal";
            }
            else if (multiplier > 1)
            {
                result.type = "plus";
            }
            else
            {
                result.type = "minus";
            }
            return result;
        }

        public static GalatimeElement operator +(GalatimeElement a, GalatimeElement b)
        {
            var c = new GalatimeElement();
            c.name = a.name + " + " + b.name;
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
                GalatimeElement e = new GalatimeElement();
                e.name = "Ignis";
                e.description = "This element has fiery abilities. Don't get burned!";

                e.attackMagic = 1.25f;
                e.defenseMagic = 1.25f;
                e.manaMultiplier = 0.8f;
                e.staminaMultiplier = 0.8f;

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
                GalatimeElement e = new GalatimeElement();
                e.name = "Chaos";
                e.description = "The element that forces destruction. A very destructive thing";

                e.attackMagic = 0.75f;
                e.staminaMultiplier = 1.25f;
                e.agilityMultiplier = 0.75f;

                e.DamageMultipliers["Caeli"] = 0.5f;
                e.DamageMultipliers["Lapis"] = 0.5f;
                e.DamageMultipliers["Lux"] = 2f;
                e.DamageMultipliers["Spatium"] = 2f;
                return e;
            }
        }

        public static GalatimeElement Aqua {
            get {
                GalatimeElement e = new GalatimeElement();
                e.name = "Aqua";
                e.description = "This element has the power of water. Do not drown!";

                e.attackMagic = 0.75f;
                e.staminaMultiplier = 1.25f;
                e.agilityMultiplier = 0.75f;

                e.DamageMultipliers["Chaos"] = 2f;
                e.DamageMultipliers["Ignis"] = 0.75f;
                e.DamageMultipliers["Lux"] = 0.5f;
                e.DamageMultipliers["Naturaela"] = 2f;
                return e;
            }
        }
    }
}
