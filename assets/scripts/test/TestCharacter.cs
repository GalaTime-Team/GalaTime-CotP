    using Godot;
    using System;
    using Galatime;
    using System.Linq;
    using System.Collections.Generic;

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
            _animationPlayer = GetNode<AnimationPlayer>("Animation");
            _sprite = GetNode<Sprite2D>("Sprite2D");
            _trailParticles = GetNode<GpuParticles2D>("TrailParticles");
            weapon = GetNode<Hand>("Hand");

            _navigation = GetNode<NavigationAgent2D>("NavigationAgent3D");
            _rayCast = GetNode<RayCast2D>("RayCast3D");
            body = this;

            _player = PlayerVariables.Player;

            speed = 300f;
            _animationPlayer.SpeedScale = speed / 100;

            Stats = new EntityStats(
                physicalAttack: 75,
                magicalAttack: 80,
                physicalDefence: 65,
                magicalDefence: 75,
                health: 70,
                mana: 99999,
                stamina: 99999,
                agility: 60
            );

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
                if (weapon._item == null) weapon.takeItem(GalatimeGlobals.getItemById("golden_holder_sword")); 
                _rayCast.TargetPosition = Vector2.Right.Rotated(GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition)) * 200;
                _navigation.TargetPosition = currentEnemy.GlobalPosition;
                vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextPathPosition());
                _navigation.SetVelocity(vectorPath);
                pathRotation = body.GlobalPosition.AngleToPoint(_navigation.GetNextPathPosition());
                float enemyRotation = body.GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition);
                weapon.Rotation = pathRotation;
                var distance = body.GlobalPosition.DistanceTo(currentEnemy.GlobalPosition);
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
                        var rotation = body.GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition);
                        weapon.Rotation = rotation;
                        for (int i = 0; i < _abiltiesReloadTimes.Length; i++)
                        {
                            if (_abiltiesReloadTimes[i] <= 0) _useAbility(i);
                        }
                    }
                }   
                var swordColliders = weapon.GetOverlappingBodies();
                if (swordColliders.Count >= 1)
                {
                    var obj = (Node2D)swordColliders[0];
                    if (obj is Entity e && e.IsInGroup("enemy") && !e.DeathState)
                    {
                        var rotation = body.GlobalPosition.AngleToPoint(currentEnemy.GlobalPosition);
                        weapon.Rotation = rotation;
                        weapon.attack(Stats[EntityStatType.physicalAttack].Value, Stats[EntityStatType.magicalAttack].Value);
                    }
                }
                velocity = vectorPath;
                body.Velocity = velocity.Normalized() * speed;
                MoveAndSlide();
                _SetAnimation((Vector2.Right.Rotated(pathRotation) * 2).Round(), vectorPath.Length() == 0 ? true : false);
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
            sortedEnemies.RemoveAll(x => x as Entity != null ? (x as Entity).DeathState : false);
            if (sortedEnemies.ToList().Count > 0) enemy = sortedEnemies[0] as Entity;
            currentEnemy = enemy;
        }

        private void _defaultMotion()
        {
            if (weapon._item != null) weapon.takeItem(new Godot.Collections.Dictionary());    

            var allies = GetTree().GetNodesInGroup("ally");
            var followTo = allies[followOrder] as CharacterBody2D;
            Vector2 vectorPath = Vector2.Zero;
            _rayCast.TargetPosition = Vector2.Zero;
            _navigation.TargetPosition = followTo.GlobalPosition;
            vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextPathPosition());
            _navigation.SetVelocity(vectorPath);
            float pathRotation = body.GlobalPosition.AngleToPoint(_navigation.GetNextPathPosition());
            weapon.Rotation = pathRotation;
            var distance = body.GlobalPosition.DistanceTo(followTo.GlobalPosition);
            // vectorPath = distance >= 100 ? vectorPath : Vector2.Zero;
            if (distance >= 150)
            {
                if (moveDelay.TimeLeft == 0) moveDelay.Start();
            }
            vectorPath = moveDelay.TimeLeft > 0 ? vectorPath : Vector2.Zero;
            velocity = vectorPath;
            body.Velocity = velocity.Normalized() * speed;
            MoveAndSlide();
            _SetAnimation((Vector2.Right.Rotated(pathRotation) * 2).Round(), vectorPath.Length() == 0 ? true : false);
        }
    }
