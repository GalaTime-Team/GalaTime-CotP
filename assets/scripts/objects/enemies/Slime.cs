using Godot;
using System;
using Galatime;

public class Slime : Entity
{
    private Node2D _player;
    private Sprite _sprite;
    public new float speed = 100;

    public override void _Ready()
    {
        element = GalatimeElement.Aqua;
        body = GetNode<KinematicBody2D>("Body");
        damageEffectPoint = GetNode<Position2D>("Body/DamageEffectPoint");

        _player = GetNode<Node2D>("/root/Node2D/player/player_body"); 
        _sprite = GetNode<Sprite>("Body/Sprite");
    }

    public override void _moveProcess()
    {
        findPath();
    }

    public void findPath() {
        Vector2 vectorPath = Vector2.Zero;
        if (_player != null)
        {
            float rotation = body.GlobalTransform.origin.AngleToPoint(_player.GlobalTransform.origin);
            vectorPath = Vector2.Left.Rotated(rotation) * speed;
            float rotationDeg = Mathf.Rad2Deg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            if (rotationDegPositive >= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
        } 
        velocity = vectorPath;
    }
}
