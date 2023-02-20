using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using Godot.Collections;
using System.Linq;

public class Slime : Entity
{
    private Player _player = null;
    private Vector2[] _path = null;
    private NavigationAgent2D _navigation = null;

    private Sprite _sprite;
    private Area2D _weapon;
    private AnimationPlayer _animationPlayer;

    private Timer _attackCountdownTimer;

    public EntityStats stats = new EntityStats
    {
        physicalAttack = 21,
        magicalAttack = 22,
        physicalDefence = 10,
        magicalDefense = 34,
        health = 19,
        mana = 15,
        stamina = 12,
        agility = 46
    };

    private bool _canMoveTest = true;
    public new float speed = 200;
    [Export] public float r = 1;

    public override void _Ready()
    {
        element = GalatimeElement.Aqua;
        body = this;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        _player = PlayerVariables.player;

        health = stats.health;
        droppedXp = 10;

        _sprite = GetNode<Sprite>("Sprite");
        _navigation = GetNode<NavigationAgent2D>("NavigationAgent");

        _weapon = GetNode<Area2D>("Weapon");

        _weapon.Connect("body_entered", this, "_attack");
        _weapon.Connect("body_exited", this, "_onAreaExit");

        _attackCountdownTimer = new Timer();
        _attackCountdownTimer.WaitTime = 1f;
        _attackCountdownTimer.OneShot = true;
        _attackCountdownTimer.Connect("timeout", this, "justHit");
        AddChild(_attackCountdownTimer);

        _animationPlayer.Play("intro");
    }

    public override void _moveProcess()
    {
        if (!_deathState) move(); else velocity = Vector2.Zero;
    }

    public override void _deathEvent(float damageRotation = 0f)
    {
        base._deathEvent();
        dynamic barrelItem = new
        {
            id = "barrel",
            max = 7,
            min = 1,
            chance = 75
        };

        lootPool.Add(barrelItem);

        _dropLoot(damageRotation);
        _dropXp();

        _canMoveTest = false;
        _animationPlayer.Play("outro");
    }

    public async void _attack(KinematicBody2D body)
    {
        if (!_deathState)
        {
            if (body is Entity entity)
            {
                _attackCountdownTimer.Start();
                GalatimeElement element = GalatimeElement.Aqua;
                float damageRotation = GlobalTransform.origin.AngleToPoint(entity.GlobalTransform.origin);
                entity.hit(20, stats.magicalAttack, element, DamageType.physical, 500, damageRotation);
            }
        }
    }

    public void justHit()
    {
        var bodies = _weapon.GetOverlappingBodies()[0] as KinematicBody2D;
        if (bodies is Entity entity)
        {
            _attackCountdownTimer.Start();
            GalatimeElement element = GalatimeElement.Aqua;
            float damageRotation = GlobalTransform.origin.AngleToPoint(entity.GlobalTransform.origin);
            entity.hit(20, stats.magicalAttack, element, DamageType.physical, 500, damageRotation);
        }
    }

    public void _onAreaExit(KinematicBody2D body)
    {
        _attackCountdownTimer.Stop();
    }

    public void move()
    {
        var enemies = GetTree().GetNodesInGroup("ally");
        Entity enemy = null;
        List<object> NonTypedEnemies = new List<object>();
        for (int i = 0; i < enemies.Count; i++)
        {
            NonTypedEnemies.Add(enemies[i]);
        }
        var sortedEnemies = NonTypedEnemies.OrderBy(x => x as Entity != null ? body.GlobalPosition.DistanceTo((x as Entity).GlobalPosition) : 0).ToList();
        sortedEnemies.RemoveAll(x => x as Entity != null ? (x as Entity)._deathState : false);
        if (sortedEnemies.ToList().Count > 0) enemy = sortedEnemies[0] as Entity;
        try
        {
            Vector2 vectorPath = Vector2.Zero;
            _navigation.SetTargetLocation(enemy.GlobalPosition);
            vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextLocation()) * speed;
            _navigation.SetVelocity(vectorPath);
            float rotation = body.GlobalTransform.origin.AngleToPoint(enemy.GlobalTransform.origin);
            _weapon.Rotation = rotation + r;
            float rotationDeg = Mathf.Rad2Deg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            if (rotationDegPositive >= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
            velocity = vectorPath;
        }
        catch (Exception err)
        {
            GD.PrintErr("CAN'T MOVE");
        }
    }
} 