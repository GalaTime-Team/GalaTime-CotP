using Galatime;
using Galatime.Helpers;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that moves the entity toward its target for melee combat.
/// </summary>
public class MeleeAttackBehavior : AIBehavior
{
    /// <summary> Distance at which to stop moving toward target. </summary>
    public float StopDistance { get; set; }

    public MeleeAttackBehavior(float stopDistance = 50f, float cooldown = 0f) 
        : base("MeleeAttack", cooldown)
    {
        StopDistance = stopDistance;
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState) return;
        
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null || targetController.CurrentTarget == null) return;
        
        var target = targetController.CurrentTarget;
        float distance = entity.GlobalPosition.DistanceTo(target.GlobalPosition);
        
        // Move toward target if not close enough
        if (distance > StopDistance && entity.CanMove)
        {
            var navigation = entity.GetNodeOrNull<NavigationAgent2D>("Navigation");
            if (navigation != null)
            {
                navigation.TargetPosition = target.GlobalPosition;
                var direction = entity.GlobalPosition.DirectionTo(navigation.GetNextPathPosition());
                entity.Body.Velocity = direction * entity.Speed;
            }
            else
            {
                // Fallback: direct movement toward target
                var direction = entity.GlobalPosition.DirectionTo(target.GlobalPosition);
                entity.Body.Velocity = direction * entity.Speed;
            }
        }
        else
        {
            // Close enough, stop moving
            entity.Body.Velocity = Vector2.Zero;
        }
    }
}
