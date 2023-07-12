using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Galatime
{
    public partial class PlayerGui : Control
    {
        // Nodes
        public AnimationPlayer _fadeAnimation;
        // private AnimationPlayer _staminaAnimation;

        // Stats
        public TextureProgressBar _health;
        public TextureProgressBar _stamina;
        public TextureProgressBar _mana;
        public TextureProgressBar _healthDrain;

        // Text Stats
        public Godot.Label _textStamina;
        public Godot.Label _textHealth;
        public Godot.Label _textMana;
        public Godot.Label _dodgeCountdownText;
        public RichTextLabel _textStats;
        public Timer DodgeTextTimer;

        public Godot.Label _versionText;

        public NinePatchRect _dialogBox;

        public Panel _pauseContainer;

        public HBoxContainer abilitiesContainer;
        public AbilitiesContainer abilitiesListContainer;

        public GridContainer _statsContainer;

        public float _localHp = 0f;
        public float _remainingDodge;

        [Signal] public delegate void items_changedEventHandler();
        [Signal] public delegate void on_pauseEventHandler(bool visible);

        public override void _Ready()
        {
            _fadeAnimation = GetNode<AnimationPlayer>("fadeAnimation");
            // _staminaAnimation = GetNode<AnimationPlayer>("status/stamina_animation");

            // Stats
            _health = GetNode<TextureProgressBar>("HealthProgress");
            _stamina = GetNode<TextureProgressBar>("StaminaProgress");
            _mana = GetNode<TextureProgressBar>("ManaProgress");
            // _healthDrain = GetNode<TextureProgressBar>("status/hp_drain");

            // Text Stats
            _textStamina = GetNode<Godot.Label>("stamina_text");
            _textMana = GetNode<Godot.Label>("mana_text");
            _textHealth = GetNode<Godot.Label>("hp_text");
            _textStats = GetNode<RichTextLabel>("PauseContainer/Stats");
            _dodgeCountdownText = GetNode<Godot.Label>("LeftCenter/DodgeContainer/Countdown");

            _dialogBox = GetNode<NinePatchRect>("DialogBox");

            _pauseContainer = GetNode<Panel>("PauseContainer");

            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

            abilitiesContainer = GetNode<HBoxContainer>("AbilitiesContainer");
            abilitiesListContainer = GetNode<AbilitiesContainer>("PauseContainer/AbilitiesContainer");
            abilitiesListContainer._onAbilityLearned();

            _statsContainer = GetNode<GridContainer>("PauseContainer/StatsContainer/Stats");

            playerVariables.Connect("items_changed", new Callable(this, "displayItem"));

            DodgeTextTimer = new Timer();
            DodgeTextTimer.Connect("timeout", new Callable(this, "_reloadingDodge"));
            AddChild(DodgeTextTimer);

            _versionText = GetNode<Godot.Label>("Version");
            // _versionText.Text = $"PROPERTY OF GALATIME TEAM\nVersion {GalatimeConstants.version}\n{GalatimeConstants.versionDescription}";
            _versionText.Text = "";

            GD.Print("GUI!!!!");
            onFade("out");
        }

        public void _onStatsChanged(EntityStats stats)
        {
            _health.MaxValue = stats[EntityStatType.health].Value;
            _stamina.MaxValue = stats[EntityStatType.stamina].Value;
            _mana.MaxValue = stats[EntityStatType.mana].Value;
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
            // tree.CreateTimer(2f).Connect("timeout",new Callable(this,"_hpDrain"));
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
            //_textStats.Text =
            //    $"Hp: [color=white]{entityStats.health}[/color]\r\n" +
            //    $"Stamina: [color=white]{entityStats.stamina}[/color]\r\n" +
            //    $"Mana: [color=white]{entityStats.mana}[/color]\r\n" +
            //    $"XP: [rainbow]{XP}[/rainbow]\r\n\r\n" +
            //    $"Physic Attack: [color=white]{entityStats.physicalAttack}[/color]\r\n" +
            //    $"Magic Attack: [color=white]{entityStats.magicalAttack}[/color]\r\n" +
            //    $"Agility: [color=white]{entityStats.agility}[/color]\r\n\r\n" +
            //    $"Physic Defence: [color=white]{entityStats.physicalDefence}[/color]\r\n" +
            //    $"Magic Defence: [color=white]{entityStats.magicalAttack}[/color]";

            if (_statsContainer.GetChildCount() <= 0)
            {
                var statContainerScene = GD.Load<PackedScene>("res://assets/objects/gui/StatContainer.tscn");
                for (int i = 0; i < 8; i++)
                {
                    var instance = statContainerScene.Instantiate<StatContainer>();
                    _statsContainer.AddChild(instance);
                    instance.on_upgrade += (int id) => _onUpgradeStat(id, instance);
                }
            }

            var statContainers = _statsContainer.GetChildren();
            for (int i = 0; i < entityStats.Count; i++)
            {
                var statContainer = statContainers[i] as StatContainer;
                var stat = entityStats[i];
                statContainer.loadData(stat, (int)XP);
            }
        }

        public void _onUpgradeStat(int id, StatContainer instance)
        {
            var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
            var result = playerVariables.upgradeStat((EntityStatType)id);
            instance.playAnimation(result);
        }

        public void addAbility(GalatimeAbility ab, int i)
        {
            abilitiesContainer.GetChild<AbilityContainer>(i).load(ab.texture, ab.reload);
        }

        public void removeAbility(int i)
        {
            abilitiesContainer.GetChild<AbilityContainer>(i).unload();
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
            var ability = abilitiesContainer.GetChild(i) as AbilityContainer;
            ability.startReload();
        }

        public void pleaseSayNoToAbility(int i)
        {
            var ability = abilitiesContainer.GetChild(i) as AbilityContainer;
            ability.no();
        }

        public void startDialog(string id, Player p) {
            _dialogBox.Call("startDialog", id, p);
        }

        public void displayItem() {
            EmitSignal(SignalName.items_changed);
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
            EmitSignal(SignalName.on_pause);
            _pauseContainer.Visible = visible;
        }
    }
}


