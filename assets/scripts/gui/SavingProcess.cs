using Godot;

namespace Galatime;

/// <summary> Represents the UI that shows when the game is saving. </summary>
public partial class SavingProcess : CanvasLayer
{
    // TODO: Rework this class entirely

    AnimationPlayer AnimationPlayer;
    AnimatedSprite2D AnimatedSprite;

    public override void _Ready()
    {
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimatedSprite = GetNode<AnimatedSprite2D>("AnimationContainer/AnimationControl/Animation");
        AnimationPlayer.Play("default");
        AnimatedSprite.Play("default");
    }

    public void PlayFailedAnimation()
    {
        AnimationPlayer.Play("default");
        AnimatedSprite.Play("failed");
    }
}

