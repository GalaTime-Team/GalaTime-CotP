using Godot;

namespace Galatime.AI;

/// <summary> Represents an AI navigator, which contains methods for navigation and pathfinding. Wraps the <see cref="NavigationAgent2D"/> class. </summary>
public partial class Navigator : NavigationAgent2D
{
    [Export] public Node2D Body;
    /// <summary> Minimum distance to target before stopping. </summary>

    #region Variables
    /// <summary> Target to navigate to. </summary>
    public Node2D Target;
    /// <summary> Target position to navigate to. </summary>
    public Vector2 NavigatorTargetPosition;
    public Vector2 NavigatorVelocity;
    public float Speed;
    #endregion

    #region Methods
    /// <summary> Finds a path to the target position and returns its velocity. </summary>
    public Vector2 GetVelocity()
    {
        TargetPosition = NavigatorTargetPosition;
        if (IsNavigationFinished()) return Vector2.Zero;
        var vectorPath = Body.GlobalPosition.DirectionTo(GetNextPathPosition()).Normalized() * Speed; // Get path vector from the path rotation.
        return vectorPath;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Update target position if target is set.
        // TODO: Implement target override by position. Meaning if the target is not set, but the position is set, use the position instead.
        if (Target != null) 
        {
            NavigatorTargetPosition = Target.GlobalPosition;
            NavigatorVelocity = GetVelocity();
        }
    }
    #endregion
}