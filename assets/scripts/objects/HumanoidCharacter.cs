using Galatime;
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

public partial class HumanoidCharacter : Entity
{
    /// <summary> If the character is dodging right now. </summary>
    protected bool IsDodge = false;
    /// <summary> If the character is able to dodge right now. </summary>
    protected bool CanDodge = true;
    /// <summary> The current rotation of the character. </summary>
    protected Vector2 VectorRotation;
    /// <summary> If the character is able to move. </summary>
    public bool CanMove = true;

    /// <summary> The list of abilities of the character. </summary>
    public List<AbilityData> Abilities = new();

    protected Timer DodgeTimer;
    protected Timer AbilityCountdownTimer;
    protected Timer StaminaCountdownTimer;
    protected Timer StaminaRegenTimer;
    protected Timer ManaCountdownTimer;
    protected Timer ManaRegenTimer;

    protected AnimationPlayer AnimationPlayer;

    protected Sprite2D Sprite;
    protected GpuParticles2D TrailParticles;

    public Hand Weapon;

    private float mana;
    public float Mana
    {
        get { return mana; }
        set
        {
            mana = value;
            mana = Mathf.Clamp(mana, 0, Stats[EntityStatType.Mana].Value);
            ManaRegenTimer.Stop();
            ManaCountdownTimer.Start();
            OnManaChanged(mana);
        }
    }

    private float stamina;
    public float Stamina
    {
        get { return stamina; }
        set
        {
            stamina = value;
            stamina = Mathf.Clamp(stamina, 0, Stats[EntityStatType.Stamina].Value);
            StaminaRegenTimer.Stop();
            StaminaCountdownTimer.Start();
            OnStaminaChanged(stamina);
        }
    }

    protected virtual void OnManaChanged(float mana)
    {
    }

    protected virtual void OnStaminaChanged(float stamina)
    {
    }

    public override void _Ready()
    {
        base._Ready();

        // Initialize abilities
        Abilities = new List<AbilityData>();
        for (int i = 0; i < PlayerVariables.abilitySlots; i++) Abilities.Add(new());

        InitializeTimers();
    }

    private void InitializeTimers()
    {
        DodgeTimer = new Timer
        {
            WaitTime = 2f,
            OneShot = true
        };
        AddChild(DodgeTimer);

        AbilityCountdownTimer = new Timer
        {
            WaitTime = 1f,
            OneShot = true,
            Name = "AbilityCountdown"
        };
        AddChild(AbilityCountdownTimer);

        StaminaCountdownTimer = new Timer
        {
            WaitTime = 5f,
            OneShot = true,
            Name = "StaminaCountdown"
        };
        StaminaCountdownTimer.Timeout += _onCountdownStaminaRegen;
        AddChild(StaminaCountdownTimer);

        StaminaRegenTimer = new Timer
        {
            WaitTime = 1f,
            OneShot = false,
            Name = "StaminaRegenCountdown"
        };
        StaminaRegenTimer.Timeout += _regenStamina;
        AddChild(StaminaRegenTimer);

        ManaCountdownTimer = new Timer
        {
            WaitTime = 5f,
            OneShot = true,
            Name = "ManaCountdown"
        };
        ManaCountdownTimer.Timeout += _onCountdownManaRegen;
        AddChild(ManaCountdownTimer);

        ManaRegenTimer = new Timer
        {
            WaitTime = 1f,
            OneShot = false,
            Name = "ManaRegenCountdown"
        };
        ManaRegenTimer.Timeout += _regenMana;
        AddChild(ManaRegenTimer);
    }

    protected void _onCountdownStaminaRegen()
    {
        StaminaCountdownTimer.Stop();
        StaminaRegenTimer.Start();
    }

    protected void _onCountdownManaRegen()
    {
        ManaCountdownTimer.Stop();
        ManaRegenTimer.Start();
    }

    protected void _regenStamina()
    {
        stamina += 10;
        stamina = Mathf.Clamp(stamina, 0, Stats[EntityStatType.Stamina].Value);
        // EmitSignal("on_stamina_changed", stamina);
        if (stamina >= Stats[EntityStatType.Stamina].Value) StaminaRegenTimer.Stop();
        OnStaminaChanged(stamina);
    }

    protected void _regenMana()
    {
        mana += 10;
        mana = Mathf.Clamp(mana, 0, Stats[EntityStatType.Mana].Value);
        // EmitSignal("on_mana_changed", mana);
        if (mana >= Stats[EntityStatType.Mana].Value) ManaRegenTimer.Stop();
        OnManaChanged(mana);
    }

    protected void _setLayerToWeapon(bool toUp)
    {
        if (Weapon != null) if (toUp) Weapon.ZIndex = 1; else Weapon.ZIndex = 0;
    }

