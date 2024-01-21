using System;
using Godot;

namespace Galatime.Helpers;

/// <summary> Class, which contains a vector to shake. </summary>
public class VectorShaker
{
    /// <summary> The duration of the shake. </summary>
    public float ShakeDuration = 1f;
    /// <summary> The amplitude of the shake. For example if X set to 2 and Y set to 1, the shake will be twice as big. </summary>
    public Vector2 ShakeAmplitude = new(1f, 1f);
    /// <summary> If the shake should run infinitely. </summary>
    public bool Infinite = false;
    /// <summary> The initial position, used to make shake pinned to the initial position. </summary>
    public Vector2 InitialVector { get; private set; }
    public bool Enabled = false;
    /// <summary> The vector that was shaken. </summary>
    public Vector2 ShakenVector { get; private set; }
    /// <summary> The offset from the initial position. </summary>
    public Vector2 ShakenOffset => ShakenVector - InitialVector;

    private double Time = 0;

    public void ShakeStart(Vector2 initialPosition) 
    {
        InitialVector = initialPosition;
        Time = 0;
        Enabled = true;
    }

    public void ShakeStop() => Enabled = false;
    
    public void ShakeProcess(double delta)
    {
        if (!Enabled) return;
        if (!Infinite) Time += delta;
        if (Time > ShakeDuration && !Infinite) return;

        var rnd = new Random();
        if (ShakeDuration <= 0 || Infinite)
        {
            ShakenVector = ShakenVector with
            {
                X = InitialVector.X + (float)rnd.NextDouble() * ShakeAmplitude.X,
                Y = InitialVector.Y + (float)rnd.NextDouble() * ShakeAmplitude.Y
            };
        }
    }
}