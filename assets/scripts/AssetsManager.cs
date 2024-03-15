using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace Galatime.Global;

/// <summary> A singleton that manages all the assets in the game. </summary>
public partial class AssetsManager : Node
{
    GameLogger Logger { get; } = new(nameof(AssetsManager), GameLogger.ConsoleColor.Green);

    /// <summary> Path to the assets folder. </summary>
    public const string ASSETS_FOLDER_PATH = "res://assets/";

    public static AssetsManager Instance { get; private set; }

    /// <summary> Contains all the assets in the game. </summary>
    public Dictionary<string, string> Assets = new();
    /// <summary> Contains all the levels in the game. </summary>
    public Dictionary<string, string> Levels = new();

    public override void _Ready()
    {
        Instance = this;
        LoadAssets();
    }

    public void LoadAssets()
    {
        Assets = LoadAndGetAssets(GalatimeConstants.AssetsPath);
        Levels = LoadAndGetAssets(GalatimeConstants.LevelsDataPath);

        // Print all loaded assets.
        var str = new StringBuilder();
        str.Append("Assets ");
        foreach (var asset in Assets) str.Append($"\"{asset.Key}\", ");
        str.Append("is loaded!");
        Logger.Log(str.ToString(), GameLogger.LogType.Success);
    }

    /// <summary> Shortcut to get the assets from a file. </summary>
    public Dictionary<string, string> LoadAndGetAssets(string path) => CSVToDictionary(GetTextFromFile(path));

    /// <summary> Shortcut to get the text from a file. </summary>
    public string GetTextFromFile(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        return file.GetAsText();
    }

    /// <summary> Simply reads the csv formatted text and stores it in the dictionary. </summary>
    /// <remarks> It ignores header line, so you can use it to read the csv file without the header. </remarks>
    public Dictionary<string, string> CSVToDictionary(string text)
    {
        var dictionary = new Dictionary<string, string>();

        // Split the text into lines and process each line.
        // Since file structure is simple, we can just split on new line.
        text.Split('\n').ToList().ForEach(line =>
        {
            var values = line.Split(',');
            dictionary[values[0]] = values[1];
        });

        return dictionary;
    }

    /// <summary> Loads an asset from the scene and returns the instance. </summary>
    /// <param name="name"> The name of the asset. </param>
    public T GetSceneAsset<T>(string name) where T : class
    {
        // Since this metdod is used for instantiation from scenes, we should load the scene first.
        var asset = GetAsset<PackedScene>(name);

        var instance = asset.Instantiate();
        return instance as T;
    }

    /// <summary> Loads an asset by specified name and type. </summary>
    public T GetAsset<T>(string name) where T : class
    {
        var asset = Assets[name];
        if (asset != null) return GD.Load<T>(ASSETS_FOLDER_PATH + asset);
        else Logger.Log($"Asset {name} not found!", GameLogger.LogType.Error);
        
        return null;
    }
}
