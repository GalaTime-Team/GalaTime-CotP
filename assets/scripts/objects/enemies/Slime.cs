using Galatime;
using Galatime.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Slime : Entity
{
    #region Nodes
    private NavigationAgent2D Navigation = null;
    private Sprite2D Sprite;

    /// <summary> Area for the character's weapon. </summary>
    private Area2D Weapon;
    private AnimationPlayer AnimationPlayer;

    /// <summary> Timer for countdown to attack. </summary>
    private Timer AttackCountdownTimer;
    private TargetController TargetController;
    #endregion

    #region Variables
    /// <summary> Packed scene for slime enemies. </summary>
    private PackedScene SlimeScene;
    /// <summary> Character speed. </summary>
    #endregion

    public Slime() : base(new(
        PhysicalAttack: 20,
        PhysicalDefense: 20,
        MagicalDefense: 20,
        Health: 20
    ), GalatimeElement.Aqua) {}

    public override void _Ready()
    {
        base._Ready();
        Body = this;

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        SlimeScene = ResourceLoader.Load<PackedScene>("res://assets/objects/enemy/Slime.tscn");

        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        Sprite = GetNode<Sprite2D>("Sprite2D");
        Navigation = GetNode<NavigationAgent2D>("NavigationAgent3D");
        TargetController = GetNode<TargetController>("TargetController");
        TargetController.TargetTeam = Teams.Allies;

        Weapon = GetNode<Area2D>("Weapon");

        Weapon.BodyEntered += Attack;
        Weapon.BodyExited += OnAreaExit;

        AttackCountdownTimer = new Timer
        {
            WaitTime = 1f,
            OneShot = true
        };
        AttackCountdownTimer.Timeout += JustHit;
        AddChild(AttackCountdownTimer);

        AnimationPlayer.Play("intro");
    }

    public override void _ExitTree()
    {
        Weapon.BodyEntered -= Attack;
        Weapon.BodyExited -= OnAreaExit;
    }

    public override void _MoveProcess()
    {
        if (!DeathState) Move(); else Velocity = Vector2.Zero;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        base._DeathEvent();
        DropXp();
        AnimationPlayer.Play("outro");
    }

    public void Attack(Node2D body)
    {
        if (!DeathState && body is Entity entity) DealDamage(entity);
    }

    public void JustHit()
    {
        var bodies = Weapon.GetOverlappingBodies()[0] as CharacterBody2D;
        if (bodies is Entity entity) DealDamage(entity);
    }

    private void DealDamage(Entity entity)
    {
        AttackCountdownTimer.Start();
        GalatimeElement element = GalatimeElement.Aqua;
        float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
        entity.TakeDamage(30, Stats[EntityStatType.PhysicalAttack].Value, element, DamageType.physical, 500, damageRotation);
    }

    public void OnAreaExit(Node2D body) => AttackCountdownTimer.Stop();

    public void Move()
    {
        var enemy = TargetController.CurrentTarget;
        if (enemy != null)
        {
            Vector2 vectorPath = Vector2.Zero;
            Navigation.TargetPosition = enemy.GlobalPosition;
            vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition()) * Speed;
            float rotation = Body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
            Weapon.Rotation = rotation;
            float rotationDeg = Mathf.RadToDeg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            if (rotationDegPositive <= 90) Sprite.FlipH = true; else Sprite.FlipH = false;
            Velocity = vectorPath;
        }
        else Velocity = Vector2.Zero;
    }
}