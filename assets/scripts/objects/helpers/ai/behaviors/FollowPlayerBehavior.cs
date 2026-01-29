using Galatime;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that makes the entity follow the player character.
/// </summary>
public class FollowPlayerBehavior : AIBehavior
{
    /// <summary> Distance at which to stop following. </summary>
    public float FollowDistance { get; set; }

    public FollowPlayerBehavior(float followDistance = 100f, float cooldown = 0f) 
        : base("FollowPlayer", cooldown)
    {
        FollowDistance = followDistance;
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState || !entity.CanMove) return;
        
        // Get the player character
        var player = Player.CurrentCharacter;
        if (player == null) return;
        
        float distance = entity.GlobalPosition.DistanceTo(player.GlobalPosition);
        
        // Follow if too far away
        if (distance > FollowDistance)
        {
            var navigation = entity.GetNodeOrNull<NavigationAgent2D>("Navigation");
            if (navigation != null)
            {
                navigation.TargetPosition = player.GlobalPosition;
                var direction = entity.GlobalPosition.DirectionTo(navigation.GetNextPathPosition());
                entity.Body.Velocity = direction * entity.Speed;
            }
            else
            {
                // Fallback: direct movement
                var direction = entity.GlobalPosition.DirectionTo(player.GlobalPosition);
                entity.Body.Velocity = direction * entity.Speed;
            }
        }
        else
        {
            entity.Body.Velocity = Vector2.Zero;
        }
    }
}
