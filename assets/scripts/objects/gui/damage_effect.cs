using Godot;
using System;

public static class damageEffectColors {
    public static Color equal = new Color(1, 1, 1);

    public static Color plus = new Color(1, 0.9f, 0);
    public static Color minus = new Color(0.6f, 0.6f, 0.6f);
    public static Color heal = new Color(0, 1, 0);
}

public class damage_effect : Node2D
{
    // Variables
    [Export] float number;
    [Export] string type;

    // Nodes
    AnimationPlayer _animation;
    Position2D _position;
    Label _text;

    Particles2D _healParticles;

    public void _onDamageFinished() {
        GetParent().RemoveChild(this);
    }

    public override void _Ready()
    {
        // Get Nodes
        _animation = GetNode<AnimationPlayer>("animation");
        _position = GetNode<Position2D>("position");
        _text = GetNode<Label>("position/damage");
        _healParticles = GetNode<Particles2D>("HealParticles");

        Random rnd = new Random();
        Vector2 randomPosition = Vector2.Zero;
        randomPosition.x = rnd.Next(-20, 20);
        randomPosition.y = rnd.Next(-20, 20);
        _position.Position = randomPosition;
        
        Color color = damageEffectColors.equal;
        string symbol = "=";
        switch (type)
        {
            case "equal":
                color = damageEffectColors.equal;
                symbol = "=";
                break;
            case "plus":
                color = damageEffectColors.plus;
                symbol = "+";
                break;
            case "heal":
                color = damageEffectColors.heal;
                symbol = "+";
                break;
            case "minus":
                color = damageEffectColors.minus;
                symbol = "-";
                break;
            default:
                break;
        }
        _text.Set("custom_colors/font_color", color);

        if (type != "heal")
        {
            var scale = _text.RectScale;
            var scaleAmount = Mathf.Clamp((float)number / 4, 0.75f, 1.25f);
            var animationSpeed = 4 - Mathf.Clamp((float)number / 2, 0f, 3f);
            scale.x = scaleAmount;
            scale.y = scaleAmount;
            _text.RectScale = scale;
            _animation.PlaybackSpeed = animationSpeed;
        }

        _text.Text = symbol + Convert.ToString(number);

        _animation.Play("damage");
        if (type == "heal")
        {
            _healParticles.Amount = (int)Math.Min(number, 200) / 5;
            _healParticles.Emitting = true;     
        }

    }
}
