using Galatime;
using Galatime.Helpers;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that makes the entity strafe around its target.
/// </summary>
public class StrafeBehavior : AIBehavior
{
    /// <summary> Optimal distance to maintain from target. </summary>
    public float OptimalDistance { get; set; }

    /// <summary> Strafe direction (true = clockwise, false = counter-clockwise). </summary>
    public bool Clockwise { get; set; }

    public StrafeBehavior(float optimalDistance = 250f, bool clockwise = true, float cooldown = 0f) 
        : base("Strafe", cooldown)
    {
        OptimalDistance = optimalDistance;
        Clockwise = clockwise;
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState || !entity.CanMove) return;
        
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null || targetController.CurrentTarget == null) return;
        
        var target = targetController.CurrentTarget;
        float distance = entity.GlobalPosition.DistanceTo(target.GlobalPosition);
        var direction = entity.GlobalPosition.DirectionTo(target.GlobalPosition);
        
        // Calculate perpendicular direction for strafing
        var perpendicular = Clockwise 
            ? new Vector2(-direction.Y, direction.X)  // Rotate 90 degrees clockwise
            : new Vector2(direction.Y, -direction.X); // Rotate 90 degrees counter-clockwise
        
        Vector2 velocity = perpendicular * entity.Speed;
        
        // Adjust distance to target
        if (distance < OptimalDistance - 50f)
        {
            // Too close, move away
            velocity += -direction * entity.Speed * 0.5f;
        }
        else if (distance > OptimalDistance + 50f)
        {
            // Too far, move closer
            velocity += direction * entity.Speed * 0.5f;
        }
        
        entity.Body.Velocity = velocity.Normalized() * entity.Speed;
    }
}
