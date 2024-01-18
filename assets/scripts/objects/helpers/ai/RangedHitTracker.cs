using Galatime.Global;
using Godot;

namespace Galatime.Helpers;

/// <summary> Used to track if the entity is can hit by a ranged attack. </summary>
public partial class RangedHitTracker : RayCast2D
{
    /// <summary> The range of check. </summary>
    [Export] public float Range = 100;
    [Export] public TargetController TargetControllerReference;

    /// <summary> If the entity is can hit by a ranged attack. </summary>
    public bool CanHit
    {
        get
        {
            if (GetCollider() is Entity entity && entity.Team == TargetControllerReference.TargetTeam) return true;
            return false;
        }
    }

    public override void _Process(double delta)
    {
        if (TargetControllerReference == null || TargetControllerReference.CurrentTarget == null) return;

        float rotation = TargetControllerReference.CurrentTarget.GlobalPosition.AngleToPoint(GlobalPosition) + Mathf.Pi;
        TargetPosition = Vector2.Right.Rotated(rotation) * Range;

        var tc = TargetControllerReference;
        var team = tc.TargetTeam;
        var layers = TargetController.CollisionTeamLayers;

        // If collision mask set not correctly, set it.
        if (!GetCollisionMaskValue(tc.GetCollisionLayerByTeam(team)))
        {
            foreach (var item in layers) SetCollisionMaskValue(item.Value, item.Key == team);
        }
    }
}
