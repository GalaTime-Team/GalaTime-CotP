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
    private CollisionShape2D Collision;
    /// <summary> Reference to the player object. </summary>
    private Player Player;
    /// <summary> Timer for countdown to attack. </summary>
    private Timer AttackCountdownTimer;
    private TargetController TargetController;
    #endregion

    #region Variables
    /// <summary> Packed scene for slime enemies. </summary>
    private PackedScene SlimeScene;
    /// <summary> Flag indicating if character can move. </summary>
    private bool CanMoveTest = true;
    /// <summary> Character speed. </summary>
    public new float speed = 200;
    /// <summary> Current stage of the character, means how many times can be splited into parts. X means it will be splitted X time into 2 parts. </summary>
    public int Stage = 2;
    #endregion

    public override async void _Ready()
    {
        base._Ready();

        Element = GalatimeElement.Aqua;
        Body = this;
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        SlimeScene = ResourceLoader.Load<PackedScene>("res://assets/objects/enemy/Slime.tscn");

        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        Player = playerVariables.Player;
        Stats = new EntityStats(
                physicalAttack: 21,
                magicalAttack: 25,
                physicalDefence: 10,
                magicalDefence: 34,
                health: 5
            );

        DroppedXp = 10;

        Sprite = GetNode<Sprite2D>("Sprite2D");
        Navigation = GetNode<NavigationAgent2D>("NavigationAgent3D");
        Collision = GetNode<CollisionShape2D>("CollisionShape2D");
        TargetController = GetNode<TargetController>("TargetController");
        TargetController.TargetTeam = Teams.allies;

        Weapon = GetNode<Area2D>("Weapon");

        Weapon.Connect("body_entered", new Callable(this, "_attack"));
        Weapon.Connect("body_exited", new Callable(this, "_onAreaExit"));

        AttackCountdownTimer = new Timer
        {
            WaitTime = 1f,
            OneShot = true
        };
        AttackCountdownTimer.Timeout += justHit;
        AddChild(AttackCountdownTimer);

        AnimationPlayer.Play("intro");

        Stats[EntityStatType.Health].Value += 10 * Stage;
        Health = Stats[EntityStatType.Health].Value;

        speed -= 50 * Stage;

        if (Stage == 1)
        {
            var smallSlimeTexture = GD.Load<Texture2D>("res://sprites/small_slime.png");
            Sprite.Texture = smallSlimeTexture;
        }

        dynamic barrelItem = new
        {
            id = "barrel",
            max = 7,
            min = 1,
            chance = 75
        };

        LootPool.Add(barrelItem);

        // await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);

        // Collision.Disabled = false;
    }

    public override void _MoveProcess()
    {
        if (!DeathState) move(); else velocity = Vector2.Zero;
    }

    public override void _DeathEvent(float damageRotation = 0f)
    {
        base._DeathEvent();

        CanMoveTest = false;
        AnimationPlayer.Play("outro");

        // if (Stage >= 2)
        // {
        //     for (int i = 0; i < 2; i++)
        //     {
        //         var slimeInstance = SlimeScene.Instantiate<Slime>();
        //         slimeInstance.Stage = Stage - 1;
        //         var position = GlobalPosition;
        //         position.Y -= i == 0 ? 40 : -40;
        //         slimeInstance.GlobalPosition = position;
        //         GetParent().AddChild(slimeInstance);
        //         slimeInstance.setKnockback(500, i == 0 ? damageRotation - Mathf.DegToRad(45) : damageRotation + Mathf.DegToRad(45));
        //     }
        // }
        // else
        // {
        //     _dropXp();
        //     _dropLoot(damageRotation);
        // }
    }

    public void _attack(CharacterBody2D body)
    {
        if (!DeathState)
        {
            if (body is Entity entity)
            {
                AttackCountdownTimer.Start();
                GalatimeElement element = GalatimeElement.Aqua;
                float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
                entity.TakeDamage(20 + 10 * Stage, Stats[EntityStatType.MagicalAttack].Value, element, DamageType.physical, 500, damageRotation);
            }
        }
    }

    public void justHit()
    {
        var bodies = Weapon.GetOverlappingBodies()[0] as CharacterBody2D;
        if (bodies is Entity entity)
        {
            AttackCountdownTimer.Start();
            GalatimeElement element = GalatimeElement.Aqua;
            float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
            entity.TakeDamage(20 + 10 * Stage, Stats[EntityStatType.MagicalAttack].Value, element, DamageType.physical, 500, damageRotation);
        }
    }

    public void _onAreaExit(CharacterBody2D body)
    {
        AttackCountdownTimer.Stop();
    }

    public void move()
    {
        var enemy = TargetController.CurrentTarget;
        if (enemy != null)
        {
            try
            {
                Vector2 vectorPath = Vector2.Zero;
                Navigation.TargetPosition = enemy.GlobalPosition;
                vectorPath = Body.GlobalPosition.DirectionTo(Navigation.GetNextPathPosition()) * speed;
                float rotation = Body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
                Weapon.Rotation = rotation;
                float rotationDeg = Mathf.RadToDeg(rotation);
                float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
                if (rotationDegPositive <= 90) Sprite.FlipH = true; else Sprite.FlipH = false;
                velocity = vectorPath;
            }
            catch (Exception err)
            {
            }
        }
        else
        {
            velocity = Vector2.Zero;
        }
    }
}