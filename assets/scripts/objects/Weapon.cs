using Galatime;
using Godot;

public partial class Weapon : Area2D
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
        // BodyEntered += (Node2D body) => _on_body_entered(body);
    }

    public void attack()
    {
        if (canAttack)
        {
            // Play animation
            _animation.Stop();
            _animation.Play("swing");

            // Delay for disable collision and countdown
            SceneTree tree = GetTree();
            tree.CreateTimer(countdown).Connect("timeout", new Callable(this, "_resetCountdown"));

            // Reset collision
            tree.CreateTimer(0.05f).Connect("timeout", new Callable(this, "_resetCollision"));

            canAttack = false;

            // Enable collision
            SetCollisionMaskValue(2, true);
        }
    }

    public void _resetCollision()
    {
        SetCollisionMaskValue(2, false);
    }

    public void _resetCountdown()
    {
        canAttack = true;
        SetCollisionMaskValue(2, false);
    }

    public void _on_body_entered(CharacterBody2D body)
    {
        // Get scripted node
        Node2D parent = body.GetParent<Node2D>();
        // !!! NEEDS REWORK !!!
        GalatimeElement element = GalatimeElement.Ignis;

        // Get angle of damage
        float damageRotation = GlobalPosition.AngleToPoint(body.GlobalPosition);
        if (parent.HasMethod("hit"))
        {
            parent.Call("hit", 10, element, 500, damageRotation);
        }
    }
}
