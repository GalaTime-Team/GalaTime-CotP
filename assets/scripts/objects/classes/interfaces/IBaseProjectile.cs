using Godot;

namespace Galatime.Damage;

/// <summary> Interface for base projectiles. They innactive if they are not launched. </summary>
public interface IBaseProjectile
{
    public Projectile Projectile { get; set; }
    public void Launch(float direction, float attackStat);
}