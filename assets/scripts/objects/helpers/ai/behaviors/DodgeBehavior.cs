using Galatime;
using Galatime.Helpers;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Behavior that makes the entity dodge/dash away from its target.
/// </summary>
public class DodgeBehavior : AIBehavior
{
    /// <summary> Distance to dodge. </summary>
    public float DodgeDistance { get; set; }

    /// <summary> Whether to consume stamina (for HumanoidCharacter). </summary>
    public bool ConsumeStamina { get; set; }

    /// <summary> Stamina cost. </summary>
    public float StaminaCost { get; set; }

    public DodgeBehavior(float dodgeDistance = 200f, bool consumeStamina = true, float staminaCost = 10f, float cooldown = 2f) 
        : base("Dodge", cooldown)
    {
        DodgeDistance = dodgeDistance;
        ConsumeStamina = consumeStamina;
        StaminaCost = staminaCost;
    }

    protected override void OnExecute(Entity entity, double delta)
    {
        if (entity == null || entity.DeathState || !entity.CanMove) return;
        
        // Check stamina for humanoid characters
        if (ConsumeStamina && entity is HumanoidCharacter humanoid)
        {
            if (humanoid.Stamina == null || humanoid.Stamina.Value < StaminaCost) return;
            humanoid.Stamina.Value -= StaminaCost;
        }
        
        var targetController = entity.GetNodeOrNull<TargetController>("TargetController");
        if (targetController != null && targetController.CurrentTarget != null)
        {
            // Dodge away from target
            var direction = targetController.CurrentTarget.GlobalPosition.DirectionTo(entity.GlobalPosition);
            entity.SetKnockback(DodgeDistance, direction.Angle());
        }
        else
        {
            // Dodge in a random direction if no target
            var randomAngle = GD.Randf() * Mathf.Tau;
            entity.SetKnockback(DodgeDistance, randomAngle);
        }
    }
}
