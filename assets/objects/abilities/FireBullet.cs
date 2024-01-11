using Galatime;
using Galatime.Damage;
using Godot;

public partial class FireBullet : GalatimeAbility
{
    #region Nodes
    public AnimationPlayer AnimationPlayer;
    public Node2D Projectile;
    public Line2D Line;
    public Area2D DamageArea;
    public HumanoidCharacter Caster;
    public RayCast2D RayCast;
    public CollisionShape2D DamageAreaCollisionShape;

    public PackedScene ExplosionScene;
    public Explosion Explosion;

    #endregion

    public bool Shotted = false;
    public PlayerVariables PlayerVariables;

    public override void _Ready()
    {
        #region Get nodes
        DamageArea = GetNode<Area2D>("Projectile/DamageArea");
        DamageAreaCollisionShape = GetNode<CollisionShape2D>("Projectile/DamageArea/Collision");
        Projectile = GetNode<Node2D>("Projectile");
        Line = GetNode<Line2D>("Projectile/Line");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        RayCast = GetNode<RayCast2D>("Projectile/RayCast");
        #endregion

        // Duplicate the shape, so it don't get shared with other objects.
        DamageAreaCollisionShape.Shape = DamageAreaCollisionShape.Shape.Duplicate() as Shape2D;

        ExplosionScene = ResourceLoader.Load<PackedScene>("res://assets/objects/damage/Explosion.tscn");
        Explosion = ExplosionScene.Instantiate<Explosion>();
        Explosion.Power = 6;
        Explosion.Type = ExplosionType.Red;
        Explosion.Element = GalatimeElement.Ignis;

        PlayerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
    }

    public override void _Process(double delta)
    {
        if (RayCast.IsColliding())
        {
            var position = RayCast.GetCollisionPoint(); // Get the position of the collision point in global coordinates.
            var lineLength = RayCast.GlobalPosition.DistanceTo(position) + 64;
            var collisionLength = RayCast.GlobalPosition.DistanceTo(position) + 16;
            Cut(lineLength, collisionLength);
        }
        else
        {
            var lineLength = 528;
            var collisionLength = 448;
            Cut(lineLength, collisionLength);
        }
        if (!Shotted)
        {
            PlayerVariables.Player.CameraShakeAmount += 0.2f;

            Projectile.GlobalPosition = Caster.Weapon.GlobalPosition;
            Projectile.Rotation = Caster.Weapon.Rotation;
        }
    }

    public void Cut(float lineLength = 0, float collisionLength = 0) 
    {
        Line.SetPointPosition(1, new Vector2(lineLength, 0));
        var shape = DamageAreaCollisionShape.Shape as RectangleShape2D;
        var shapeSize = shape.Size;
        shapeSize.X = collisionLength;
        DamageAreaCollisionShape.Position = new(shapeSize.X / 2, 0);
        shape.Size = shapeSize;
        DamageAreaCollisionShape.Shape = shape;
    }

    public override void Execute(HumanoidCharacter p)
    {
        Caster = p;
        AnimationPlayer.Play("shoot");
    }

    public void DealDamage()
    {
        Shotted = true;

        var bodies = DamageArea.GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is Entity entity)
                entity.TakeDamage(65, Caster.Stats[EntityStatType.MagicalAttack].Value, Data.Element, DamageType.Magical);
            if (body is Projectile projectile && projectile.Moving && RayCast.IsColliding())
            {
                PlayerVariables.Player.PlayerGui.ParryEffect(projectile.GlobalPosition);
                var explosion = projectile.Explosion;
                explosion.Type = ExplosionType.Red;
                explosion.Power += 6;
                projectile.Destroy();  
                return;
            }
        }
        if (RayCast.IsColliding())
        {
            var position = RayCast.GetCollisionPoint(); // Get the position of the collision point in global coordinates.
            GetParent().AddChild(Explosion);
            Explosion.GlobalPosition = position;
        }
    }
}   
