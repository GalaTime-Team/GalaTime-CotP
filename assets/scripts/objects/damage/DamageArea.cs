using Galatime.Helpers;
using Godot;
using System;

namespace Galatime.Damage;

public enum BodyType
{
    Entity,
    Projectile,
    Solid
}

/// <summary> Represents a damage area, which deals damage to entities and can knockback. </summary>
public partial class DamageArea : Area2D
{
    #region Variables
    /// <summary> The element of the damage area. </summary>
    [Export] public GalatimeElement Element = new();
    /// <summary> The type of damage of the damage area. </summary>
    [Export] public DamageType Type = DamageType.Physical;
    [Export] public Teams TargetTeam = Teams.Allies;
    /// <summary> The attack stat (can be magical or physical) of the damage area. </summary>
    public float AttackStat = 0;
    /// <summary> The power of the damage area. </summary>
    [Export] public float Power = 0;
    /// <summary> The knockback of the damage area. </summary>
    [Export] public float Knockback = 0;

    /// <summary> If the damage area is active or not. Doesn't deal damage if inactive. This doesn't affect the <see cref="HitOneTime"/>. </summary>
    [Export] public bool Active;
    #endregion

    #region Events
    public Action<Node2D, BodyType> Hit;
    #endregion

    public override void _Ready()
    {
        // Attach event to the area to track entities.
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        // Deal damage to the entity if someone entered the area.
        if (Active) DealDamage(body);
    }

    public void HitOneTime()
    {
        var bodies = GetOverlappingBodies();
        // Interate over the bodies and deal damage to all of them.
        foreach (var body in bodies) DealDamage(body);
    }

    public void DealDamage(Node body)
    {
        // Checking if the body is an entity and not a dead body (Just to be sure).
        if (body is Entity e && e.Team == TargetTeam)
        {
            // Calculating the rotation to the damaged entity to push back.
            var damageRotation = GlobalPosition.AngleToPoint(e.GlobalPosition);

            // Deal damage to the entity based on the properties and damage rotation.
            e.TakeDamage(Power, AttackStat, Element, Type, Knockback, damageRotation);

            // Fire the Hit event.
            Hit?.Invoke(e, BodyType.Entity);
        }
        if (body is Projectile p && p.Moving)
        {
            Hit?.Invoke(p, BodyType.Projectile);
            return;
        }
        Hit?.Invoke(body as Node2D, BodyType.Solid);
    }
}
