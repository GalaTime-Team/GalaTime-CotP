using System.Collections.Generic;
using Galatime.Global;
using Godot;

namespace Galatime.Helpers;

/// <summary> Used to track if the entity is can hit by a ranged attack. </summary>
public partial class RangedHitTracker : Node2D
{
    /// <summary> The range of check. </summary>
    [Export] public float Range = 100;
    /// <summary> The number of rays that shoot. </summary>
    [Export] public int Rays = 1;
    /// <summary> The angle offset of the raycasts in radians. </summary>
    [Export] public float RaysAngleOffset = 0;
    [Export] public int RequiredToCanHit = 1;
    /// <summary> The time to change the can hit state. </summary>
    [Export] public float TimeToChange = 0.05f;
    [Export] public TargetController TargetControllerReference;

    public List<RayCast2D> RayCasts = new();
    public double TimerToChange;

    /// <summary> If the entity is can hit by a ranged attack. </summary>
    public bool CanHit;

    public float TargetAngleRotation;

    /// <summary> Shoots the raycasts if needed. </summary>
    public void ShootRaycasts()
    {
        while (RayCasts.Count > Rays) RayCasts[RayCasts.Count - 1].QueueFree();
        while (RayCasts.Count < Rays)
        {
            var ray = new RayCast2D() { Enabled = false };
            RayCasts.Add(ray);
            AddChild(ray);
        }
    }


    public override void _Process(double delta)
    {
        if (TargetControllerReference == null || TargetControllerReference.CurrentTarget == null) return;

        TimerToChange += delta;
        if (TimerToChange < TimeToChange) return;
        TimerToChange = 0;

        ShootRaycasts();

        TargetAngleRotation = TargetControllerReference.CurrentTarget.GlobalPosition.AngleToPoint(GlobalPosition) + Mathf.Pi;

        var tc = TargetControllerReference;
        var team = tc.TargetTeam;
        var layers = TargetController.CollisionTeamLayers;

        for (int i = 0; i < RayCasts.Count; i++)
        {
            RayCast2D ray = RayCasts[i];

            // If collision mask set not correctly, set it.
            if (!ray.GetCollisionMaskValue(tc.GetCollisionLayerByTeam(team)))
                foreach (var item in layers) ray.SetCollisionMaskValue(item.Value, item.Key == team);

            // Solid object is collidable, so able to determine if raycast hits an entity even if one of the raycast is colliding with a solid object.
            ray.SetCollisionMaskValue(1, true);

            ray.TargetPosition = new Vector2(Range, 0);
            ray.Rotation = (i * RaysAngleOffset * 2) - (Rays * RaysAngleOffset) + RaysAngleOffset + TargetAngleRotation;

            ray.Enabled = true;
        }

        CheckCanHit();
    }

    private void CheckCanHit()
    {
        int successfulRaycasts = 0; // Initialize the count of successful raycasts
        foreach (var ray in RayCasts) 
        {
            if (ray.GetCollider() is Entity entity && entity.Team == TargetControllerReference.TargetTeam)
            {
                successfulRaycasts++;
                // Return true if the required number of successful raycasts is reached
                if (successfulRaycasts == RequiredToCanHit)
                {
                    CanHit = true;
                    return;
                }
            }
        }
        CanHit = false;
    }
}
