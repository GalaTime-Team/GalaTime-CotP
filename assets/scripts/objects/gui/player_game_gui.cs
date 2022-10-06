using Godot;
using System;
using Galatime;

namespace Galatime {
    public class player_game_gui : Control
    {
        // Nodes
        private AnimationPlayer _fadeAnimation;

        private Node _player;

        private TextureProgress _health;

        public override void _Ready()
        {
            _fadeAnimation = GetNode<AnimationPlayer>("fadeAnimation");

            _player = GetNode("/root/Node2D/player");

            _health = GetNode<TextureProgress>("status/hp");

            _player.Connect("fade", this, "onFade");
            _player.Connect("healthChanged", this, "onHealthChanged");
        }

        public void onFade(string type)
        {
            if (type == "in" || type == "out")
            {
                GD.Print("Fade " + type);
                if (_fadeAnimation != null)
                {
                    _fadeAnimation.Play(type);
                } else
                {
                    GD.PrintErr("null");
                }
            }
            else
            {
                GD.PrintErr("Use \"in\" or \"out\"");
            }
        }

        public void onHealthChanged(float health) {
            _health.Value = health;
        }
    }
}


