using Godot;
using System;
using Galatime;

public class Weapon : Area2D
{
    private AnimationPlayer _animation;
    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("WeaponAnimationPlayer");
        GD.Print(_animation.ToString());
        Connect("body_entered", this, "_on_body_entered");
        
    }

    public void attack() {
        _animation.Play("swing");
    } 

    public void _on_body_entered(KinematicBody2D body) {
        Node2D parent = body.GetParent<Node2D>();
        GalatimeElement element = GalatimeElement.Ignis;
        float damageRotation = GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
        if (parent.HasMethod("hit"))
        {
            parent.Call("hit", 10, element, 1000, damageRotation);
        }
    }
}
