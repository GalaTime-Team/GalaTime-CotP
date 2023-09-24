using Godot;
using System;
using Galatime;
using Godot.Collections;

namespace Galatime.Damage;

/// <summary> The type of the explosion. Normal or Red (Can't be blocked and stun). </summary>
public enum ExplosionType {
    Normal,
    Red
}

/// <summary> The explosion node. </summary>s
public partial class Explosion : Area2D
{   
    #region Nodes
    // <summary> The explosion visual. </summary>
    public GpuParticles2D Particles;
    // <summary> The explosion audio. </summary>
    public AudioStreamPlayer2D AudioStreamPlayer;
    public CollisionShape2D Collision;
    #endregion

    #region Variables
    /// <summary> The power of the explosion which determines the damage, knockback and radius. </summary>
    public int Power = 3;

    /// <summary> The element of the explosion. </summary>
    public GalatimeElement Element = new();

    /// <summary> The type of the explosion. Normal or Red (Can't be blocked and stun). </summary>
    public ExplosionType Type = ExplosionType.Normal;

    /// <summary> The audios of the explosion of types. </summary>
    public Dictionary<ExplosionType, AudioStream> ExplosionAudios = new();

    /// <summary> The textures of the explosion of types. </summary>
    public Dictionary<ExplosionType, Texture2D> ExplosionTextures = new();
    #endregion

    public override void _Ready() {
        #region Get nodes
        Particles = GetNode<GpuParticles2D>("Particles");
        AudioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer");
        Collision = GetNode<CollisionShape2D>("Collision");
        #endregion

        ExplosionAudios[ExplosionType.Normal] = ResourceLoader.Load<AudioStream>("res://assets/audios/sounds/damage/explosion_medium.wav");
        ExplosionAudios[ExplosionType.Red] = ResourceLoader.Load<AudioStream>("res://assets/audios/sounds/damage/explosion_hard.wav");
        ExplosionTextures[ExplosionType.Normal] = ResourceLoader.Load<Texture2D>("res://sprites/damage/explosion.png");
        ExplosionTextures[ExplosionType.Red] = ResourceLoader.Load<Texture2D>("res://sprites/damage/explosion_red.png");

        BodyEntered += OnBodyEntered;

        Explode();  
    }

    private void OnBodyEntered(Node node) {
        // Deal damage to the entity.
        if (node is Entity e) {
            float damageRotation = GlobalPosition.AngleToPoint(e.GlobalPosition);
            e.TakeDamage(7 * Power, 150, Element, DamageType.magical, 100 * Power, damageRotation);
        }
    }

    public async void Explode() {
        // Disable the explosion in this time (Depends on the power).
        var disableOn = Power * .1f;
        var particleSize = Power * 1.33f;

        var processMaterial = Particles.ProcessMaterial as ParticleProcessMaterial;
        processMaterial.ScaleMax = particleSize;
        processMaterial.ScaleMin = particleSize;

        var collisionShape = Collision.Shape as CircleShape2D;
        collisionShape.Radius = Power * 2.22f;
        Collision.Shape = collisionShape;

        Particles.ProcessMaterial = processMaterial;
        Particles.Texture = ExplosionTextures[Type];
        AudioStreamPlayer.Stream = ExplosionAudios[Type];
        
        // Apply to the particles as well.
        Particles.Lifetime = Power * .11f;

        // Enable particles.
        Particles.Emitting = true;
        AudioStreamPlayer.Play();

        await ToSignal(GetTree().CreateTimer(disableOn), "timeout");
        Monitoring = false;
    }
}
