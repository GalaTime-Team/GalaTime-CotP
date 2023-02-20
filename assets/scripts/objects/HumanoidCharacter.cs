using Galatime;
using Godot;
using System;

public class HumanoidCharacter : Entity
{
    protected bool _isDodge = false;
    protected bool _canDodge = true;
    public bool canMove = true;
    public string IdleAnimation = "idle_down";

    protected Vector2 _vectorRotation;

    public PackedScene[] _abilities = new PackedScene[3];
    public Timer[] _abilitiesTimers = new Timer[3];
    public int[] _abiltiesReloadTimes = new int[3];

    protected Timer _dodgeTimer;
    protected Timer _abilityCountdownTimer;
    protected Timer _staminaCountdownTimer;
    protected Timer _staminaRegenTimer;
    protected Timer _manaCountdownTimer;
    protected Timer _manaRegenTimer;

    protected AnimationPlayer _animationPlayer;

    protected Sprite _sprite;
    protected Particles2D _trailParticles;

    public Hand weapon;

    [Signal] public delegate void on_stamina_changed(float stamina);
    [Signal] public delegate void on_mana_changed(float mana);

    protected float mana;
    public float Mana
    {
        get { return mana; }
        set
        {
            mana = value;
            mana = Mathf.Clamp(mana, 0, stats.mana);
            _manaRegenTimer.Stop();
            _manaCountdownTimer.Start();
            EmitSignal("on_mana_changed", mana);
        }
    }

    protected float stamina;
    public float Stamina
    {
        get { return stamina; }
        set
        {
            stamina = value;
            stamina = Mathf.Clamp(stamina, 0, stats.stamina);
            _staminaRegenTimer.Stop();
            _staminaCountdownTimer.Start();
            EmitSignal("on_stamina_changed", stamina);
        }
    }

    public override void _Ready()
    {
        _dodgeTimer = new Timer();
        _dodgeTimer.WaitTime = 2f;
        _dodgeTimer.OneShot = true;
        _dodgeTimer.Connect("timeout", this, "_onCountdownDodge");
        AddChild(_dodgeTimer);

        _abilityCountdownTimer = new Timer();
        _abilityCountdownTimer.WaitTime = 1f;
        _abilityCountdownTimer.OneShot = true;
        AddChild(_abilityCountdownTimer);

        _staminaCountdownTimer = new Timer();
        _staminaCountdownTimer.WaitTime = 5f;
        _staminaCountdownTimer.OneShot = true;
        _staminaCountdownTimer.Connect("timeout", this, "_onCountdownStaminaRegen");
        AddChild(_staminaCountdownTimer);

        _staminaRegenTimer = new Timer();
        _staminaRegenTimer.WaitTime = 1f;
        _staminaRegenTimer.OneShot = false;
        _staminaRegenTimer.Connect("timeout", this, "_regenStamina");
        AddChild(_staminaRegenTimer);

        _manaCountdownTimer = new Timer();
        _manaCountdownTimer.WaitTime = 5f;
        _manaCountdownTimer.OneShot = true;
        _manaCountdownTimer.Connect("timeout", this, "_onCountdownManaRegen");
        AddChild(_manaCountdownTimer);

        _manaRegenTimer = new Timer();
        _manaRegenTimer.WaitTime = 1f;
        _manaRegenTimer.OneShot = false;
        _manaRegenTimer.Connect("timeout", this, "_regenMana");
        AddChild(_manaRegenTimer);
    }

    protected void _onCountdownStaminaRegen()
    {
        _staminaCountdownTimer.Stop();
        _staminaRegenTimer.Start();
    }

    protected void _onCountdownManaRegen()
    {
        _manaCountdownTimer.Stop();
        _manaRegenTimer.Start();
    }

    protected void _regenStamina()
    {
        stamina += 10;
        stamina = Mathf.Clamp(stamina, 0, stats.stamina);
        EmitSignal("on_stamina_changed", stamina);
        heal(5);
        if (stamina >= stats.stamina) _staminaRegenTimer.Stop();
    }

    protected void _regenMana()
    {
        mana += 10;
        mana = Mathf.Clamp(mana, 0, stats.mana);
        EmitSignal("on_mana_changed", mana);
        if (mana >= stats.mana) _manaRegenTimer.Stop();
    }

    protected void _setLayerToWeapon(bool toUp)
    {
        if (weapon != null) if (toUp) weapon.ZIndex = 1; else weapon.ZIndex = 0;
    }

