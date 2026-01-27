using Galatime;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks if the entity's stamina is below a certain percentage.
/// Only works for HumanoidCharacter entities.
/// </summary>
public class LowStaminaCondition : AICondition
{
    /// <summary> Stamina percentage threshold (0-1). </summary>
    public float Threshold { get; set; }

    public LowStaminaCondition(float threshold = 0.3f) : base("LowStamina")
    {
        Threshold = threshold;
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity is not HumanoidCharacter humanoid) return false;
        if (humanoid.Stamina == null) return false;
        
        return humanoid.Stamina.Value / humanoid.Stamina.MaxValue <= Threshold;
    }
}
