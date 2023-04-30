using Godot;
using System;
using Galatime;
using System.Collections.Generic;
using Godot.Collections;
using System.Linq;

public partial class Slime : Entity
{
    private Player _player = null;
    private Vector2[] _path = null;
    private NavigationAgent2D _navigation = null;

    private Sprite2D _sprite;
    private Area2D _weapon;
    private AnimationPlayer _animationPlayer;
    private CollisionShape2D _collision;

    private Timer _attackCountdownTimer;

    private PackedScene slimeScene;

    private bool _canMoveTest = true;
    public new float speed = 200;
    [Export] public float r = 1;
    public int stage = 2;

    public override async void _Ready()
    {
        base._Ready();

        element = GalatimeElement.Aqua;
        body = this;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        slimeScene = ResourceLoader.Load<PackedScene>("res://assets/objects/enemy/slime.tscn");

        _player = PlayerVariables.Player;
        Stats = new EntityStats(
                physicalAttack: 21,
                magicalAttack: 25,
                physicalDefence: 10,
                magicalDefence: 34,
                health: 5
            );

        droppedXp = 10;

        _sprite = GetNode<Sprite2D>("Sprite2D");
        _navigation = GetNode<NavigationAgent2D>("NavigationAgent3D");
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");

        _weapon = GetNode<Area2D>("Weapon");

        _weapon.Connect("body_entered", new Callable(this, "_attack"));
        _weapon.Connect("body_exited", new Callable(this, "_onAreaExit"));

        _attackCountdownTimer = new Timer();
        _attackCountdownTimer.WaitTime = 1f;
        _attackCountdownTimer.OneShot = true;
        _attackCountdownTimer.Connect("timeout", new Callable(this, "justHit"));
        AddChild(_attackCountdownTimer);

        _animationPlayer.Play("intro");

        Stats[EntityStatType.health].Value += 10 * stage;
        Health = Stats[EntityStatType.health].Value;

        speed -= 50 * stage;

        var scale = Vector2.One;
        scale.X += Math.Max(1.5f + stage + 0.5f, 4);
        scale.Y += Math.Max(1.5f + stage + 0.5f, 4);   
        _sprite.Scale = scale;

        if (stage == 1)
        {
            var smallSlimeTexture = GD.Load<Texture2D>("res://sprites/small_slime.png");
            _sprite.Texture = smallSlimeTexture;
        }

        dynamic barrelItem = new
        {
            id = "barrel",
            max = 7,
            min = 1,
            chance = 75
        };

        lootPool.Add(barrelItem);

        await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);

        _collision.Disabled = false;
    }

    public override void _moveProcess()
    {
        if (!DeathState) move(); else velocity = Vector2.Zero;
    }

    public override void _deathEvent(float damageRotation = 0f)
    {
        base._deathEvent();

        _canMoveTest = false;
        _animationPlayer.Play("outro");

        if (stage >= 2)
        {
            for (int i = 0; i < 2; i++)
            {
                var slimeInstance = slimeScene.Instantiate<Slime>();
                slimeInstance.stage = stage - 1;
                var position = GlobalPosition;
                position.Y -= i == 0 ? 40 : -40;
                slimeInstance.GlobalPosition = position;
                GetParent().AddChild(slimeInstance);
                slimeInstance.setKnockback(500, i == 0 ? damageRotation - Mathf.DegToRad(45) : damageRotation + Mathf.DegToRad(45));
            }
        }
        else
        {
            _dropXp();
            _dropLoot(damageRotation);
        }
    }

    public async void _attack(CharacterBody2D body)
    {
        if (!DeathState)
        {
            if (body is Entity entity)
            {
                _attackCountdownTimer.Start();
                GalatimeElement element = GalatimeElement.Aqua;
                float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
                entity.hit(20 + 10 * stage, Stats[EntityStatType.magicalAttack].Value, element, DamageType.physical, 500, damageRotation);
            }
        }
    }

    public void justHit()
    {
        var bodies = _weapon.GetOverlappingBodies()[0] as CharacterBody2D;
        if (bodies is Entity entity)
        {
            _attackCountdownTimer.Start();
            GalatimeElement element = GalatimeElement.Aqua;
            float damageRotation = GlobalPosition.AngleToPoint(entity.GlobalPosition);
            entity.hit(20 + 10 * stage, Stats[EntityStatType.magicalAttack].Value, element, DamageType.physical, 500, damageRotation);
        }
    }

    public void _onAreaExit(CharacterBody2D body)
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
        sortedEnemies.RemoveAll(x => x as Entity != null ? (x as Entity).DeathState : false);
        if (sortedEnemies.ToList().Count > 0) enemy = sortedEnemies[0] as Entity;
        if (enemy != null)
        {
            try
            {
                Vector2 vectorPath = Vector2.Zero;
                _navigation.TargetPosition = enemy.GlobalPosition;
                vectorPath = body.GlobalPosition.DirectionTo(_navigation.GetNextPathPosition()) * speed;
                _navigation.SetVelocity(vectorPath);
                float rotation = body.GlobalPosition.AngleToPoint(enemy.GlobalPosition);
                _weapon.Rotation = rotation;
                float rotationDeg = Mathf.RadToDeg(rotation);
                float rotationDegPositive = rotationDeg * 1 > 0 ? rotationDeg : -rotationDeg;
                if (rotationDegPositive <= 90) _sprite.FlipH = true; else _sprite.FlipH = false;
                velocity = vectorPath;
            }
            catch (Exception err)
            {
                GD.PrintErr("CAN'T MOVE");
            }
        }
        else
        {
            velocity = Vector2.Zero;
        }
    }
} 