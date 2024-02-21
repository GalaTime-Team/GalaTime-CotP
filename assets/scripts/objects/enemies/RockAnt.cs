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

    public Navigator Navigator;
    public TargetController TargetController;
    public AttackSwitcher AttackSwitcher;

    /// <summary> If the rock ant is currently targetting (is positioning towards a target). </summary>
    public bool Targetting;

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
            new AttackCycle("dig", Dig, 1f)
        );
    }

    public void Dig()
    {
        AudioBurrow.Play();
        GetTree().CreateTimer(0.5f, false).Timeout += () => 
        {
            Sprite.Visible = false;
            EndDig();
        };
    }

    public void EndDig()
    {
        Targetting = true;

        var rnd = new Random();
        var delay = rnd.Next(1, 4);

        GetTree().CreateTimer(delay, false).Timeout += () => 
        {
            var ef = DangerNotifierEffect.GetInstance();
            AddChild(ef);
            ef.GlobalPosition = GlobalPosition;

            Targetting = false;

            ef.Start();
            GetTree().CreateTimer(0.35f, false).Timeout += () =>
            {
                Sprite.Visible = true;

                DigDamageArea.AttackStat = Stats[EntityStatType.PhysicalAttack].Value;
                DigDamageArea.HitOneTime();

                ef.QueueFree();
                AudioBurrow.Play();

                AttackSwitcher.NextCycle();
            };
        };
    }

    public override void _AIProcess(double delta)
    {
        Velocity = Vector2.Zero;

        var t = TargetController.CurrentTarget;
        if (t == null) return;

        if (!DeathState && !AttackSwitcher.IsAttackCycleActive("dig")) 
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

        if (Targetting)
        {
            GlobalPosition = t.GlobalPosition;
        }
        if (Targetting != Collision.Disabled) Callable.From(() => Collision.Disabled = Targetting).CallDeferred();
    }
}
