using Godot;
using System;

namespace Galatime {
    public class AbilityContainer : VBoxContainer
    {
        public Label label;
        public TextureProgress sprite;
        public AnimationPlayer animationPlayer;
        public float reloadTime;

        public Timer clockTimer;
        public Timer textTimer;

        public Texture defaultTexture = GD.Load<Texture>("res://sprites/gui/abilities/empty.png"); 

        private float _remaining;
        private float _delay;
        private float _shakeAmount = 0;

        public override void _Ready()
        {
            sprite = GetNode<TextureProgress>("Sprite");
            label = GetNode<Label>("Label");
            clockTimer = GetNode<Timer>("ClockTimer");
            textTimer = GetNode<Timer>("TextTimer");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            textTimer.Connect("timeout", this, "_textLoading");
            clockTimer.Connect("timeout", this, "_loading");
        }

        public override void _Process(float delta)
        {
            _shake();
        }

        private void _shake()
        {
            var shakeOffset = new Vector2();

            Random rnd = new();
            shakeOffset.x = rnd.Next(-1, 2) * _shakeAmount;
            shakeOffset.y = rnd.Next(-1, 2) * _shakeAmount + 10;

            _shakeAmount = Mathf.Lerp(_shakeAmount, 0, 0.05f);

            sprite.RectPosition = shakeOffset;
        }

        public void load(Texture texture, float reload)
        {
            sprite.TextureUnder = texture;
            reloadTime = reload;
        }

        public void unload()
        {
            sprite.TextureUnder = defaultTexture;
            reloadTime = 2;
        }

        public void startReload()
        {
            // if (reloadTime <= 0) GD.Print("Ability is less that 0"); return;

            sprite.Value = 100;

            _remaining = reloadTime;
            label.Text = _remaining + "s";

            _delay = reloadTime / 100;
            clockTimer.WaitTime = _delay;

            clockTimer.Start();
            textTimer.Start();
        }

        public void no()
        {
            _shakeAmount += 2;
            animationPlayer.Stop();
            animationPlayer.Play("no");
        }

        public void click() 
        {
            _shakeAmount += 0.5f;
            animationPlayer.Stop();
            animationPlayer.Play("click");
        }

        public async void _loading()
        {
            sprite.Value--;
            if (sprite.Value <= 0) clockTimer.Stop();
        }

        public async void _textLoading()
        {
            _remaining--;
            label.Text = _remaining + "s";
            if (_remaining <= 0) textTimer.Stop();
            if (sprite.Value <= 0) label.Text = "";
        }
    }
}
