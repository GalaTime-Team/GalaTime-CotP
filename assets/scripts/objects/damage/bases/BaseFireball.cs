using Godot;

using Galatime.Global;

namespace Galatime.Damage;

public partial class BaseFireball : Node2D, IBaseProjectile
{
    public Projectile Projectile { get; set; }
    public Projectile Proj { get; set; }
    public AnimationPlayer AnimationPlayer;

    public override void _Ready()
    {
        Proj = GetNode<Projectile>("Projectile");
        AnimationPlayer = Proj.GetNode<AnimationPlayer>("AnimationPlayer");

        Proj.Exploded += (Projectile projectile) => AnimationPlayer.Play("outro");
    }

    public void Launch(float direction, float attackStat)
    {
        Proj.Rotation = direction;
        Proj.Visible = true;
        Proj.Moving = true;
        Proj.AttackStat = attackStat;
        Proj.Timeout = 10f;
        AnimationPlayer.Play("intro");

        PlayerVariables.Instance.Player.CameraShakeAmount += 5f;
    }
}

