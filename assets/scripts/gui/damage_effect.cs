using Godot;
using System;

public struct damageEffectColors {
    public static Color equal = new Color(1, 1, 1);

    public static Color plus = new Color(1, 0.9f, 0);
    public static Color minus = new Color(1, 1, 1);
    public static Color heal = new Color(0, 1, 0);
}

public partial class damage_effect : Node2D
{
    // Variables
    [Export] float number;
    [Export] string type;

    // Nodes
    AnimationPlayer _animation;
    Marker2D _position;
    Label _text;

    GpuParticles2D _healParticles;

    public void _onDamageFinished() {
        GetParent().RemoveChild(this);
    }

    public override void _Ready()
    {
        // Get Nodes
        _animation = GetNode<AnimationPlayer>("animation");
        _position = GetNode<Marker2D>("position");
        _text = GetNode<Label>("position/damage");
        _healParticles = GetNode<GpuParticles2D>("HealParticles");

        Random rnd = new Random();
        Vector2 randomPosition = Vector2.Zero;
        randomPosition.X = rnd.Next(-20, 20);
        randomPosition.Y = rnd.Next(-20, 20);
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
            var scale = _text.Scale;
            var scaleAmount = Mathf.Clamp((float)number / 4, 0.75f, 1.25f);
            var animationSpeed = 4 - Mathf.Clamp((float)number / 2, 0f, 3f);
            scale.X = scaleAmount;
            scale.Y = scaleAmount;
            //  _text.Scale = scale;
            _animation.SpeedScale = animationSpeed;
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
