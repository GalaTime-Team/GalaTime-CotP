using Godot;
using System;

namespace Galatime
{
    public partial class ItemPickup : CharacterBody2D
    {
        public Vector2 velocity = Vector2.Zero;

        [Export] public Vector2 spawnVelocity;
        [Export] public string itemId;
        [Export] public int quantity = 1;

        public Godot.Collections.Dictionary item;

        public AnimationPlayer animationPlayer;
        public Sprite2D sprite;
        public Sprite2D sprite2;
        public Sprite2D sprite3;
        public GpuParticles2D particles;
        public Area2D pickupArea;

        private Player _playerNode;

        public async override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            sprite = GetNode<Sprite2D>("Sprite2D");
            sprite2 = GetNode<Sprite2D>("Sprite2");
            sprite3 = GetNode<Sprite2D>("Sprite3");
            particles = GetNode<GpuParticles2D>("Particles");
            pickupArea = GetNode<Area2D>("PickupArea");

            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            _playerNode = playerVariables.Player;

            pickupArea.BodyEntered += (Node2D node) => _onEntered(node);

            DisplayItem(itemId);
            velocity = spawnVelocity;   
            // animationPlayer.Play("idle");

            // Random rnd = new Random();
            // RotationDegrees = rnd.Next(0, 360);
        }

        public void _onEntered(Node2D node)
        {
            GD.Print(node);
            if (node is Player)
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
                var itemTexture = GD.Load<Texture2D>("res://sprites/" + icon);
                sprite.Texture = itemTexture;
                GD.Print("item quantity pickup" + quantity);
                if (quantity >= 2)
                {
                    var spriteNewPosition = new Vector2();
                    spriteNewPosition.Y = -6;
                    sprite.Position = spriteNewPosition;
                    var sprite2NewPosition = new Vector2();
                    sprite2NewPosition.Y = 6;
                    sprite2.Position = sprite2NewPosition;
                    sprite2.Texture = itemTexture;
                    sprite2.Visible = true;
                }
                if (quantity >= 5)
                {
                    var sprite2NewPosition = new Vector2();
                    sprite2NewPosition.Y = 6;
                    sprite2NewPosition.X = -6;
                    sprite2.Position = sprite2NewPosition;
                    var sprite3NewPosition = new Vector2();
                    sprite3NewPosition.Y = 6;
                    sprite3NewPosition.X = 6;
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

        public override void _PhysicsProcess(double delta)
        {
            velocity = velocity.Lerp(Vector2.Zero, 0.02f);
            particles.Emitting = velocity.Length() <= 10 ? false : true;
            //RotationDegrees += velocity.Length() / 10;
            Velocity = velocity;
            MoveAndSlide();
        }
    }
}