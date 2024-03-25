using System;
using Galatime;
using Galatime.AI;
using Galatime.Helpers;
using Galatime.Damage;
using Godot;

public partial class RockAnt : Entity
{
    public Sprite2D Sprite;
    public AudioStreamPlayer2D AudioWalk;
    public AudioStreamPlayer2D AudioBurrow;
    public CollisionShape2D Collision;
    public DamageArea DigDamageArea;

    public DamageBeam SalivaBeam;
    public AnimationPlayer SalivaBeamAnimation;
    public DangerNotifierEffect DangerEffect;

    public Navigator Navigator;
    public TargetController TargetController;
    public AttackSwitcher AttackSwitcher;
    public RangedHitTracker RangedHitTracker;

    /// <summary> If the rock ant is currently targetting (is positioning towards a target). </summary>
    public bool DigTargetting;
    public bool SalivaTargetting;

    public override void _Ready()
    {
        base._Ready();

        Sprite = GetNode<Sprite2D>("Sprite2D");
        AudioWalk = GetNode<AudioStreamPlayer2D>("AudioWalk");
        AudioBurrow = GetNode<AudioStreamPlayer2D>("AudioBurrow");

        Collision = GetNode<CollisionShape2D>("Collision");

        Navigator = GetNode<Navigator>("Navigator");
        TargetController = GetNode<TargetController>("TargetController");
        AttackSwitcher = GetNode<AttackSwitcher>("AttackSwitcher");

        RangedHitTracker = GetNode<RangedHitTracker>("RangedHitTracker");
        SalivaBeam = GetNode<DamageBeam>("SalivaBeam");
        SalivaBeamAnimation = GetNode<AnimationPlayer>("SalivaBeam/AnimationPlayer");

        DigDamageArea = GetNode<DamageArea>("DigDamageArea");

        TargetController.OnTargetChanged += () =>
        {
            GD.Print("Changed target");
            Navigator.Target = TargetController.CurrentTarget;
        };

        Body = this;

        RegisterAttacks();
        AttackSwitcher.NextCycle();
    }

    public void RegisterAttacks()
    {
        AttackSwitcher.RegisterAttackCycles
        (
            new AttackCycle("dig", Dig, Reset, 1f/3f, () => !RangedHitTracker.CanHit),
            new AttackCycle("saliva", Saliva, Reset, 2f/3f) // 2:1 chance of saliva
        );
    }

    public override void _MoveProcess(double delta)
    {
        if (!DeathState) AttackSwitcher.Enabled = !DisableAI;
        if (DeathState) AttackSwitcher.Enabled = false;

        Velocity = Vector2.Zero;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        AttackSwitcher.Enabled = false;
        Sprite.Visible = false;
    }

    public void Saliva()
    {
        if (!RangedHitTracker.CanHit) 
        {
            AttackSwitcher.NextCycle();
            return;
        }

        SalivaBeamAnimation.Stop();
        var da = SalivaBeam.DamageArea;
        da.Element = Element;
        da.AttackStat = Stats[EntityStatType.MagicalAttack].Value;
        SalivaBeamAnimation.Play("shoot");
    }

    // Used in animation
    public void SetSalivaTargetting(bool value) => SalivaTargetting = value;

    public void Dig()
    {
        AudioBurrow.Play();
        GetTree().CreateTimer(0.5f, false).Timeout += () => 
        {
            AIIgnore = true;
            Sprite.Visible = false;
            EndDig();
        };
    }

    public void EndDig()
    {
        DigTargetting = true;

        var rnd = new Random();
        var delay = rnd.Next(1, 4);

        GetTree().CreateTimer(delay, false).Timeout += () => 
        {   
            DangerEffect = DangerNotifierEffect.GetInstance();
            AddChild(DangerEffect);
            DangerEffect.GlobalPosition = GlobalPosition;

            DigTargetting = false;

            DangerEffect.Start();
            GetTree().CreateTimer(0.35f, false).Timeout += () =>
            {
                AIIgnore = false;
                Sprite.Visible = true;

                DigDamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
                DigDamageArea.HitOneTime();

                DangerEffect.QueueFree();
                DangerEffect = null;
                AudioBurrow.Play();

                AttackSwitcher.NextCycle();
            };
        };
    }

    public void Reset()
    {
        SalivaTargetting = false;
        Sprite.Rotation = 0;

        DigTargetting = false;

        SalivaBeamAnimation.Stop();
        DangerEffect?.QueueFree();
        DangerEffect = null;
        Visible = true;
    }

    public override void _AIProcess(double delta)
    {
        Velocity = Vector2.Zero;

        var t = TargetController.CurrentTarget;
        // TODO: Change target angle rotation reference to target controller
        if (SalivaTargetting) 
            SalivaBeam.Rotation = RangedHitTracker.TargetAngleRotation;

        if (AttackSwitcher.IsAttackCycleActive("saliva"))
            Sprite.Rotation = RangedHitTracker.TargetAngleRotation + Mathf.Pi;
        else
            Sprite.Rotation = 0;

        if (!DeathState && !AttackSwitcher.IsAttackCycleActive("dig") && !AttackSwitcher.IsAttackCycleActive("saliva")) 
        {
            Navigator.Speed = Speed;

            var v = Navigator.NavigatorVelocity;
            if (v.Length() > 10 && t.GlobalPosition.DistanceTo(GlobalPosition) > 100)
            {
                if (!AudioWalk.Playing) AudioWalk.Play();

                Velocity = v;
                MoveAndSlide();
            }
            else
            {
                if (AudioWalk.Playing) AudioWalk.Stop();
            }
        }
        else
        {
            if (AudioWalk.Playing) AudioWalk.Stop();
        }

        if (DigTargetting)
        {
            GlobalPosition = t.GlobalPosition;
        }
        if (DigTargetting != Collision.Disabled) Callable.From(() => Collision.Disabled = DigTargetting).CallDeferred();
    }
}