    public virtual AbilityData addAbility(AbilityData ab, int i)
    {
        Abilities[i] = ab;
        Abilities[i].CooldownTimer = new Timer
        {
            Name = $"{ab.Name}TimerCountdown",
            WaitTime = ab.Reload,
            OneShot = true
        };
        Abilities[i].CooldownTimer.Timeout += () => _OnCooldownTimerTimeout(i);
        AddChild(Abilities[i].CooldownTimer);
        return Abilities[i];
    }

    private void _OnCooldownTimerTimeout(int i)
    {
        var ability = Abilities[i];
        if (ability.Charges < ability.MaxCharges) {
            ability.Charges++;
            _OnAbilityReload(i);
            ability.CooldownTimer.Start();
        }
    }

    protected virtual void _OnAbilityReload(int i)
    {
    }


    public override void _ExitTree()
    {
        // Deleting isnstances of ability timer from the abilities list to avoid memory leaks
        foreach (var ab in Abilities)
        {
            ab.CooldownTimer.QueueFree();
            ab.CooldownTimer = null;
        }
    }

    protected virtual void RemoveAbility(int i)
    {
        AbilityData ability = new();
        Abilities[i] = ability;
        ability.CooldownTimer.Stop();
    }

    protected virtual bool _useAbility(int i)
    {
        try
        {
            var ability = Abilities[i];
            if (!ability.IsEmpty && ability.IsReloaded)
            {
                // Start the cooldown
                AbilityCountdownTimer.Start();

                // Getting instance of ability and add data from json to ability
                var abilityInstance = ability.Scene.Instantiate<GalatimeAbility>();
                abilityInstance.Data = ability;
                
                // Check if the character has enough stamina and mana, if not, return false.
                if (stamina - abilityInstance.Data.Costs.Stamina < 0) return false;
                if (abilityInstance.Data.Costs.Stamina > 0) Stamina -= abilityInstance.Data.Costs.Stamina;
                if (mana - abilityInstance.Data.Costs.Mana < 0) return false;
                if (abilityInstance.Data.Costs.Mana > 0) Mana -= abilityInstance.Data.Costs.Mana;

                // Add the ability and execute it.
                GetParent().AddChild(abilityInstance);
                abilityInstance.Execute(this);

                // Start the cooldown
                ability.CooldownTimer.Stop();
                ability.CooldownTimer.Start();

                ability.Charges--;
                _OnAbilityReload(i);
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
    }

    protected async void dodge()
    {
        if (Stamina - 15 >= 0 && !IsDodge && CanMove)
        {
            IsDodge = true;
            float direction = Weapon.Rotation;
            SetKnockback(1200, direction);
            TrailParticles.Emitting = true;
            Stamina -= 15;
            await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
            IsDodge = false;
            TrailParticles.Emitting = false;
        }
    }

    protected void setDirectionByWeapon()
    {
        var r = Mathf.Wrap(Weapon.RotationDegrees, 0, 360);
        var v = r switch
        {
            <= 45 or >= 320 => Vector2.Right,
            >= 45 and <= 135 => Vector2.Down,
            >= 135 and <= 220 => Vector2.Left,
            >= 220 and <= 320 => Vector2.Up,
            _ => Vector2.Zero
        };
        VectorRotation = v;
    }

    protected void _SetAnimation(Vector2 animationVelocity, bool idle)
    {
        if (idle) AnimationPlayer.Stop();
        AnimationPlayer.SpeedScale = Speed / 100;
        if (animationVelocity.Y != 0)
        {
            if (animationVelocity.Y <= -1 && AnimationPlayer.CurrentAnimation != "walk_up")
            {
                if (!idle) AnimationPlayer.Play("walk_up"); else AnimationPlayer.Play("idle_up");
            }
            if (animationVelocity.Y >= 1 && AnimationPlayer.CurrentAnimation != "walk_down")
            {
                if (!idle) AnimationPlayer.Play("walk_down"); else AnimationPlayer.Play("idle_down");
                _setLayerToWeapon(true);
            }
        }
        else
        {
            if (animationVelocity.X >= 1 && AnimationPlayer.CurrentAnimation != "walk_right")
            {
                if (!idle) AnimationPlayer.Play("walk_right"); else AnimationPlayer.Play("idle_right");
            }
            if (animationVelocity.X <= -1 && AnimationPlayer.CurrentAnimation != "walk_left")
            {
                if (!idle) AnimationPlayer.Play("walk_left"); else AnimationPlayer.Play("idle_left");
            }
        }
        _setLayerToWeapon(AnimationPlayer.CurrentAnimation == "idle_up" || AnimationPlayer.CurrentAnimation == "walk_up" ? false : true);
        TrailParticles.Texture = Sprite.Texture;
    }
}
