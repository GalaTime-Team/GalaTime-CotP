using Godot;
using System;
using Galatime;

public class Weapon : Area2D
{
    private AnimationPlayer _animation;
    private float countdown = 0.3f;
    private bool canAttack = true;
    private Node2D _player;
    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("WeaponAnimationPlayer"); 
        _player = GetParent<Node2D>();

        // Connect event
        Connect("body_entered", this, "_on_body_entered");
    }

    public void attack() {
        if (canAttack)
        {
            // Play animation
            _animation.Stop();
            _animation.Play("swing");

            // Delay for disable collision and countdown
            SceneTree tree = GetTree();
            tree.CreateTimer(countdown).Connect("timeout", this, "_resetCountdown");

            // Reset collision
            tree.CreateTimer(0.05f).Connect("timeout", this, "_resetCollision");

            canAttack = false;

            // Enable collision
            SetCollisionMaskBit(2, true);
        }
    } 

    public void _resetCollision() {
        SetCollisionMaskBit(2, false);
    }

    public void _resetCountdown() {
        canAttack = true;
        SetCollisionMaskBit(2, false);
    }

    public void _on_body_entered(KinematicBody2D body) {
        GD.Print("body_entered");
        // Get scripted node
        Node2D parent = body.GetParent<Node2D>();
        // !!! NEEDS REWORK !!!
        GalatimeElement element = GalatimeElement.Ignis;

        // Get angle of damage
        float damageRotation = GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
        if (parent.HasMethod("hit"))
        {
            parent.Call("hit", 10, element, 500, damageRotation);
        }
    }
}
