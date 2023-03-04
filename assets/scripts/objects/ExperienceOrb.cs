using Godot;
using System;

namespace Galatime
{
    public partial class ExperienceOrb : CharacterBody2D
    {
        [Export] public int quantity = 10;

        public Area2D pickupArea;

        private Player _playerNode;
        private AnimationPlayer _animationPlayer;
        private AnimationPlayer _HUEanimationPlayer;
        private Sprite2D _sprite;

        private string[] texturesPath = new string[4] { "res://sprites/test/xp_orb_stage_0.png", "res://sprites/test/xp_orb_stage_1.png", "res://sprites/test/xp_orb_stage_2.png", "res://sprites/test/xp_orb_stage_3.png" };

        public async override void _Ready()
        {
            pickupArea = GetNode<Area2D>("PickupArea");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _HUEanimationPlayer = GetNode<AnimationPlayer>("HUEAnimationPlayer");
            _sprite = GetNode<Sprite2D>("Sprite2D");

            _HUEanimationPlayer.Play("loop");

            _playerNode = GetTree().GetNodesInGroup("player")[0] as Player;

            pickupArea.BodyEntered += (Node2D node) => _onEntered(node);

            string textureToLoad = texturesPath[0];
            if (quantity >= 10 && quantity < 20)
            {
                textureToLoad = texturesPath[1];
            }
            if (quantity >= 20 && quantity < 40)
            {
                textureToLoad = texturesPath[2];
            }
            if (quantity >= 40)
            {
                textureToLoad = texturesPath[3];
            }
            _sprite.Texture = GD.Load<Texture2D>(textureToLoad);
        }

        public void _onEntered(Node2D node)
        {
            if (node is Player p)
            {
                p.Xp += quantity;
                _animationPlayer.Play("outro");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 vector = Vector2.Right;
            float rotation = GlobalPosition.AngleToPoint(_playerNode.body.GlobalPosition);
            float distance = GlobalPosition.DistanceTo(_playerNode.body.GlobalPosition);
            vector = vector.Rotated(rotation) * Mathf.Clamp(200 - distance, 20, 200);
            if (distance >= 5)
            {
                Velocity = vector;
                MoveAndSlide();
            }
        }
    }
}