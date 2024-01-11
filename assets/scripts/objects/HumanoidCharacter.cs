using Galatime;
using Godot;
using System.Collections.Generic;

using Galatime.Helpers;
using System;

public enum HumanoidValues
{
    Mana,
    Stamina
}

public partial class HumanoidCharacter : Entity
{
    /// <summary> If the character is dodging right now. </summary>
    protected bool IsDodge = false;
    /// <summary> If the character is able to dodge right now. </summary>
    protected bool CanDodge = true;
    /// <summary> The current rotation of the character. </summary>
    protected Vector2 VectorRotation;

    public HumanoidStates State = HumanoidStates.Idle;
    public bool IsWalk => State == HumanoidStates.Idle || State == HumanoidStates.Walk;

    /// <summary> The list of abilities of the character. </summary>
    public List<AbilityData> Abilities = new();

    protected Timer DodgeTimer;
    protected Timer AbilityCountdownTimer;

    // Required nodes for the character
    public HumanoidDoll HumanoidDoll;
    public Sprite2D Sprite;
    public GpuParticles2D TrailParticles;
    public Hand Weapon;
    public AudioStreamPlayer2D DrinkingAudioPlayer;
    public ResourceValue Mana;
    public ResourceValue Stamina;

    public Action<AbilityData, int> OnAbilityAdded;
    public Action OnAbilityRemoved;
    public Action<int, bool> OnAbilityUsed;
    public Action<int, double> OnAbilityReload;

    public override void _MoveProcess(double delta)
    {
        // Required for the rotate character animation.
        SetDirectionByWeapon();

        // Switching between idle and walk state.
        if (IsWalk) State = Body.Velocity.Length() <= 20 ? HumanoidStates.Idle : HumanoidStates.Walk;

        // Set the animation based on the velocity and the state.
        HumanoidDoll.SetAnimation(VectorRotation, State);

        // Set the trail particles texture to the same as the sprite texture.
        TrailParticles.Texture = Sprite?.Texture;
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
        for (int i = 0; i < PlayerVariables.AbilitySlots; i++) Abilities.Add(new());

        InitializeValues();
        InitializeTimers();

        OnDeath += OnCharacterDeath;
        OnRevived += OnCharacterRevived;
    }

    private void OnCharacterRevived()
    {
        Sprite.SelfModulate = new Color(1, 1, 1, 1);
    }

    private void OnCharacterDeath()
    {
        Sprite.SelfModulate = Sprite.SelfModulate with { A = 0.5f }; // TODO: Replace with death animation. This is just for testing.
    }

