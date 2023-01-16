using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using Godot.Collections;
using System.Linq;

public class Slime : Entity
{
    private Navigation2D _navigation = null;
    private Player _player = null;
    private Vector2[] _path = null;
    private Line2D _line = null;

    private Sprite _sprite;
    private Area2D _weapon;
    private AnimationPlayer _animationPlayer;
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

        _weapon = GetNode<Area2D>("Body/Weapon");

        health = 10;

        _weapon.Connect("body_entered", this, "_attack");

        var tree = GetTree();
        if (tree.HasGroup("levelNavigation"))
        {
            var nodes = tree.GetNodesInGroup("levelNavigation");
            _navigation = nodes[0] as Navigation2D;
        }
        else
        {
            GD.Print("navigation doesn't found");
        }

    }

    public override void _moveProcess()
    {
        _line.GlobalPosition = Vector2.Zero;
        findPath();
        generatePath();
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

    public void generatePath()
    {
        if (_navigation != null && _player != null)
        {
            
            _path = _navigation.GetSimplePath(body.GlobalPosition, _player.body.GlobalPosition, false);
            _line.Points = _path;
        }
        else
        {
            GD.Print("no navigator or player");
        }
    }

    public void _attack(KinematicBody2D body) {
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

    public void findPath() {
        Vector2 vectorPath = Vector2.Zero;
        try
        {
            if (Node2D.IsInstanceValid(_player))
            {
                float rotation = body.GlobalTransform.origin.AngleToPoint(_player.body.GlobalTransform.origin);
                _weapon.Rotation = rotation + r;
                if (_path != null)
                {
                    if (_path.Length > 0)
                    {
                        vectorPath = body.GlobalPosition.DirectionTo(_path[2]) * speed;
                        if (body.GlobalPosition == _path[0])
                        {
                            _path = _path.Skip(1).ToArray();
                            GD.Print("finished point");
                        }
                    }
                }
                float rotationDeg = Mathf.Rad2Deg(rotation);
                float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
                if (rotationDegPositive >= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
            }
            velocity = vectorPath;
        }
        catch (Exception err)
        {
            GD.PrintErr("CAN'T MOVE " + err.Message);
        }
        }
    }
