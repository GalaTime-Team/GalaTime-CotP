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
    private Line2D _line = null;
    private NavigationAgent2D _navigation = null;

    private Sprite _sprite;
    private Area2D _weapon;
    private AnimationPlayer _animationPlayer;

    public new EntityStats stats = new EntityStats 
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
    public new float speed = 100;
    [Export] public float r = 1;

    public override void _Ready()
    {
        element = GalatimeElement.Aqua;
        body = GetNode<KinematicBody2D>("Body");
        damageEffectPoint = GetNode<Position2D>("Body/DamageEffectPoint");
        _animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");

        _player = GetNode<Player>("/root/Node2D/Player");
        _line = GetNode<Line2D>("Line");

        _sprite = GetNode<Sprite>("Body/Sprite");
        _navigation = GetNode<NavigationAgent2D>("Body/NavigationAgent");

        _weapon = GetNode<Area2D>("Body/Weapon");

        health = 10;

        _weapon.Connect("body_entered", this, "_attack");
    }

    public override void _moveProcess()
    {
        _line.GlobalPosition = Vector2.Zero;
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

        _canMoveTest = false;
        _animationPlayer.Play("outro");
    }

    public void _attack(KinematicBody2D body)
    {
        if (!_deathState)
        {
            Entity parent = body.GetParent<Entity>();
            // !!! NEEDS REWORK !!!
            GalatimeElement element = GalatimeElement.Aqua;

            // Get angle of damage
            float damageRotation = _sprite.GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
            if (parent.HasMethod("hit"))
            {
                parent.hit(30, stats.physicalAttack, element, DamageType.physical, 250, damageRotation);
            }
        }
    }

    public void move() {
        try
        {
            Vector2 vectorPath = Vector2.Zero;
            _navigation.SetTargetLocation(_player.body.GlobalPosition);
            _line.Points = _navigation.GetNavPath();
            vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextLocation()) * speed;
            _navigation.SetVelocity(vectorPath);
            float rotation = body.GlobalTransform.origin.AngleToPoint(_player.body.GlobalTransform.origin);
            _weapon.Rotation = rotation + r;
            float rotationDeg = Mathf.Rad2Deg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            if (rotationDegPositive >= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
            velocity = vectorPath;
        }
        catch (Exception err)
        {
            GD.PrintErr("CAN'T MOVE " + _path.Length);
        }
        }
    }
    