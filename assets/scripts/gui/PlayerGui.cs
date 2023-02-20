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

        private Player _player;

        // Stats
        private TextureProgress _health;
        private TextureProgress _stamina;
        private TextureProgress _mana;
        private TextureProgress _healthDrain;

        // Text Stats
        private Godot.Label _textStamina;
        private Godot.Label _textHealth;
        private Godot.Label _textMana;
        private Godot.Label _dodgeCountdownText;
        private RichTextLabel _textStats;
        public Timer DodgeTextTimer;

        private Godot.Label _versionText;

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

            _player = PlayerVariables.player;

            // Stats
            _health = GetNode<TextureProgress>("HealthProgress");
            _stamina = GetNode<TextureProgress>("StaminaProgress");
            _mana = GetNode<TextureProgress>("ManaProgress");
            // _healthDrain = GetNode<TextureProgress>("status/hp_drain");

            // Text Stats
            _textStamina = GetNode<Godot.Label>("stamina_text");
            _textMana = GetNode<Godot.Label>("mana_text");
            _textHealth = GetNode<Godot.Label>("hp_text");
            _textStats = GetNode<RichTextLabel>("PauseContainer/Stats");
            _dodgeCountdownText = GetNode<Godot.Label>("LeftCenter/DodgeContainer/Countdown");

            _dialogBox = GetNode<NinePatchRect>("DialogBox");

            _pauseContainer = GetNode<Panel>("PauseContainer");

            _abilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");

            _player.Connect("healthChanged", this, "onHealthChanged");
            _player.Connect("on_stamina_changed", this, "onStaminaChanged");
            _player.Connect("on_mana_changed", this, "onManaChanged");
            GetNode<PlayerVariables>("/root/PlayerVariables").Connect("items_changed", this, "displayItem");

            DodgeTextTimer = new Timer();
            DodgeTextTimer.Connect("timeout", this, "_reloadingDodge");
            AddChild(DodgeTextTimer);

            _versionText = GetNode<Godot.Label>("Version");
            _versionText.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.version}\n{GalatimeConstants.versionDescription}";
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

        public void onManaChanged(float mana)
        {
            // _staminaAnimation.Play("pulse");
            _mana.Value = mana;
            _textMana.Text = mana + " MANA";
        }

        public void changeStats(EntityStats entityStats, float XP)
        {
            _textStats.BbcodeText = 
                $"Hp: [color=white]{entityStats.health}[/color]\r\n" +
                $"Stamina: [color=white]{entityStats.stamina}[/color]\r\n" +
                $"Mana: [color=white]{entityStats.mana}[/color]\r\n" +
                $"XP: [rainbow]{XP}[/rainbow]\r\n\r\n" +
                $"Physic Attack: [color=white]{entityStats.physicalAttack}[/color]\r\n" +
                $"Magic Attack: [color=white]{entityStats.magicalAttack}[/color]\r\n" +
                $"Agility: [color=white]{entityStats.agility}[/color]\r\n\r\n" +
                $"Physic Defence: [color=white]{entityStats.physicalDefence}[/color]\r\n" +
                $"Magic Defence: [color=white]{entityStats.magicalAttack}[/color]";
        }

        public void addAbility(GalatimeAbility ab, int i)
        {
            _abilitiesContainer.GetChild<AbilityContainer>(i).load(ab.texture, ab.reload);
            GD.Print(ab.texture.GetPath());
        }

        public void removeAbility(int i)
        {
            _abilitiesContainer.GetChild<AbilityContainer>(i).unload();
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

        public void pause(bool visible)
        {
            EmitSignal("on_pause");
            _pauseContainer.Visible = visible;
        }
    }
}


