using Galatime;
using Godot;
using System;

public class GoldenHolderSword : Area2D
{
    private AnimationPlayer _animation;
    private float countdown = 0.3f;
    private bool canAttack = true;

    private float _physicalAttack;

    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("WeaponAnimationPlayer");

        // Connect event
        Connect("body_entered", this, "_on_body_entered");
    }

    public void attack(float physicalAttack, float magicalAttack)
    {
        _physicalAttack = physicalAttack;
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

    public void _resetCollision()
    {
        SetCollisionMaskBit(2, false);
    }

    public void _resetCountdown()
    {
        canAttack = true;
        SetCollisionMaskBit(2, false);
    }

    public void _on_body_entered(KinematicBody2D body)
    {
        GD.Print("body_entered");
        // Get scripted node
        Entity parent = body.GetParent<Entity>();
        // !!! NEEDS REWORK !!!
        GalatimeElement element = new GalatimeElement();

        // Get angle of damage
        float damageRotation = GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
        if (parent.HasMethod("hit"))
        {
            parent.hit(10, _physicalAttack, element, DamageType.physical, 500, damageRotation);
        }
    }
}
