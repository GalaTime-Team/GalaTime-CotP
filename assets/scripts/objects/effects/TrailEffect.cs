using Godot;


/// <summary> Node, that creates a trail effect based on position in the scene. </summary>
public partial class TrailEffect : Line2D
{
    #region Export
    [Export] public bool Enabled = true;
    /// <summary> The count of points of the trail. </summary>
    [Export] public long Length = 10;
    /// <summary> The gap between the trail points. </summary>
    [Export] public float DistanceGap = 10;
    /// <summary> The parent node that will be followed. </summary>
    [Export] public Node2D Parent;
    #endregion

    #region Variables
    private float CurrentDistance = 0f;
    private Vector2 PreviousPosition;
    #endregion

    public override void _PhysicsProcess(double delta)
    {
        // Set last point position to make it look more smooth.
        if (Points.Length > 0) SetPointPosition(Points.Length - 1, Parent.GlobalPosition);

        if (Enabled)
        {
            // Based on distance we calculate the gap.
            CurrentDistance += Parent.GlobalPosition.DistanceTo(PreviousPosition);
            PreviousPosition = Parent.GlobalPosition;

            if (CurrentDistance > DistanceGap)
            {
                CurrentDistance = 0f;
                // Add new point if gap is reached.
                AddPoint(Parent.GlobalPosition);
            }
        }
        // No need to calculate distance if trail is disabled.
        else CurrentDistance = 0f;

        // Remove points if length is reached or if trail is disabled.
        if (Points.Length > Length || (Points.Length > 0 && !Enabled)) RemovePoint(0);
    }
}