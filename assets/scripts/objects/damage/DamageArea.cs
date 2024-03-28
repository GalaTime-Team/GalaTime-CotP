using Galatime.Helpers;
using Godot;
using System;

namespace Galatime.Damage;

/// <summary> Types of collisions that fell under the damage zone. </summary>
public enum BodyType
{
    /// <summary> Entity collided with. </summary>
    Entity,
    /// <summary> Projectile collided with. </summary>
    Projectile,
    /// <summary> Solid collided with or none. </summary>
    Solid
}

[Tool]
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
    /// <summary> How often the damage area deals damage. Damage Per Second. 0 means no damage. </summary>
    [Export] public float DPS = 0;
    private double DamageTimer = 0;
    #endregion

    #region Events
    public Action<Node2D, BodyType> Hit;
    #endregion

    public override void _Ready()
    {
        // Attach event to the area to track entities.
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (Active && DPS > 0)
        {
            // If the damage area is active, then it deals damage every second.
            DamageTimer += delta;
            if (DamageTimer >= DPS)
            {
                HitOneTime();
                DamageTimer = 0;
            }
        }
    }

    private void OnBodyEntered(Node body)
    {
        // Deal damage to the entity if someone entered the area.
        if (Active) DealDamage(body);
    }

    /// <summary> Deals damage to all entities in the area. Don't be confused with <see cref="Active"/>, because it doesn't affect <see cref="HitOneTime"/>, but deals damage over time. </summary>
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
