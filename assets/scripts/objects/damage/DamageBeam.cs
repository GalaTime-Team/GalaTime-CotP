using Godot;
using System;

namespace Galatime.Damage;

[Tool]
/// <summary> Represents a beam that can deal damage. Have additional features to cut the beam. </summary>
public partial class DamageBeam : Node2D
{
    /// <summary> Reference to the damage area. Use this to deal damage. </summary>
    public DamageArea DamageArea { get; private set; }

    public RayCast2D RayCast { get; private set; }
    public CollisionShape2D DamageAreaCollisionShape { get; private set; }
    [Export] public Line2D Line;

    [Export(PropertyHint.Range, "0,2000")] public float BeamWidth = 40f;
    [Export(PropertyHint.Range, "0,2000")] public float BeamLength = 100f;
    /// <summary> The offset of the beam from the end of the beam. Use this to make collisions more accurate. </summary>
    [Export(PropertyHint.Range, "0,2000")] public float BeamOffset = 5f;

    public override void _Ready()
    {
        // Create the damage area.
        DamageArea = GetNode<DamageArea>("DamageArea");
        DamageAreaCollisionShape = GetNode<CollisionShape2D>("DamageArea/Collision");
        RayCast = GetNode<RayCast2D>("RayCast");

        // Duplicate the shape, so it don't get shared with other objects.
        DamageAreaCollisionShape.Shape = DamageAreaCollisionShape.Shape.Duplicate() as Shape2D;
    }

    public void Shoot()
    {
        DamageArea.HitOneTime();
    }

    public override void _Process(double delta)
    {
        if (RayCast != null && IsInstanceValid(RayCast))
        {
            RayCast.TargetPosition = new Vector2(BeamLength, 0);
            if (RayCast.IsColliding())
            {
                var position = RayCast.GetCollisionPoint(); // Get the position of the collision point in global coordinates.
                var lineLength = RayCast.GlobalPosition.DistanceTo(position);
                Cut(lineLength);
            }
            else Cut(BeamLength);
        }
    }

    public void Cut(float lineLength = 0)
    {
        Line?.SetPointPosition(1, new Vector2(lineLength, 0));

        var shape = DamageAreaCollisionShape.Shape as RectangleShape2D;
        var shapeSize = shape.Size;
        shapeSize.X = lineLength;
        shapeSize.Y = BeamWidth;
        DamageAreaCollisionShape.Position = new(shapeSize.X / 2, 0);
        shape.Size = shapeSize;
        DamageAreaCollisionShape.Shape = shape;
    }
}
