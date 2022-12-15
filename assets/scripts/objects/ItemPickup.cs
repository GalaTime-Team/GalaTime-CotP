using Godot;
using System;

public class ItemPickup : KinematicBody2D
{
    public Vector2 velocity = Vector2.Zero;

    [Export] public Vector2 spawnVelocity;
    [Export] public string itemId;

    public Godot.Collections.Dictionary item;

    public AnimationPlayer animationPlayer;
    public Sprite sprite;
    public Particles2D particles;
    public Area2D pickupArea;

    private KinematicBody2D _playerBody;
    private Node2D _playerNode;

    public async override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<Sprite>("Sprite");
        particles = GetNode<Particles2D>("Particles");
        pickupArea = GetNode<Area2D>("PickupArea");

        _playerBody = GetNode<KinematicBody2D>(Galatime.GalatimeConstants.playerBodyPath);
        _playerNode = GetNode<Node2D>(Galatime.GalatimeConstants.playerPath);

        pickupArea.Connect("body_entered", this, "_onEntered");

        DisplayItem(itemId);
        velocity = spawnVelocity;
        animationPlayer.Play("idle");

        Random rnd = new Random();
        RotationDegrees = rnd.Next(0, 360);
    }

    public void _onEntered(Node node)
    {
        if (node == _playerBody)
        {
            GetNode<PlayerVariables>("/root/PlayerVariables").addItem(item);
            QueueFree();
        }
    }

    public void DisplayItem(string id)
    {
        item = GalatimeGlobals.getItemById(id);
        Godot.Collections.Dictionary ItemAssets = (Godot.Collections.Dictionary)item["assets"];
        string icon = (string)ItemAssets["icon"];
        if (item != null)
        {
            sprite.Texture = GD.Load<Texture>("res://sprites/" + icon);
        }
        else
        {
            sprite.Texture = null;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity = velocity.LinearInterpolate(Vector2.Zero, 0.02f);
        particles.Emitting = velocity.Length() <= 10 ? false : true;
        RotationDegrees += velocity.Length() / 10;
        MoveAndSlide(velocity);
    }
}
