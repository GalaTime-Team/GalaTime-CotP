using Galatime;
using Galatime.Helpers;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that makes the entity flee from its target.
/// </summary>
public class FleeBehavior : AIBehavior
{
    /// <summary> Minimum distance to maintain from target. </summary>
    public float FleeDistance { get; set; }

    public FleeBehavior(float fleeDistance = 400f, float cooldown = 0f) 
        : base("Flee", cooldown)
    {
        FleeDistance = fleeDistance;
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState || !entity.CanMove) return;
        
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null || targetController.CurrentTarget == null) return;
        
        var target = targetController.CurrentTarget;
        float distance = entity.GlobalPosition.DistanceTo(target.GlobalPosition);
        
        // Only flee if target is close
        if (distance < FleeDistance)
        {
            // Move away from target
            var direction = target.GlobalPosition.DirectionTo(entity.GlobalPosition);
            entity.Body.Velocity = direction * entity.Speed * 1.2f; // Flee faster
        }
        else
        {
            entity.Body.Velocity = Vector2.Zero;
        }
    }
}
