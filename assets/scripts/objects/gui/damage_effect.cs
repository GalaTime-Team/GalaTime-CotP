using Godot;
using System;

public static class damageEffectColors {
    public static Color equal = new Color(1, 1, 1);
    
    public static Color plus = new Color(1, 0.66f, 0.2f);
    public static Color minus = new Color(0, 1, 0);
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

    public void _onDamageFinished() {
        GetParent().RemoveChild(this);
    }

    public override void _Ready()
    {
        // Get Nodes
        _animation = GetNode<AnimationPlayer>("animation");
        _position = GetNode<Position2D>("position");
        _text = GetNode<Label>("position/damage");

        Random rnd = new Random();
        Vector2 randomPosition = Vector2.Zero;
        randomPosition.x = rnd.Next(-10, 10);
        randomPosition.y = rnd.Next(-10, 10);
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
            case "minus":
                color = damageEffectColors.minus;
                symbol = "-";
                break;
            default:
                break;
        }
        _text.Set("custom_colors/font_color", color);

        _text.Text = symbol + Convert.ToString(number);

        _animation.Play("damage");
    }
}
