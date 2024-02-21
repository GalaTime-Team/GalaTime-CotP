using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Galatime;

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

/// <summary> A custom JSON converter for the GalatimeElement class. </summary>
public class ElementConverter : JsonConverter<GalatimeElement>
{
    public override void WriteJson(JsonWriter writer, GalatimeElement value, JsonSerializer serializer)
    {
        // Serialize the element name as a string
        writer.WriteValue(value.Name);
    }

    public override GalatimeElement ReadJson(JsonReader reader, Type objectType, GalatimeElement existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Deserialize the element name from a string
        string name = (string)reader.Value;
        // Use the GalatimeElement.GetByName method to get the element object
        return GalatimeElement.GetByName(name);
    }
}

[GlobalClass, Tool]
public partial class GalatimeElement : Resource
{
    private string initializerElement = "Default";
    /// <summary> Initializer for a current element. For example you can initialize with "Chaos + Ignis" </summary>
    [Export]
    public string InitializerElement
    {
        get => initializerElement;
        set
        {
            initializerElement = value;
            var e = ConvertStringToElement(initializerElement);

            if (e is not null)
            {
                Name = e.Name;
                Description = e.Description;
                DamageMultipliers = e.DamageMultipliers;
            }
        }
    }

    /// <summary> Name of the element. </summary>
    public string Name = "Default";
    /// <summary> Description of the element. </summary>
    public string Description = "No description";

    /// <summary> Damage multipliers of the current element. </summary>
    public Dictionary<string, float> DamageMultipliers = new();

    /// <summary> A method that calculates the received damage from another element and returns a result object. </summary>
    /// <param name="e"> The element that is attacking this element. </param>
    /// <param name="amount"> The base damage amount of the attack. </param>
    /// <returns> A result object that contains the received damage value and the type of damage difference. </returns>
    public GalatimeElementDamageResult GetReceivedDamage(GalatimeElement e, float amount)
    {
        GalatimeElementDamageResult result = new();

        // Use the standard multiplier if no element against this element is found.
        if (!DamageMultipliers.ContainsKey(e.Name))
        {
            result.Damage = amount;
            return result;
        }

        // Calculate the received damage.
        var multiplier = DamageMultipliers[e.Name];
        result.Damage = amount * multiplier;

        // Determine the type of damage difference.
        result.Type = multiplier switch
        {
            1 => DamageDifferenceType.equal,
            > 1 => DamageDifferenceType.plus,
            _ => DamageDifferenceType.minus,
        };

        return result;
    }

    /// <summary> Converts a string to an element. </summary>
    /// <param name="elementsSum"> String in format "Ignis + Chaos" </param>
    public static GalatimeElement ConvertStringToElement(string elementsSum)
    {
        var elements = elementsSum.Split('+');
        GalatimeElement result = null;

        // Return one element if there's only one element.
        if (elements.Length == 1) return GetByName(elements[0].Trim());

        // Combine the elements.
        foreach (var element in elements)
        {
            string trimmedElement = element.Trim();
            GalatimeElement convertedElement = GetByName(trimmedElement);

            // If if the first element, set it as first.
            if (result == null)
                result = convertedElement;
            // Combine the elements.
            else
                if (convertedElement is not null) result += convertedElement;
        }

        return result;
    }

    /// <summary> Combines two elements by taking take the average of the two multipliers if there's. </summary>
    public static GalatimeElement operator +(GalatimeElement a, GalatimeElement b)
    {
        var c = new GalatimeElement
        {
            // Combine the names of the elements.
            Name = $"{a.Name} + {b.Name}",
            Description = $"The combination of {a.Name}, {b.Name} elements.",
            // Damage multipliers assigned to the new element.
            DamageMultipliers = a.DamageMultipliers
        };
        foreach (var elem in b.DamageMultipliers.Keys)
        {
            // Check if the element already exists, if it does, take the average of the two multipliers.
            if (c.DamageMultipliers.ContainsKey(elem))
            {
                c.DamageMultipliers[elem] = (b.DamageMultipliers[elem] + c.DamageMultipliers[elem]) / 2;
                // Skip the assignment to escape overriding the element.
                continue;
            }
            // Add the element to the dictionary if no multiplier.
            c.DamageMultipliers[elem] = b.DamageMultipliers[elem];
        }
        return c;
    }

    public static GalatimeElement Ignis
    {
        get
        {
            GalatimeElement e = new()
            {
                Name = "Ignis",
                Description = "This element has fiery abilities. Don't get burned!",
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
                Description = "The element that forces destruction. A very destructive thing.",
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
            };

            e.DamageMultipliers["Chaos"] = 2f;
            e.DamageMultipliers["Ignis"] = 0.75f;
            e.DamageMultipliers["Lux"] = 0.5f;
            e.DamageMultipliers["Naturaela"] = 2f;
            return e;
        }
    }

    public static GalatimeElement Lapis
    {
        get
        {
            GalatimeElement e = new()
            {
                Name = "Lapis",
                Description = "The element that gives life. It's very strong."
            };

            e.DamageMultipliers["Aqua"] = 2f;
            e.DamageMultipliers["Caeli"] = 0.5f;
            e.DamageMultipliers["Chaos"] = 2;
            e.DamageMultipliers["Ignis"] = 0.5f;
            e.DamageMultipliers["Lux"] = 0.5f;
            e.DamageMultipliers["Naturaela"] = 1f;
            e.DamageMultipliers["Corporis"] = 2f;
            e.DamageMultipliers["Tenerbis"] = 0.5f;
            return e;
        }
    }

    public static List<GalatimeElement> Elements = new() {
        Ignis,
        Chaos,
        Aqua,
        Lapis
    };

    public static GalatimeElement GetByName(string name) => Elements.Find(e => e.Name == name);
}
