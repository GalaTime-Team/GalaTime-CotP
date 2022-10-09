using Godot;
using System;
using Galatime;

public class Weapon : Area2D
{
    private AnimationPlayer _animation;
    private float countdown = 0.5f;
    private bool canAttack = true;
    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("WeaponAnimationPlayer");

        // Connect event
        Connect("body_entered", this, "_on_body_entered");

        // Disable collision
        Monitoring = false;
    }

    public void attack() {
        if (canAttack)
        {
            // Enable collision
            Monitoring = true;

            // Play animation
            _animation.Stop();
            _animation.Play("swing");

            // Delay for disable collision and countdown
            SceneTree tree = GetTree();
            tree.CreateTimer(0.05f).Connect("timeout", this, "_resetCollision");
            tree.CreateTimer(countdown).Connect("timeout", this, "_resetCountdown");

            canAttack = false;
        }
    } 

    public void _resetCollision() {
        // Disable collision
        Monitoring = false;
    }

    public void _resetCountdown() {
        canAttack = true;
    }

    public void _on_body_entered(KinematicBody2D body) {
        // Get scripted node
        Node2D parent = body.GetParent<Node2D>();
        // !!! NEEDS REWORK !!!
        GalatimeElement element = GalatimeElement.Ignis;

        // Get angle of damage
        float damageRotation = GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
        if (parent.HasMethod("hit"))
        {
            parent.Call("hit", 10, element, 2000, damageRotation);
        }
    }
}
