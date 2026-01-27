using Galatime;
using Galatime.Helpers;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that uses an entity's ranged ability on its target.
/// </summary>
public class RangedAttackBehavior : AIBehavior
{
    /// <summary> Index of the ability to use (0-2). </summary>
    public int AbilityIndex { get; set; }

    /// <summary> Whether to strafe while using the ability. </summary>
    public bool StrafeWhileAttacking { get; set; }

    /// <summary> Optimal distance to maintain from target. </summary>
    public float OptimalDistance { get; set; }

    public RangedAttackBehavior(int abilityIndex = 0, bool strafe = true, float optimalDistance = 300f, float cooldown = 1f) 
        : base($"RangedAttack{abilityIndex}", cooldown)
    {
        AbilityIndex = abilityIndex;
        StrafeWhileAttacking = strafe;
        OptimalDistance = optimalDistance;
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState) return;
        
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController == null || targetController.CurrentTarget == null) return;
        
        // Use the ability
        entity.UseAbility(AbilityIndex);
        
        // Position maintenance
        if (StrafeWhileAttacking && entity.CanMove)
        {
            var target = targetController.CurrentTarget;
            float distance = entity.GlobalPosition.DistanceTo(target.GlobalPosition);
            var direction = entity.GlobalPosition.DirectionTo(target.GlobalPosition);
            
            // Move away if too close
            if (distance < OptimalDistance - 50f)
            {
                entity.Body.Velocity = -direction * entity.Speed;
            }
            // Move closer if too far
            else if (distance > OptimalDistance + 50f)
            {
                entity.Body.Velocity = direction * entity.Speed;
            }
            else
            {
                // Strafe perpendicular to target
                var perpendicular = new Vector2(-direction.Y, direction.X);
                entity.Body.Velocity = perpendicular * entity.Speed * 0.7f;
            }
        }
    }
}
