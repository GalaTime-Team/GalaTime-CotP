using Galatime;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that makes the entity stay idle (do nothing).
/// </summary>
public class IdleBehavior : AIBehavior
{
    public IdleBehavior(float cooldown = 0f) : base("Idle", cooldown)
    {
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState) return;
        
        // Stop moving
        entity.Body.Velocity = Vector2.Zero;
    }
}
