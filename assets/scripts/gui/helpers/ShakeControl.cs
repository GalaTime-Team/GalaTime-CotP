using Godot;
using System;

namespace Galatime.UI;

public partial class ShakeControl : Control
{
    /// <summary> The amount of shake intensity. </summary>
    [Export] public float Amount = 10f;

    /// <summary> The duration of the shake effect in seconds. </summary>
    [Export] public float Duration = 0.5f;

    private Vector2 OriginalPosition;
    private Random Random;

    public override void _Ready()
    {
        // Get the original position of the control
        OriginalPosition = Position;

        Random = new Random();
    }

    /// <summary> Start the one-time shake effect. Configured by Amount and Duration. </summary>
    public void Shake()
    {
        var tween = GetTree().CreateTween();

        // Reset the position of the control
        Position = OriginalPosition;

        tween.TweenMethod(Callable.From<float>(tweenProcess), Amount, 0f, Duration);
        void tweenProcess(float x)
        {
            var shakeAmount = x;
            float offsetX = (float)Random.NextDouble() * shakeAmount * 2 - shakeAmount;
            float offsetY = (float)Random.NextDouble() * shakeAmount * 2 - shakeAmount;
            Vector2 offset = new(offsetX, offsetY);
            Position = OriginalPosition + offset;
        }
    }
}