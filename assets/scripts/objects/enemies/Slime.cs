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

        GD.Print(stats.health);
    }

    public override void _moveProcess()
    {
        _line.GlobalPosition = Vector2.Zero;
        // move();
        velocity = Vector2.Zero;
    }

    public override void _deathEvent(float damageRotation = 0f)
    {
        dynamic testItem = new
        {
            id = "barrel",
            max = 7,
            min = 1,
            chance = 75
        };

        lootPool.Add(testItem);

        _dropLoot(damageRotation);

        _canMoveTest = false;
        _animationPlayer.Play("outro");
    }

    public void _attack(KinematicBody2D body)
    {
        Node2D parent = body.GetParent<Node2D>();
        // !!! NEEDS REWORK !!!
        GalatimeElement element = GalatimeElement.Aqua;

        // Get angle of damage
        float damageRotation = _sprite.GlobalTransform.origin.AngleToPoint(body.GlobalTransform.origin);
        GD.Print(damageRotation);
        if (parent.HasMethod("hit"))
        {
            parent.Call("hit", 4, element, 250, damageRotation);
        }
    }

    public void move() {
        try
        {
            Vector2 vectorPath = Vector2.Zero;
            _path = Navigation2DServer.MapGetPath(_navigation.GetNavigationMap(), body.GlobalPosition, _player.body.GlobalPosition, false);
            vectorPath = body.GlobalPosition.DirectionTo(_path[1]) * speed;
            if (_path[0] == body.GlobalPosition) _path.Skip(0).ToArray();
            _navigation.SetVelocity(body.GlobalPosition);
            float rotation = body.GlobalTransform.origin.AngleToPoint(_player.body.GlobalTransform.origin);
            _weapon.Rotation = rotation + r;
            float rotationDeg = Mathf.Rad2Deg(rotation);
            float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
            if (rotationDegPositive >= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
            velocity = vectorPath;
        }
        catch (Exception err)
        {
            // GD.PrintErr("CAN'T MOVE " + err.Message);
        }
        }
    }
