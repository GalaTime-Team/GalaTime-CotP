using Godot;
using Godot.Collections;
using System;

namespace Galatime;

public partial class DamageEffect : Node2D
{
    #region Exports
    [Export] public float Number;
    [Export] public DamageDifferenceType Type;
    #endregion

    #region Nodes
    public AnimationPlayer AnimationPlayer;
    public Label DamageLabel;
    public GpuParticles2D HealParticles;
    #endregion

    /// <summary> Represents the color of the damage effect by type. </summary>
    public Dictionary<DamageDifferenceType, Color> DamageEffectColors = new() {
        { DamageDifferenceType.equal, new(1, 1, 1) },
        { DamageDifferenceType.plus, new(1, 0.9f, 0) },
        { DamageDifferenceType.minus, new(0.78f, 0.78f, 0.78f) },
        { DamageDifferenceType.heal, new(0, 1, 0) }
    };

    /// <summary> Represents the predified symbol of the damage effect by type. </summary>
    public Dictionary<DamageDifferenceType, string> DamageEffectSymbols = new() {
        { DamageDifferenceType.equal, "=" },
        { DamageDifferenceType.plus, "+" },
        { DamageDifferenceType.minus, "-" },
        { DamageDifferenceType.heal, "+" }
    };

    private void OnDamageFinished() {
        // Make sure the effect is removed from the scene when it is finished.
        GetParent().RemoveChild(this);
    }

    public override void _Ready()
    {
        #region Get nodes
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        DamageLabel = GetNode<Label>("DamageLabel");
        HealParticles = GetNode<GpuParticles2D>("HealParticles");
        #endregion

        // Place the damage label in a random position.
        Random rnd = new();
        Vector2 randomPosition = Vector2.Zero;
        randomPosition.X = rnd.Next(-20, 20);
        randomPosition.Y = rnd.Next(-20, 20);
        DamageLabel.Position += randomPosition;
        
        // Getting the color and symbol of the damage effect.
        Color color = DamageEffectColors[Type];
        string symbol = DamageEffectSymbols[Type];
        // Setting the color of the damage label
        DamageLabel.Set("theme_override_colors/font_color", color);

        if (Type != DamageDifferenceType.heal)
        {
            // Setting the scale and speed of the damage label.
            var scale = DamageLabel.Scale;
            var scaleAmount = Mathf.Clamp(Number / 4, 0.75f, 1.25f);
            var animationSpeed = 4 - Mathf.Clamp(Number / 2, 0f, 3f);
            scale.X = scaleAmount;
            scale.Y = scaleAmount;
            AnimationPlayer.SpeedScale = animationSpeed;
        }

        // Setting the text.
        DamageLabel.Text = $"{symbol}{Number}";

        // Play the animation.
        AnimationPlayer.Play("damage");

        // If healing, set the amount of healing particles.
        if (Type == DamageDifferenceType.heal)
        {
            HealParticles.Amount = (int)Math.Min(Number, 200) / 5;
            HealParticles.Emitting = true;     
        }

    }
}
