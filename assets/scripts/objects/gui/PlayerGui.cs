using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Galatime
{
    public class PlayerGui : Control
    {
        // Nodes
        private AnimationPlayer _fadeAnimation;
        // private AnimationPlayer _staminaAnimation;

        private Node _player;

        // Stats
        private TextureProgress _health;
        private TextureProgress _stamina;
        private TextureProgress _healthDrain;

        // Text Stats
        private Godot.Label _textStamina;
        private Godot.Label _textHealth;
        private Godot.Label _dodgeCountdownText;
        public Timer DodgeTextTimer;

        private NinePatchRect _dialogBox;

        private Panel _pauseContainer;

        private HBoxContainer _abilitiesContainer;

        private float _localHp = 0f;
        private float _remainingDodge;

        [Signal] public delegate void items_changed();
        [Signal] public delegate void on_pause(bool visible);

        public override void _Ready()
        {
            _fadeAnimation = GetNode<AnimationPlayer>("fadeAnimation");
            // _staminaAnimation = GetNode<AnimationPlayer>("status/stamina_animation");

            _player = GetNode("/root/Node2D/Player");

            // Stats
            _health = GetNode<TextureProgress>("HealthProgress");
            _stamina = GetNode<TextureProgress>("StaminaProgress");
            // _healthDrain = GetNode<TextureProgress>("status/hp_drain");

            // Text Stats
            _textStamina = GetNode<Godot.Label>("stamina_text");
            _textHealth = GetNode<Godot.Label>("hp_text");
            _dodgeCountdownText = GetNode<Godot.Label>("LeftCenter/DodgeContainer/Countdown");

            _dialogBox = GetNode<NinePatchRect>("DialogBox");

            _pauseContainer = GetNode<Panel>("PauseContainer");

            _abilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");

            _player.Connect("fade", this, "onFade");
            _player.Connect("healthChanged", this, "onHealthChanged");
            _player.Connect("on_stamina_changed", this, "onStaminaChanged");
            _player.Connect("on_dialog_start", this, "startDialog");
            _player.Connect("on_pause", this, "_onPause");
            _player.Connect("on_ability_add", this, "addAbility");
            _player.Connect("reloadAbility", this, "reloadAbility");
            _player.Connect("reloadDodge", this, "reloadDodge");
            _player.Connect("sayNoToAbility", this, "pleaseSayNoToAbility");
            GetNode<PlayerVariables>("/root/PlayerVariables").Connect("items_changed", this, "displayItem");

            DodgeTextTimer = new Timer();
            DodgeTextTimer.Connect("timeout", this, "_reloadingDodge");
            AddChild(DodgeTextTimer);
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
            // _localHp = health;
            _health.Value = health;
            _textHealth.Text = health + " HP";
            // SceneTree tree = GetTree();
            // tree.CreateTimer(2f).Connect("timeout", this, "_hpDrain");
        }

        public void onStaminaChanged(float stamina)
        {
            // _staminaAnimation.Play("pulse");
            _stamina.Value = stamina;
            _textStamina.Text = stamina + " STAM";
        }

        public void addAbility(GalatimeAbility ab, int i)
        {
            _abilitiesContainer.GetChild<AbilityContainer>(i).load(ab.texture, ab.reload);
        }

        public void reloadDodge()
        {
            _remainingDodge = 2;
            DodgeTextTimer.Start();
            _reloadingDodge();
        }

        public async void _reloadingDodge()
        {
            _remainingDodge--;
            _dodgeCountdownText.Text = _remainingDodge + "s";
            if (_dodgeCountdownText.Text == "-1s")
            {
                DodgeTextTimer.Stop(); _dodgeCountdownText.Text = "";
            }
        }

        public void reloadAbility(int i)
        {
            var ability = _abilitiesContainer.GetChild(i) as AbilityContainer;
            ability.startReload();
        }

        public void pleaseSayNoToAbility(int i)
        {
            var ability = _abilitiesContainer.GetChild(i) as AbilityContainer;
            ability.no();
        }

        public void startDialog(string id, Player p) {
            _dialogBox.Call("startDialog", id, p);
        }

        public void displayItem() {
            EmitSignal("items_changed");
        }
 
        //public void _hpDrain()
        //{
            //Tween tween = GetNode<Tween>("Tween");
            //tween.InterpolateProperty(_healthDrain, "value",
            //_healthDrain.Value, _localHp, 0.3f,
            //Tween.TransitionType.Linear, Tween.EaseType.InOut);
            //tween.Start();
            //_healthDrain.Value = _localHp;
        //}

        private void _onPause(bool visible)
        {
            EmitSignal("on_pause");
            _pauseContainer.Visible = visible;
        }
    }
}


