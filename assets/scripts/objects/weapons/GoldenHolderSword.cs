using Galatime;
using Godot;
using System;
public partial class GoldenHolderSword : Area2D
{
    private AnimationPlayer _animation;
    private AudioStreamPlayer2D _audio;
    private float countdown = 0.5f;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool _swungBit = true;

    private float _physicalAttack;

    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("WeaponAnimationPlayer");
        _audio = GetNode<AudioStreamPlayer2D>("SwingSoundPlayer");

        // Connect event
        // Connect("body_entered",new Callable(this,"_on_body_entered"));
    }

    public async void attack(float physicalAttack, float magicalAttack)
    {
        _physicalAttack = physicalAttack;
        if (canAttack)
        {
            var rand = new Random();
            _audio.PitchScale = (float)(rand.NextDouble() * (1.1 - 0.9) + 0.9);
            _audio.Play();

            // Play animation
            _animation.Stop();
            _animation.Play(_swungBit ? "swing" : "swing_inverted");
            _swungBit = !_swungBit;

            // Delay for disable collision and countdown
            SceneTree tree = GetTree();
            tree.CreateTimer(countdown).Connect("timeout", new Callable(this, "_resetCountdown"));

            // Reset collision
            tree.CreateTimer(0.05f).Connect("timeout", new Callable(this, "_resetCollision"));

            canAttack = false;

            // Enable collision
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            isAttacking = true;
            // SetCollisionMaskValue(2, true);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var bodies = GetOverlappingBodies();
        if (isAttacking)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] is Projectile projectile && projectile.TargetTeam == Galatime.Helpers.Teams.allies && projectile.Moving) Parry(projectile);
                if (bodies[i] is Entity entity)
                {
                    GalatimeElement element = new();
                    float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
                    entity.TakeDamage(10, _physicalAttack, element, DamageType.physical, 500, damageRotation);
                }
            }
            isAttacking = false;
        }
    }

    public void Parry(Projectile projectile) {
        projectile.Rotation = GlobalRotation;
        projectile.Accuracy = 0f;
        projectile.Speed *= 3f;
        projectile.Power = (int)(projectile.Power * 2f);
        projectile.TargetTeam = Galatime.Helpers.Teams.enemies;
        projectile.Explosive = true;
    }

    public void _resetCollision()
    {
        isAttacking = false;
        //SetCollisionMaskValue(2, false);
    }

    public void _resetCountdown()
    {
        canAttack = true;
        isAttacking = false;
        //SetCollisionMaskValue(2, false);
    }
}
