using Galatime;
using Godot;
using System;

public class ExperienceOrb : KinematicBody2D
{
    [Export] public int quantity = 10;

    public Area2D pickupArea;

    private Player _playerNode;
    private AnimationPlayer _animationPlayer;

    public async override void _Ready()
    {
        pickupArea = GetNode<Area2D>("PickupArea");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        _playerNode = GetTree().GetNodesInGroup("player")[0] as Player;

        pickupArea.Connect("body_entered", this, "_onEntered");
    }

    public void _onEntered(Node node)
    {
        if (node == _playerNode.body)
        {
            _playerNode.xp += quantity;
            _animationPlayer.Play("outro");
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 vector = Vector2.Left;
        float rotation = GlobalTransform.origin.AngleToPoint(_playerNode.body.GlobalTransform.origin);
        float distance = GlobalTransform.origin.DistanceTo(_playerNode.body.GlobalTransform.origin);
        vector = vector.Rotated(rotation) * Mathf.Clamp(200 - distance, 20, 200);
        if (distance >= 5) MoveAndSlide(vector);
    }
}
