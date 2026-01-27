using Galatime;
using Galatime.Helpers;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks if the entity has a target.
/// </summary>
public class HasTargetCondition : AICondition
{
    public HasTargetCondition() : base("HasTarget")
    {
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity == null) return false;
        
        // Try to find a TargetController on the entity
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null) return false;
        
        return targetController.CurrentTarget != null;
    }
}
