using Galatime;
using Godot;
using System;
public class GoldenHolderSword : Area2D
{
    private AnimationPlayer _animation;
    private AudioStreamPlayer2D _audio;
    private float countdown = 0.5f;
    private bool canAttack = true;
    private bool _swungBit = true;

    private float _physicalAttack;

    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("WeaponAnimationPlayer");
        _audio = GetNode<AudioStreamPlayer2D>("SwingSoundPlayer");

        // Connect event
        Connect("body_entered", this, "_on_body_entered");
    }

    public async void attack(float physicalAttack, float magicalAttack)
    {
        _physicalAttack = physicalAttack;
        if (canAttack)
        {
            var rand = new Random();
            _audio.PitchScale = (float)(rand.NextDouble() * (1.1 - 0.9) + 0.9);

            // Play animation
            _animation.Stop();
            _animation.Play(_swungBit ? "swing" : "swing_inverted");
            _swungBit = !_swungBit;
                
            // Delay for disable collision and countdown
            SceneTree tree = GetTree();
            tree.CreateTimer(countdown).Connect("timeout", this, "_resetCountdown");

            // Reset collision
            tree.CreateTimer(0.05f).Connect("timeout", this, "_resetCollision");

            canAttack = false;

            // Enable collision
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
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
        GD.Print("body entered");
        if (body is Entity entity)
        {
            GalatimeElement element = new GalatimeElement();
            float damageRotation = GlobalTransform.origin.AngleToPoint(entity.GlobalTransform.origin);
            entity.hit(10, _physicalAttack, element, DamageType.physical, 500, damageRotation);
        }
    }
}
