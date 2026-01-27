using Galatime;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks if the entity's mana is below a certain percentage.
/// Only works for HumanoidCharacter entities.
/// </summary>
public class LowManaCondition : AICondition
{
    /// <summary> Mana percentage threshold (0-1). </summary>
    public float Threshold { get; set; }

    public LowManaCondition(float threshold = 0.3f) : base("LowMana")
    {
        Threshold = threshold;
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity is not HumanoidCharacter humanoid) return false;
        if (humanoid.Mana == null) return false;
        
        return humanoid.Mana.Value / humanoid.Mana.MaxValue <= Threshold;
    }
}
