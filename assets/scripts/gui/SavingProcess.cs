using Godot;

namespace Galatime
{
    public partial class SavingProcess : CanvasLayer
    {
        AnimationPlayer animationPlayer;
        AnimatedSprite2D animatedSprite;

        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animatedSprite = GetNode<AnimatedSprite2D>("AnimationContainer/Animation");
            animationPlayer.Play("default");
            animatedSprite.Play("default");
        }

        public void PlayFailedAnimation()
        {
            animationPlayer.Play("default");
            animatedSprite.Play("failed");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}

