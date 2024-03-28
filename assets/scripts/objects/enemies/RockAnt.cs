using System;

using Galatime;
using Galatime.AI;
using Galatime.Helpers;
using Galatime.Damage;
using ExtensionMethods;

using Godot;
using Galatime.Global;

public partial class RockAnt : Entity
{
    public Sprite2D Sprite;
    public AudioStreamPlayer2D AudioWalk;
    public AudioStreamPlayer2D AudioBurrow;
    public CollisionShape2D Collision;
    public DamageArea DigDamageArea;
    public DamageArea DamageArea;

    public DangerNotifierEffect DangerEffect;

    public Navigator Navigator;
    public TargetController TargetController;
    public AttackSwitcher AttackSwitcher;
    public RangedHitTracker RangedHitTracker;

    /// <summary> If the rock ant is currently targetting (is positioning towards a target). </summary>
    public bool DigTargetting;

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
        DigDamageArea = GetNode<DamageArea>("DigDamageArea");
        DamageArea = GetNode<DamageArea>("DamageArea");

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
            new AttackCycle("dig", Dig, Reset, .25f, () => !RangedHitTracker.CanHit),
            new AttackCycle("melee", () => {
                DamageArea.Active = true;
                DamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
                
                AttackSwitcher.StartTimer("melee", () => 
                {
                    DamageArea.Active = false;
                    AttackSwitcher.NextCycle();
                }, 1f);
            }, Reset, .75f)
        );
    }

    public override void _MoveProcess(double delta)
    {
        bool on = true;
        if (!DeathState) on = !DisableAI;
        if (DeathState || !TargetController.HasTarget) on = false;
        AttackSwitcher.Enabled = on;

        Velocity = Vector2.Zero;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        AttackSwitcher.Enabled = false;
        Reset();

        // Make sprite red so it's obvious it's dead.
        Sprite.Modulate = GameColors.Red;
    }

    public void Dig()
    {
        AudioBurrow.Play();
        AttackSwitcher.StartTimer("dig", () => 
        {
            AIIgnore = true;
            Sprite.Visible = false;
            EndDig();
        }, .5f);
    }

    public void EndDig()
    {
        DigTargetting = true;

        var rnd = new Random();
        var delay = rnd.Next(1, 4);

        AttackSwitcher.StartTimer("dig", () => 
        {
            DangerEffect = DangerNotifierEffect.GetInstance();
            AddChild(DangerEffect);
            DangerEffect.GlobalPosition = GlobalPosition;

            DigTargetting = false;

            DangerEffect.Start();
            AttackSwitcher.StartTimer("dig", () => 
            {
                AIIgnore = false;
                Sprite.Visible = true;

                DigDamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
                DigDamageArea.HitOneTime();

                DangerEffect.QueueFree();
                DangerEffect = null;
                AudioBurrow.Play();

                AttackSwitcher.NextCycle();
            }, .35f);
        }, delay);
    }

    public void Reset()
    {
        DigTargetting = false;

        DangerEffect?.QueueFree();
        DangerEffect = null;

        Sprite.Visible = true;
        Callable.From(() => Collision.Disabled = false).CallDeferred();

        DamageArea.Active = false;
        DigDamageArea.Active = false;

        AudioWalk.Stop();
    }

    public override void _AIProcess(double delta)
    {
        Velocity = Vector2.Zero;

        var t = TargetController.CurrentTarget;
        // If no target, do nothing
        if (t == null || DeathState) return;

        if (AttackSwitcher.IsAttackCycleActive("melee"))
        {
            DamageArea.Rotation = GlobalPosition.AngleToPoint(t.GlobalPosition) + (float)Math.PI;

            Navigator.Speed = Speed;
            var v = Navigator.NavigatorVelocity;
            
            if (!AudioWalk.Playing) AudioWalk.Play();
            if (v.Length() < 10 && AudioWalk.Playing) AudioWalk.Stop();

            Velocity = v;
            MoveAndSlide();
        }
        else
            if (AudioWalk.Playing) AudioWalk.Stop();

        // Targetting behavior
        if (DigTargetting)
            GlobalPosition = t.GlobalPosition;

        // Disable collision if targetting.
        if (DigTargetting != Collision.Disabled) Callable.From(() => Collision.Disabled = DigTargetting).CallDeferred();
 
        // Flip sprite based on direction.
        Sprite.FlipSpriteByAngle(GlobalPosition.AngleToPoint(t.GlobalPosition));
    }
}
