using Galatime;
using Galatime.Damage;
using Galatime.Helpers;
using Godot;
using System;

public partial class Firecloak : Entity
{
    #region Nodes
    /// <summary> Base Fireball, which similar to player's fireball. </summary>
    public BaseFireball BaseFireball;
    public Timer FireballSpawnTimer;
    public TargetController TargetController;
    #endregion

    public override void _Ready()
    {
        base._Ready();

        #region Get nodes
        BaseFireball = GetNode<BaseFireball>("BaseFireball");
        TargetController = GetNode<TargetController>("TargetController");
        FireballSpawnTimer = GetNode<Timer>("FireballSpawnTimer");
        #endregion

        FireballSpawnTimer.Timeout += SpawnFireball;

        FireballSpawnTimer.Start();
    }

    public void SpawnFireball()
    {
        var magicalAttack = Stats[EntityStatType.MagicalAttack].Value;
        var prj = BaseFireball.Duplicate() as BaseFireball;
        prj.GlobalPosition = GlobalPosition;
        GetParent().AddChild(prj);
        prj.Launch(TargetController.CurrentTarget.GlobalPosition.AngleToPoint(GlobalPosition) + Mathf.Pi, magicalAttack);

        PlayerVariables.Instance.Player.CameraShakeAmount += 10;
    }

    public override void _MoveProcess(double delta)
    {
        Velocity = Vector2.Zero;
    }

    public override void _DeathEvent(float damageRotation = 0)
    {
        Visible = false;
        FireballSpawnTimer.Stop();

        base._DeathEvent(damageRotation);
    }

    public override void _AIProcess(double delta)
    {
        base._AIProcess(delta);
    }
}
