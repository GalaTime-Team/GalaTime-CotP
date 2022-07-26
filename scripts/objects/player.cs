using Godot;
using System;

public class player : Node2D {
    [Export] public float Speed = 200f;
    [Export] public string IdleAnimation = "idle_down";
    private Vector2 velocity = Vector2.Zero;
    private string[] _movementAnimations = new string[] { "walk_up", "walk_down", "walk_right", "walk_left" };
    private bool canMove = true;

    // Nodes
    private AnimationPlayer _animationPlayer;
    private AnimationPlayer _animationPlayerWeapon;

    private KinematicBody2D _body;
    private Sprite _weapon;

    private Camera2D _camera;

    // Signals
    [Signal] delegate void fade(string type);
    [Signal] delegate void wrap();

    public override void _Ready()
    {
        // Get Nodes
        _animationPlayer = GetNode<AnimationPlayer>("player_body/animation");
        _animationPlayerWeapon = GetNode<AnimationPlayer>("player_body/animation_weapon");

        _body = GetNode<KinematicBody2D>("player_body");
        _weapon = GetNode<Sprite>("player_body/weapon");

        _camera = GetNode<Camera2D>("player_body/camera");

        // Start
        foreach (string animationsString in _movementAnimations)
        {
            Animation animation = _animationPlayer.GetAnimation(animationsString);
            animation.Loop = true;
            _animationPlayer.PlaybackSpeed = Speed / 100;
        }

        // Animation weaponAnimation = _animationPlayerWeapon.GetAnimation("charged");
        // weaponAnimation.Loop = true;
        // _animationPlayerWeapon.Play("charged");

        // _animationPlayer.Play("fade_out");
    }

    private void _SetAnimation(Vector2 animationVelocity)
    {
        if (animationVelocity.Length() == 0)
        {
            _animationPlayer.Play(IdleAnimation);
        }
        if (animationVelocity.y != 0)
        {
            if (animationVelocity.y <= -1 && _animationPlayer.CurrentAnimation != "walk_up")
            {
                IdleAnimation = "idle_up";
                _animationPlayer.Play("walk_up");
            }
            if (animationVelocity.y >= 1 && _animationPlayer.CurrentAnimation != "walk_down")
            {
                IdleAnimation = "idle_down";
                _animationPlayer.Play("walk_down");
            }
        }
        else
        {
            if (animationVelocity.x >= 1 && _animationPlayer.CurrentAnimation != "walk_right")
            {
                IdleAnimation = "idle_right";
                _animationPlayer.Play("walk_right");
            }
            if (animationVelocity.x <= -1 && _animationPlayer.CurrentAnimation != "walk_left")
            {
                IdleAnimation = "idle_left";
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
        inputVelocity = inputVelocity.Normalized() * Speed;
        velocity = inputVelocity;

        _weapon.LookAt(GetGlobalMousePosition());
        _setCameraPosition();
    }

    private void _setCameraPosition()
    {
        _camera.GlobalPosition = _camera.GlobalPosition.LinearInterpolate((_weapon.GlobalPosition + GetLocalMousePosition() / 10), 0.05f);
        GD.Print(GetLocalMousePosition());    
    }

    public void _onWrap()
    {
        GD.Print("work");
        canMove = false;
        EmitSignal("fade", "in");
    }

    public override void _PhysicsProcess(float delta)
    {
        _SetMove();
        if (canMove) _body.MoveAndSlide(velocity);
        base._PhysicsProcess(delta);
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
