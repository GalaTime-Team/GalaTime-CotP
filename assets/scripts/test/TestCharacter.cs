using Galatime;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TestCharacter : HumanoidCharacter
{
    [Export] private int followOrder = 0;

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
        AnimationPlayer = GetNode<AnimationPlayer>("Animation");
        Sprite = GetNode<Sprite2D>("Sprite2D");
        TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
        Weapon = GetNode<Hand>("Hand");

        _navigation = GetNode<NavigationAgent2D>("NavigationAgent3D");
        _rayCast = GetNode<RayCast2D>("RayCast3D");
        Body = this;

        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        _player = playerVariables.Player;

        Speed = 300f;
        AnimationPlayer.SpeedScale = Speed / 100;

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
        enemySwitchDelay.Timeout += () => _setEnemy();
        AddChild(enemySwitchDelay);
        enemySwitchDelay.Start();

        addAbility(GalatimeGlobals.getAbilityById("flamethrower"), 0);
        addAbility(GalatimeGlobals.getAbilityById("fireball"), 1);
        addAbility(GalatimeGlobals.getAbilityById("firewave"), 2);
    }
    public override void _MoveProcess()
    {
        base._MoveProcess();
        Vector2 vectorPath = Vector2.Zero;
        float pathRotation = 0;
        if (currentEnemy != null)
        {
            if (Weapon.Item == null) Weapon.TakeItem(GalatimeGlobals.getItemById("golden_holder_sword"));
            _rayCast.TargetPosition = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition)) * 200;
            _navigation.TargetPosition = currentEnemy.GlobalPosition;
            vectorPath = Body.GlobalPosition.DirectionTo(_navigation.GetNextPathPosition());
            pathRotation = Body.GlobalPosition.AngleToPoint(_navigation.GetNextPathPosition());
            float enemyRotation = Body.GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition);
            Weapon.Rotation = pathRotation;
            var distance = Body.GlobalPosition.DistanceTo(currentEnemy.GlobalPosition);
            if (distance >= 200)
            {
                if (moveDelay.TimeLeft == 0) moveDelay.Start();
            }
            vectorPath = moveDelay.TimeLeft > 0 ? vectorPath : Vector2.Zero;
            if (retreatDelay.TimeLeft > 0)
            {
                vectorPath = Vector2.Right.Rotated(enemyRotation + MathF.PI);
            }
            if (distance <= 150)
            {
                if (retreatDelay.TimeLeft == 0) retreatDelay.Start();
            }
            if (_rayCast.IsColliding())
            {
                var obj = _rayCast.GetCollider();
                if (obj is Entity e && e.IsInGroup("enemy") && !e.DeathState)
                {
                    var rotation = Body.GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition);
                    Weapon.Rotation = rotation;
                    for (int i = 0; i < Abilities.Count; i++)
                    {
                        var ability = Abilities[i];
                        if (ability.IsReloaded) _useAbility(i);
                    }
                }
            }
            var swordColliders = Weapon.GetOverlappingBodies();
            if (swordColliders.Count >= 1)
            {
                var obj = (Node2D)swordColliders[0];
                if (obj is Entity e && e.IsInGroup("enemy") && !e.DeathState)
                {
                    var rotation = Body.GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition);
                    Weapon.Rotation = rotation;
                    Weapon.Attack(this);
                }
            }
            Body.Velocity = vectorPath.Normalized() * Speed;
            MoveAndSlide();
            // _SetAnimation((Vector2.Right.Rotated(pathRotation) * 2).Round(), vectorPath.Length() == 0 ? true : false);
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
        var sortedEnemies = NonTypedEnemies.OrderBy(x => x as Entity != null ? Body.GlobalPosition.DistanceTo((x as Entity).GlobalPosition) : 0).ToList();
        sortedEnemies.RemoveAll(x => x as Entity != null ? (x as Entity).DeathState : false);
        if (sortedEnemies.ToList().Count > 0) enemy = sortedEnemies[0] as Entity;
        currentEnemy = enemy;
    }

    private void _defaultMotion()
    {
        // if (weapon._item != null) weapon.takeItem(new Godot.Collections.Dictionary());

        var allies = GetTree().GetNodesInGroup("ally");
        var followTo = allies[followOrder] as CharacterBody2D;
        Vector2 vectorPath = Vector2.Zero;
        _rayCast.TargetPosition = Vector2.Zero;
        _navigation.TargetPosition = followTo.GlobalPosition;
        vectorPath = Body.GlobalPosition.DirectionTo(_navigation.GetNextPathPosition());
        float pathRotation = Body.GlobalPosition.AngleToPoint(_navigation.GetNextPathPosition());
        Weapon.Rotation = pathRotation;
        var distance = Body.GlobalPosition.DistanceTo(followTo.GlobalPosition);
        // vectorPath = distance >= 100 ? vectorPath : Vector2.Zero;
        if (distance >= 150)
        {
            if (moveDelay.TimeLeft == 0) moveDelay.Start();
        }
        vectorPath = moveDelay.TimeLeft > 0 ? vectorPath : Vector2.Zero;
        Body.Velocity = vectorPath.Normalized() * Speed;
        MoveAndSlide();
        // _SetAnimation((Vector2.Right.Rotated(pathRotation) * 2).Round(), vectorPath.Length() == 0 ? true : false);
    }
}
