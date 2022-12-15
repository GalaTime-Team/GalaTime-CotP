using Godot;
using System;
using Galatime;

namespace Galatime
{
    public class player_game_gui : Control
    {
        // Nodes
        private AnimationPlayer _fadeAnimation;

        private Node _player;

        private TextureProgress _health;
        private TextureProgress _healthDrain;

        private NinePatchRect _dialogBox;

        private Panel _pauseContainer;

        private float _localHp = 0f;

        [Signal] public delegate void items_changed();
        [Signal] public delegate void on_pause(bool visible);

        public override void _Ready()
        {
            _fadeAnimation = GetNode<AnimationPlayer>("fadeAnimation");

            _player = GetNode("/root/Node2D/player");

            _health = GetNode<TextureProgress>("status/hp");
            _healthDrain = GetNode<TextureProgress>("status/hp_drain");

            _dialogBox = GetNode<NinePatchRect>("DialogBox");

            _pauseContainer = GetNode<Panel>("PauseContainer");

            _player.Connect("fade", this, "onFade");
            _player.Connect("healthChanged", this, "onHealthChanged");
            _player.Connect("on_dialog_start", this, "startDialog");
            _player.Connect("on_pause", this, "_onPause");
            GetNode<PlayerVariables>("/root/PlayerVariables").Connect("items_changed", this, "displayItem");
        }

        public void onFade(string type)
        {
            if (type == "in" || type == "out")
            {
                GD.Print("Fade " + type);
                if (_fadeAnimation != null)
                {
                    _fadeAnimation.Play(type);
                }
                else
                {
                    GD.PrintErr("null");
                }
            }
            else
            {
                GD.PrintErr("Use \"in\" or \"out\"");
            }
        }

        public void onHealthChanged(float health)
        {
            _localHp = health;
            _health.Value = health;
            SceneTree tree = GetTree();
            tree.CreateTimer(2f).Connect("timeout", this, "_hpDrain");
        }

        public void startDialog(string id) {
            _dialogBox.Call("startDialog", id);
        }

        public void displayItem() {
            EmitSignal("items_changed");
        }
 
        public void _hpDrain()
        {
            Tween tween = GetNode<Tween>("Tween");
            tween.InterpolateProperty(_healthDrain, "value",
            _healthDrain.Value, _localHp, 0.3f,
            Tween.TransitionType.Linear, Tween.EaseType.InOut);
            tween.Start();
            _healthDrain.Value = _localHp;
        }

        private void _onPause(bool visible)
        {
            GD.Print(visible);
            EmitSignal("on_pause");
            _pauseContainer.Visible = visible;
        }
    }
}