    public virtual GalatimeAbility addAbility(string scenePath, int i)
    {
        PackedScene scene = GD.Load<PackedScene>(scenePath);
        GalatimeAbility ability = scene.Instance<GalatimeAbility>();
        _abilities[i] = scene;
        var binds = new Godot.Collections.Array();
        binds.Add(i);
        if (_abilitiesTimers[i] != null) _abilitiesTimers[i].Stop();
        _abiltiesReloadTimes[i] = 0;
        _abilitiesTimers[i] = new Timer();
        _abilitiesTimers[i].Connect("timeout", this, "_abilitiesCountdown", binds);
        AddChild(_abilitiesTimers[i]);
        return ability;
    }

    protected virtual void removeAbility(int i)
    {
        _abilities[i] = null;
        if (_abilitiesTimers[i] != null) _abilitiesTimers[i].Stop();
        _abiltiesReloadTimes[i] = 0;
    }

    protected virtual void _abilitiesCountdown(int i)
    {
        if (_abiltiesReloadTimes[i] <= 0) _abilitiesTimers[i].Stop();
        _abiltiesReloadTimes[i]--;
        GD.Print(_abiltiesReloadTimes[i] + " time");
    }

    protected virtual bool _useAbility(int i)
    {
        try
        {
            if (_abiltiesReloadTimes[i] <= 0 && _abilityCountdownTimer.TimeLeft == 0)
            {
                _abilityCountdownTimer.Start();
                var ability = _abilities[i].Instance<GalatimeAbility>();
                if (ability.costs.ContainsKey("stamina"))
                {
                    if (stamina - ability.costs["stamina"] < 0)
                    {
                        EmitSignal("sayNoToAbility", i); return false;
                    }
                    Stamina -= ability.costs["stamina"];
                }
                if (ability.costs.ContainsKey("mana"))
                {
                    if (mana - ability.costs["mana"] < 0)
                    {
                        EmitSignal("sayNoToAbility", i); return false;
                    }
                    Mana -= ability.costs["mana"];
                    GD.Print($"mana cost {ability.costs["mana"]}");
                }
                GetParent().AddChild(ability);
                ability.execute(this, stats.physicalAttack, stats.magicalAttack);
                _abilitiesTimers[i].Stop();
                _abilitiesTimers[i].Start();
                _abiltiesReloadTimes[i] = (int)Math.Round(ability.reload);
            }
            else
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error when used ability: " + ex.Message);
            return false;
        }
        return true;
    }

    protected async void dodge()
    {
        if (Stamina - 20 >= 0 && _dodgeTimer.TimeLeft <= 0 && canMove)
        {
            _isDodge = true;
            float direction = weapon.Rotation + 3.14159f;
            setKnockback(1200, direction);
            _trailParticles.Emitting = true;
            Stamina -= 15;
            _dodgeTimer.Start();
            EmitSignal("reloadDodge");
            await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
            _isDodge = false;
            _trailParticles.Emitting = false;
        }
    }

    protected void setDirectionByWeapon()
    {
        var r = Mathf.Wrap(weapon.RotationDegrees, 0, 360);
        var v = Vector2.Zero;
        if (r <= 45) v = Vector2.Right;
        if (r >= 45 && r <= 135) v = Vector2.Down;
        if (r >= 135 && r <= 220) v = Vector2.Left;
        if (r >= 220 && r <= 320) v = Vector2.Up;
        if (r >= 320) v = Vector2.Right;
        _vectorRotation = v;
    }

    protected void _SetAnimation(Vector2 animationVelocity, bool idle)
    {
        _setLayerToWeapon(_animationPlayer.CurrentAnimation == "idle_up" || _animationPlayer.CurrentAnimation == "walk_up" ? false : true);
        if (idle) _animationPlayer.Stop();  
        if (animationVelocity.y != 0)
        {
            if (animationVelocity.y <= -1 && _animationPlayer.CurrentAnimation != "walk_up")
            {
                if (!idle) _animationPlayer.Play("walk_up"); else _animationPlayer.Play("idle_up");
            }
            if (animationVelocity.y >= 1 && _animationPlayer.CurrentAnimation != "walk_down")
            {
                if (!idle) _animationPlayer.Play("walk_down"); else _animationPlayer.Play("idle_down");
                _setLayerToWeapon(true);
            }
        }
        else
        {
            if (animationVelocity.x >= 1 && _animationPlayer.CurrentAnimation != "walk_right")
            {
                if (!idle) _animationPlayer.Play("walk_right"); else _animationPlayer.Play("idle_right");
            }
            if (animationVelocity.x <= -1 && _animationPlayer.CurrentAnimation != "walk_left")
            {
                if (!idle) _animationPlayer.Play("walk_left"); else _animationPlayer.Play("idle_left");
            }
        }
        _trailParticles.Texture = _sprite.Texture;
    }
}
