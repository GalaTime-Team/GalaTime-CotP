using Galatime;
using Galatime.Helpers;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks the distance between the entity and its target.
/// </summary>
public class TargetDistanceCondition : AICondition
{
    public enum DistanceType
    {
        LessThan,
        GreaterThan,
        Between
    }

    public DistanceType Type { get; set; }
    public float Distance { get; set; }
    public float MaxDistance { get; set; } // Used for Between type

    public TargetDistanceCondition(DistanceType type, float distance, float maxDistance = 0f) 
        : base($"TargetDistance{type}")
    {
        Type = type;
        Distance = distance;
        MaxDistance = maxDistance;
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity == null) return false;
        
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null || targetController.CurrentTarget == null) return false;
        
        float distance = entity.GlobalPosition.DistanceTo(targetController.CurrentTarget.GlobalPosition);
        
        return Type switch
        {
            DistanceType.LessThan => distance < Distance,
            DistanceType.GreaterThan => distance > Distance,
            DistanceType.Between => distance >= Distance && distance <= MaxDistance,
            _ => false
        };
    }
}
