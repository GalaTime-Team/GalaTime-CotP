using Galatime;
using Galatime.Helpers;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks if the entity has no target.
/// </summary>
public class NoTargetCondition : AICondition
{
    public NoTargetCondition() : base("NoTarget")
    {
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity == null) return true;
        
        // Try to find a TargetController on the entity
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null) return true;
        
        return targetController.CurrentTarget == null;
    }
}
