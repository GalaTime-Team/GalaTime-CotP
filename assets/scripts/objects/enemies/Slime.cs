using Godot;
using System;
using Galatime;

public class Slime : Entity
{
    private Node2D _player;
    private Sprite _sprite;
    private Area2D _weapon;
    public new float speed = 100;
    [Export] public float r = 1;

    public override void _Ready()
    {
        element = GalatimeElement.Aqua;
        body = GetNode<KinematicBody2D>("Body");
        damageEffectPoint = GetNode<Position2D>("Body/DamageEffectPoint");

        _player = GetNode<Node2D>("/root/Node2D/Player/player_body"); 
        _sprite = GetNode<Sprite>("Body/Sprite");
        _weapon = GetNode<Area2D>("Body/Weapon");

        health = 100;

        _weapon.Connect("body_entered", this, "_attack");
    }

    public override void _moveProcess()
    {
        findPath();
    }

    public void _attack(KinematicBody2D body) {
        Node2D parent = body.GetParent<Node2D>();
        // !!! NEEDS REWORK !!!
        GalatimeElement element = GalatimeElement.Aqua;

        // Get angle of damage
        float damageRotation = _sprite.GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
        GD.Print(damageRotation);
        if (parent.HasMethod("hit"))
        {
            parent.Call("hit", 4, element, 250, damageRotation);
        }
    }

    public void findPath() {
        Vector2 vectorPath = Vector2.Zero;
        if (Node2D.IsInstanceValid(_player))
        {
            float rotation = body.GlobalTransform.origin.AngleToPoint(_player.GlobalTransform.origin);
            _weapon.Rotation = rotation + r;
            vectorPath = Vector2.Left.Rotated(rotation) * speed;
            float rotationDeg = Mathf.Rad2Deg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            if (rotationDegPositive >= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
        } 
        velocity = vectorPath;
    }
}
