using Galatime;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks if the entity's health is below a certain percentage.
/// </summary>
public class LowHealthCondition : AICondition
{
    /// <summary> Health percentage threshold (0-1). </summary>
    public float Threshold { get; set; }

    public LowHealthCondition(float threshold = 0.3f) : base("LowHealth")
    {
        Threshold = threshold;
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity == null || entity.Stats == null) return false;
        
        var maxHealth = entity.Stats[EntityStatType.Health].Value;
        if (maxHealth <= 0) return false;
        
        return entity.Health / maxHealth <= Threshold;
    }
}
