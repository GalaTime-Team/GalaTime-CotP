using Godot;
using System;

namespace Galatime
{
    public class ItemPickup : KinematicBody2D
    {
        public Vector2 velocity = Vector2.Zero;

        [Export] public Vector2 spawnVelocity;
        [Export] public string itemId;
        [Export] public int quantity = 1;

        public Godot.Collections.Dictionary item;

        public AnimationPlayer animationPlayer;
        public Sprite sprite;
        public Sprite sprite2;
        public Sprite sprite3;
        public Particles2D particles;
        public Area2D pickupArea;

        private KinematicBody2D _playerBody;
        private Node2D _playerNode;

        public async override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            sprite = GetNode<Sprite>("Sprite");
            sprite2 = GetNode<Sprite>("Sprite2");
            sprite3 = GetNode<Sprite>("Sprite3");
            particles = GetNode<Particles2D>("Particles");
            pickupArea = GetNode<Area2D>("PickupArea");

            _playerBody = GetNode<KinematicBody2D>(GalatimeConstants.playerBodyPath);
            _playerNode = GetNode<Node2D>(GalatimeConstants.playerPath);

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
                GetNode<PlayerVariables>("/root/PlayerVariables").addItem(item, quantity);
                QueueFree();
            }
        }

        public void DisplayItem(string id)
        {
            item = GalatimeGlobals.getItemById(id);
            Godot.Collections.Dictionary ItemAssets = (Godot.Collections.Dictionary)item["assets"];
            string icon = (string)ItemAssets["icon"];
            GD.Print("res://sprites/" + icon);
            if (item != null)
            {
                var itemTexture = GD.Load<Texture>("res://sprites/" + icon);
                sprite.Texture = itemTexture;
                GD.Print("item quantity pickup" + quantity);
                if (quantity >= 2)
                {
                    var spriteNewPosition = new Vector2();
                    spriteNewPosition.y = -6;
                    sprite.Position = spriteNewPosition;
                    var sprite2NewPosition = new Vector2();
                    sprite2NewPosition.y = 6;
                    sprite2.Position = sprite2NewPosition;
                    sprite2.Texture = itemTexture;
                    sprite2.Visible = true;
                }
                if (quantity >= 5)
                {
                    var sprite2NewPosition = new Vector2();
                    sprite2NewPosition.y = 6;
                    sprite2NewPosition.x = -6;
                    sprite2.Position = sprite2NewPosition;
                    var sprite3NewPosition = new Vector2();
                    sprite3NewPosition.y = 6;
                    sprite3NewPosition.x = 6;
                    sprite3.Position = sprite3NewPosition;
                    sprite3.Texture = itemTexture;
                    sprite3.Visible = true;
                }
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
}