using Godot;
using System;

namespace Galatime
{
    public class ExperienceOrb : KinematicBody2D
    {
        [Export] public int quantity = 10;

        public Area2D pickupArea;

        private Player _playerNode;
        private AnimationPlayer _animationPlayer;
        private AnimationPlayer _HUEanimationPlayer;
        private Sprite _sprite;

        private string[] texturesPath = new string[4] { "res://sprites/test/xp_orb_stage_0.png", "res://sprites/test/xp_orb_stage_1.png", "res://sprites/test/xp_orb_stage_2.png", "res://sprites/test/xp_orb_stage_3.png" };

        public async override void _Ready()
        {
            pickupArea = GetNode<Area2D>("PickupArea");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _HUEanimationPlayer = GetNode<AnimationPlayer>("HUEAnimationPlayer");
            _sprite = GetNode<Sprite>("Sprite");

            _HUEanimationPlayer.Play("loop");

            _playerNode = GetTree().GetNodesInGroup("player")[0] as Player;

            pickupArea.Connect("body_entered", this, "_onEntered");

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
            _sprite.Texture = GD.Load<Texture>(textureToLoad);
        }

        public void _onEntered(Node node)
        {
            if (node == _playerNode.body)
            {
                _playerNode.Xp += quantity;
                _animationPlayer.Play("outro");
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            Vector2 vector = Vector2.Left;
            float rotation = GlobalTransform.origin.AngleToPoint(_playerNode.body.GlobalTransform.origin);
            float distance = GlobalTransform.origin.DistanceTo(_playerNode.body.GlobalTransform.origin);
            vector = vector.Rotated(rotation) * Mathf.Clamp(200 - distance, 20, 200);
            if (distance >= 5) MoveAndSlide(vector);
        }
    }
}