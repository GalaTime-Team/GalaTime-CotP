using System;
using Godot;

namespace ExtensionMethods;

public static class Sprite2DExtensionMethods
{
    /// <summary> Flips the sprite horizontally if sprite should be flipped based on the angle. </summary>
    public static bool FlipSpriteByAngle(this Sprite2D sprite, float value) => sprite.FlipH = Math.Abs(value) < Math.PI / 2;
}