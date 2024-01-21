using Godot;
using System;
using Godot.Collections;
using Galatime.Global;

namespace Galatime.Damage;

/// <summary> The type of the explosion. Normal or Red (Can't be blocked and stun). </summary>
public enum ExplosionType
{
    Normal,
    Red
}

/// <summary> The explosion node. </summary>s
public partial class Explosion : Area2D
{
    #region Nodes
    // <summary> The explosion particles visual nodes. </summary>
    public GpuParticles2D Particles, WhiteParticles, HoleParticles, SmokeParticles, TrailParticles;
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

    public override void _Ready()
    {
        #region Get nodes
        Particles = GetNode<GpuParticles2D>("Particles");
        WhiteParticles = GetNode<GpuParticles2D>("WhiteParticles");
        HoleParticles = GetNode<GpuParticles2D>("HoleParticles");
        SmokeParticles = GetNode<GpuParticles2D>("SmokeParticles");
        TrailParticles = GetNode<GpuParticles2D>("TrailParticles");
        AudioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer");
        Collision = GetNode<CollisionShape2D>("Collision");
        #endregion

        ExplosionAudios[ExplosionType.Normal] = ResourceLoader.Load<AudioStream>("res://assets/audios/sounds/damage/explosion_medium.wav");
        ExplosionAudios[ExplosionType.Red] = ResourceLoader.Load<AudioStream>("res://assets/audios/sounds/damage/explosion_hard.wav");
        ExplosionTextures[ExplosionType.Normal] = ResourceLoader.Load<Texture2D>("res://sprites/damage/explosion_old.png");
        ExplosionTextures[ExplosionType.Red] = ResourceLoader.Load<Texture2D>("res://sprites/damage/explosion_red.png");

        BodyEntered += OnBodyEntered;

        Explode();
    }

    public static Explosion GetInstance() => GD.Load<PackedScene>(FilePathsConstants.ExplosionPath).Instantiate<Explosion>();

    private void OnBodyEntered(Node node)
    {
        // Deal damage to the entity.
        if (node is Entity e)
        {
            float damageRotation = GlobalPosition.AngleToPoint(e.GlobalPosition);
            e.TakeDamage(3 * Power, 150, Element, DamageType.Magical, 100 * Power, damageRotation);
        }
    }

    public async void Explode()
    {
        // Disable the explosion in this time (Depends on the power).
        var disableOn = Power * .05f;
        var particleSize = Power * 1.33f;
        var particleSizeDifference = (int)(particleSize * 0.2f);

        var rnd = new Random().Next(-particleSizeDifference, particleSizeDifference);
        particleSize += rnd;

        SetProcessMaterialSize(Particles, particleSize);
        SetProcessMaterialSize(HoleParticles, particleSize);
        SetProcessMaterialSize(WhiteParticles, particleSize * 1.25f);
        SetProcessMaterialEmissionShapeRadius(SmokeParticles, particleSize * 2);

        SmokeParticles.Amount = Power * 2;

        var collisionShape = Collision.Shape.Duplicate() as CircleShape2D;
        collisionShape.Radius = Power * 2.22f;
        Collision.Shape = collisionShape;

        Particles.Texture = ExplosionTextures[Type];
        AudioStreamPlayer.Stream = ExplosionAudios[Type];

        // Apply to the particles as well.
        Particles.Lifetime = Power * .06f;
        WhiteParticles.Lifetime = Particles.Lifetime / 2;
        HoleParticles.Lifetime = Particles.Lifetime * 3;

        TrailParticles.Amount = Power * 2;
        TrailParticles.Lifetime = Particles.Lifetime / 2;

        // Enable particles.
        Particles.Emitting = true;
        WhiteParticles.Emitting = true;
        HoleParticles.Emitting = true;
        TrailParticles.Emitting = true;
        AudioStreamPlayer.Play();

        PlayerVariables.Instance.Player.CameraShakeAmount = Power * 2;

        GetTree().CreateTimer(disableOn, false).Timeout += () =>
        {
            SmokeParticles.Emitting = true;
            Monitoring = false;
            GetTree().CreateTimer(disableOn * 2.33, false).Timeout += () =>
            {
                SmokeParticles.Emitting = false;
                GetTree().CreateTimer(3, false).Timeout += QueueFree;
            };
        };
    }

    /// <summary> Sets the process material size of the particles. This method duplicates the process material. </summary>
    /// <param name="particles">The particles node to set the size.</param>
    /// <param name="particleSize">The size of the particles.</param>
    private void SetProcessMaterialSize(GpuParticles2D particles, float particleSize)
    {
        var material = particles.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        material.ScaleMax = particleSize;
        material.ScaleMin = particleSize;
        particles.ProcessMaterial = material;
    }

    private void SetProcessMaterialEmissionShapeRadius(GpuParticles2D particles, float radius)
    {
        var material = particles.ProcessMaterial.Duplicate() as ParticleProcessMaterial;
        material.EmissionSphereRadius = radius;
        particles.ProcessMaterial = material;
    }
}