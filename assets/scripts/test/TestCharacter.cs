using Godot;
using System;
using Galatime;
using Newtonsoft.Json.Schema;
using System.Linq;
using System.Collections.Generic;

public class TestCharacter : HumanoidCharacter
{
    private Vector2[] _path = null;
    private Line2D _line = null;
    private NavigationAgent2D _navigation = null;
    private RayCast2D _rayCast;

    private Timer retreatDelay;
    private Timer moveDelay;

    private Timer enemySwitchDelay;

    private Player _player;

    private Entity currentEnemy = null;

    public override void _Ready()
    {
        base._Ready();
        _animationPlayer = GetNode<AnimationPlayer>("Animation");
        _sprite = GetNode<Sprite>("Sprite");
        _trailParticles = GetNode<Particles2D>("TrailParticles");
        weapon = GetNode<Hand>("Hand");

        _navigation = GetNode<NavigationAgent2D>("NavigationAgent");
        _rayCast = GetNode<RayCast2D>("RayCast");
        body = this;

        _player = PlayerVariables.player;

        speed = 250f;
        _animationPlayer.PlaybackSpeed = speed / 100;

        stats = new EntityStats
        {
            physicalAttack = 75,
            magicalAttack = 80,
            physicalDefence = 65,
            magicalDefense = 75,
            health = 70,
            mana = 99999,
            stamina = 99999,
            agility = 60
        };

        Stamina += 99999;
        Mana += 99999;

        retreatDelay = new Timer();
        retreatDelay.WaitTime = 0.3f;
        retreatDelay.OneShot = true;
        AddChild(retreatDelay);

        moveDelay = new Timer();
        moveDelay.WaitTime = 0.3f;
        moveDelay.OneShot = true;
        AddChild(moveDelay);

        enemySwitchDelay = new Timer();
        enemySwitchDelay.WaitTime = 0.5f;
        enemySwitchDelay.Connect("timeout", this, "_setEnemy");
        AddChild(enemySwitchDelay);
        enemySwitchDelay.Start();

        addAbility("res://assets/objects/abilities/Flamethrower.tscn", 0);
        addAbility("res://assets/objects/abilities/fireball.tscn", 1);
        addAbility("res://assets/objects/abilities/firewave.tscn", 2);
    }
    public override void _moveProcess()
    {
        base._moveProcess();
        Vector2 vectorPath = Vector2.Zero;
        float pathRotation = 0;
        if (currentEnemy != null)
        {
            _rayCast.CastTo = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition) + MathF.PI) * 200;
            _navigation.SetTargetLocation(currentEnemy.GlobalPosition);
            vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextLocation());
            _navigation.SetVelocity(vectorPath);
            pathRotation = body.GlobalTransform.origin.AngleToPoint(_navigation.GetNextLocation());
            float enemyRotation = body.GlobalTransform.origin.AngleToPoint(currentEnemy.GlobalPosition);
            weapon.Rotation = pathRotation + MathF.PI;
            var distance = body.GlobalPosition.DistanceTo(currentEnemy.GlobalTransform.origin);
            if (distance >= 200)
            {
                if (moveDelay.TimeLeft == 0) moveDelay.Start();
            }
            vectorPath = moveDelay.TimeLeft > 0 ? vectorPath : Vector2.Zero;
            if (retreatDelay.TimeLeft > 0)
            {
                vectorPath = Vector2.Right.Rotated(enemyRotation);
            }
            if (distance <= 150)
            {
                if (retreatDelay.TimeLeft == 0) retreatDelay.Start();
            }
            if (_rayCast.IsColliding())
            {
                var obj = _rayCast.GetCollider();
                if (obj is Entity e && e.IsInGroup("enemy") && !e._deathState)
                {
                    var rotation = body.GlobalPosition.AngleToPoint(currentEnemy.GlobalTransform.origin);
                    weapon.Rotation = rotation + MathF.PI;
                    for (int i = 0; i < _abiltiesReloadTimes.Length; i++)
                    {
                        if (_abiltiesReloadTimes[i] <= 0)
                        {
                            _useAbility(i);
                            break;
                        }
                    }
                }
            }
            velocity = vectorPath;
            body.MoveAndSlide(velocity.Normalized() * speed);
            _SetAnimation((Vector2.Right.Rotated(pathRotation + Mathf.Pi) * 2).Round(), vectorPath.Length() == 0 ? true : false);
        }
        else
        {
            _defaultMotion();
        }
    }

    private void _setEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("enemy");
        Entity enemy = null;
        List<object> NonTypedEnemies = new List<object>();
        for (int i = 0; i < enemies.Count; i++)
        {
            NonTypedEnemies.Add(enemies[i]);
        }
        var sortedEnemies = NonTypedEnemies.OrderBy(x => x as Entity != null ? body.GlobalPosition.DistanceTo((x as Entity).GlobalPosition) : 0).ToList();
        sortedEnemies.RemoveAll(x => x as Entity != null ? (x as Entity)._deathState : false);
        if (sortedEnemies.ToList().Count > 0) enemy = sortedEnemies[0] as Entity;
        currentEnemy = enemy;
    }

    private void _defaultMotion()
    {
        Vector2 vectorPath = Vector2.Zero;
        _rayCast.CastTo = Vector2.Zero;
        _navigation.SetTargetLocation(_player.body.GlobalPosition);
        vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextLocation());
        _navigation.SetVelocity(vectorPath);
        float pathRotation = body.GlobalTransform.origin.AngleToPoint(_navigation.GetNextLocation());
        weapon.Rotation = pathRotation + MathF.PI;
        var distance = body.GlobalPosition.DistanceTo(_player.body.GlobalTransform.origin);
        // vectorPath = distance >= 100 ? vectorPath : Vector2.Zero;
        if (distance >= 150)
        {
            if (moveDelay.TimeLeft == 0) moveDelay.Start();
        }
        vectorPath = moveDelay.TimeLeft > 0 ? vectorPath : Vector2.Zero;
        velocity = vectorPath;
        body.MoveAndSlide(velocity.Normalized() * speed);
        _SetAnimation((Vector2.Right.Rotated(pathRotation + Mathf.Pi) * 2).Round(), vectorPath.Length() == 0 ? true : false);
    }
}
