using Godot;

namespace Galatime.Global;

/// <summary> Represents a collection of constants used in the game. </summary>
public static class GalatimeConstants
{
    /// <summary> The version of the game. </summary>
    public const string Version = "0.11.0";
    /// <summary> The short summary of the version. </summary>
    public const string VersionDescription = "Beta\nPREVIEW BUILD";

    /// <summary> The path to the saves folder. </summary>
    public const string SavesPath = "user://saves/";
    /// <summary> The path to the settings file. </summary>
    public const string SettingsPath = "user://settings.yml";
    /// <summary> The path to the assets file. </summary>
    public const string AssetsPath = "res://assets/data/json/assets.csv";
    /// <summary> The path to the levels file. </summary>
    public const string LevelsDataPath = "res://assets/data/json/levels.csv";

    /// <summary> The Discord application ID, which is used for connecting to the Discord rich presence. </summary>
    public const string DISCORD_ID = "1071756821158699068";
}

/// <summary> A collection of colors used in the game. </summary>
public static class GameColors 
{
    public readonly static Color Red = new(1f, 0f, 0f);
    public readonly static Color Green = new(0f, 1f, 0f);
    public readonly static Color Blue = new(0f, 0f, 1f);

    public readonly static Color Yellow = new(1f, 1f, 0f);

    public readonly static Color Gray = new(0.5f, 0.5f, 0.5f);

    public readonly static Color DarkGray = new(0.25f, 0.25f, 0.25f);

    public readonly static Color White = new(1f, 1f, 1f);

    public readonly static Color Black = new(0f, 0f, 0f);
}


public static class FilePathsConstants
{
    public const string ExplosionPath = "res://assets/objects/damage/Explosion.tscn";
}