using Godot;
using System;

namespace Galatime 
{
    public partial class BlueFireVisuals : CharacterBody2D
    {
        private AnimationPlayer _animationPlayer;

        public int RotateSpeed = 2;
        public int Radius = 120;
        private Vector2 _centre = new Vector2();
        private double _angle = 0;

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            _animationPlayer.Play("intro");

            _animationPlayer.AnimationFinished += (StringName name) => animationFinished(name);
        }

        public void animationFinished(StringName name)
        {
            if (name == "intro")
            {
                _animationPlayer.Play("loop");
            }
        }

        public void destroy()
        {
            _animationPlayer.Play("outro");
        }

        public override void _PhysicsProcess(double delta)
        {
            _angle += RotateSpeed * delta;
            var offset = new Vector2((float)Math.Sin(_angle), (float)Math.Cos(_angle)) * Radius;
            var pos = _centre + offset;

            Velocity = pos;
            MoveAndSlide();
        }
    }
}
