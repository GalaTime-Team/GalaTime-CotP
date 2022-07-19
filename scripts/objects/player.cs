using Godot;
using System;

public class player : KinematicBody2D
{
    [Export] float speed = 200f;

    private Vector2 velocity = Vector2.Zero;
    private string _idleAnimation = "walk_down";
    private string[] _movementAnimations = new string[] { "walk_up", "walk_down", "walk_right", "walk_left" };

    private AnimationPlayer _animationPlayer;
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("animation");
        foreach(string animationsString in _movementAnimations)
        {
            Animation animation = _animationPlayer.GetAnimation(animationsString);
            animation.Loop = true;
            _animationPlayer.PlaybackSpeed = speed / 100;
            GD.Print(_animationPlayer.PlaybackSpeed);
        }
    }

    private void _SetAnimation(Vector2 animationVelocity)
    {
        if (animationVelocity.Length() == 0)
        {
            _animationPlayer.Play(_idleAnimation);
        }
        if (animationVelocity.y != 0)
        {
            if (animationVelocity.y <= -1 && _animationPlayer.CurrentAnimation != "walk_up")
            {
                _idleAnimation = "idle_up";
                _animationPlayer.Play("walk_up");
            }
            if (animationVelocity.y >= 1 && _animationPlayer.CurrentAnimation != "walk_down")
            {
                _idleAnimation = "idle_down";
                _animationPlayer.Play("walk_down");
            }
        }
        else
        {
            if (animationVelocity.x >= 1 && _animationPlayer.CurrentAnimation != "walk_right")
            {
                _idleAnimation = "idle_right";
                _animationPlayer.Play("walk_right");
            }
            if (animationVelocity.x <= -1 && _animationPlayer.CurrentAnimation != "walk_left")
            {
                _idleAnimation = "idle_left";
                _animationPlayer.Play("walk_left");
            }
        }
        
    }

    private void _SetMove()
    {
        Vector2 inputVelocity = Vector2.Zero;
        if (Input.IsActionPressed("game_move_up"))
        {
            inputVelocity.y -= 1;
        }
        if (Input.IsActionPressed("game_move_down"))
        {
            inputVelocity.y += 1;
        }
        if (Input.IsActionPressed("game_move_right"))
        {
            inputVelocity.x += 1;
        }
        if (Input.IsActionPressed("game_move_left"))
        {
            inputVelocity.x -= 1;
        }
        _SetAnimation(inputVelocity);
        inputVelocity = inputVelocity.Normalized() * speed;
        velocity = inputVelocity;
    }

    public override void _PhysicsProcess(float delta)
    {
        _SetMove();
        MoveAndSlide(velocity);
        base._PhysicsProcess(delta);
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