    public void InitializeValues()
    {
        Stamina = new("Stamina", Stats[EntityStatType.Stamina].Value, 3, 1);
        Mana = new("Mana", Stats[EntityStatType.Mana].Value, 3, 1);

        AddChild(Stamina);
        AddChild(Mana);

        Stamina.OnValueChanged += OnStaminaChanged;
        Mana.OnValueChanged += OnManaChanged;
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
            WaitTime = .5f,
            OneShot = true,
            Name = "AbilityCountdown"
        };
        AddChild(AbilityCountdownTimer);
    }

    protected void SetLayerToWeapon(bool toUp)
    {
        if (Weapon != null) if (toUp) Weapon.ZIndex = 1; else Weapon.ZIndex = 0;
    }

    public virtual AbilityData AddAbility(AbilityData ab, int i)
    {
        bool isValid(Node i) => i != null && IsInstanceValid(i);

        Abilities[i] = ab;
        if (ab.Reload > 0)
        {
            ref Timer cooldownTimer = ref Abilities[i].CooldownTimer;

            // Deleting previous timer if it exists to avoid memory leaks.
            if (isValid(cooldownTimer))
            {
                cooldownTimer.Stop();
                cooldownTimer.QueueFree();
                cooldownTimer = null;
            }

            // Creating new timer
            cooldownTimer = new Timer
            {
                Name = $"{ab.Name}TimerCountdown",
                WaitTime = ab.Reload,
                OneShot = true
            };
            cooldownTimer.Timeout += () => OnCooldownTimerTimeout(i);
            AddChild(cooldownTimer);

            // Starting timer if the ability is not reloaded.
            if (Abilities[i].Charges < Abilities[i].MaxCharges)
            {
                cooldownTimer.Start();
            }
        }
        OnAbilityAdded?.Invoke(Abilities[i], i);

        return Abilities[i];
    }

    private void OnCooldownTimerTimeout(int i)
    {
        var ability = Abilities[i];
        if (ability.Charges < ability.MaxCharges)
        {
            ability.Charges++;
            _OnAbilityReload(i);
            OnAbilityReload?.Invoke(i, 0);
            ability.CooldownTimer.Start();
        }
    }

    public virtual void _OnAbilityReload(int i)
    {
    }


    public override void _ExitTree()
    {
        // Deleting instances of ability timer from the abilities list to avoid memory leaks
        foreach (var ab in Abilities)
        {
            ab.CooldownTimer.QueueFree();
            ab.CooldownTimer = null;
        }

        OnDeath -= OnCharacterDeath;
        OnRevived -= OnCharacterRevived;
    }

    public virtual void RemoveAbility(int i)
    {
        AbilityData ability = new();
        Abilities[i] = ability;
        ability.CooldownTimer.Stop();
        OnAbilityRemoved?.Invoke();
    }

    public virtual bool UseAbility(int i)
    {
        var ability = Abilities[i];
        if (CanUseAbility(ability))
        {
            // Consume mana and stamina if needed.
            if (ability.Costs.Stamina > 0) Stamina.Value -= ability.Costs.Stamina;
            if (ability.Costs.Mana > 0) Mana.Value -= ability.Costs.Mana;

            AbilityCountdownTimer.Start();

            // Getting instance of ability and add data from json to ability.
            var abilityScene = ResourceLoader.Load<PackedScene>(ability.ScenePath);
            var abilityInstance = abilityScene.Instantiate<GalatimeAbility>();
            abilityInstance.Data = ability;

            // Add the ability and execute it.
            GetParent().AddChild(abilityInstance);
            abilityInstance.Execute(this);

            // Start the cooldown.
            ability.CooldownTimer.Stop();
            ability.CooldownTimer.Start();

            ability.Charges--;

            OnAbilityReload?.Invoke(i, 0);
            _OnAbilityReload(i);
        }
        else
        {
            OnAbilityUsed?.Invoke(i, false);
            return false;
        }
        OnAbilityUsed?.Invoke(i, true);
        return true;
    }

    /// <summary> Checks if ability can be used if you have enough currency and not on cooldown. </summary>
    /// <param name="i"> The index of the ability to check. </param>
    public bool CanUseAbility(AbilityData i) => 
        !i.IsEmpty && i.IsReloaded && AbilityCountdownTimer.TimeLeft <= 0 &&
        Stamina.Value - i.Costs.Stamina >= 0 && Mana.Value - i.Costs.Mana >= 0;

    public async void Dodge()
    {
        if (Stamina.Value - 15 >= 0 && !IsDodge && CanMove)
        {
            TrailParticles.Texture = Sprite?.Texture;
            IsDodge = true;
            float direction = Weapon.Rotation;
            SetKnockback(1200, direction);
            TrailParticles.Emitting = true;
            Stamina.Value -= 15;
            await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
            IsDodge = false;
            TrailParticles.Emitting = false;
        }
    }

    protected void SetDirectionByWeapon()
    {
        var r = Mathf.Wrap(Weapon.RotationDegrees, 0, 360);
        VectorRotation = r switch
        {
            <= 45 or >= 320 => Vector2.Right,
            >= 45 and <= 135 => Vector2.Down,
            >= 135 and <= 220 => Vector2.Left,
            >= 220 and <= 320 => Vector2.Up,
            _ => Vector2.Zero
        };
    }

    public void PlayDrinkingSound() => DrinkingAudioPlayer.Play();

    public void StepForward(float rotation) => SetKnockback(100, rotation);
}
