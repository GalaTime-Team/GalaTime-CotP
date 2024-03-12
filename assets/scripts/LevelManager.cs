using Galatime.Helpers;
using Galatime.Interfaces;

using Godot;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Galatime.Global;

/// <summary> Represents an object in the level. Contains the name of the object and the state that can be any type. </summary>
public struct LevelObject
{
    public string Name = "";
    public object[] Data;

    public LevelObject(string name = "", object[] data = null) => (Name, Data) = (name, data);

    public static bool operator ==(LevelObject left, LevelObject right) => left.Name == right.Name;
    public static bool operator !=(LevelObject left, LevelObject right) => left.Name != right.Name;
}

/// <summary> Manages the level. Contains the audios for the level and managing the time scale. </summary>
public partial class LevelManager : Node
{
    public GameLogger Logger { get; private set; } = new("LevelManager", GameLogger.ConsoleColor.Green);
    public static LevelManager Instance { get; private set; }

    /// <summary> The time, in seconds, which determines the switching duration between calm and combat. </summary>
    const float AUDIO_SWITCHING_DURATION = 0.5f;
    /// <summary> The time, in seconds, which determines the duration of the end of the combat audios. </summary>
    const float AUDIO_ENDING_DURATION = 3f;
    public const string CHEAT_MENU_SCENE_PATH = "res://assets/objects/gui/CheatsMenu.tscn";
    /// <summary> The audio pack, which contains the audios for the level, contains the both calm and combat versions. </summary>
    public Dictionary<string, AudioPack> AudioPacks = new() {
            {"classicalbreak", new AudioPack(
                "res://assets/audios/soundtracks/dream_world.mp3",
                "res://assets/audios/soundtracks/dream_world.mp3"
            )}
        };

    public CheatsMenu CheatsMenu;
    /// <summary> The canvas layer. Used to add global independent UI elements. </summary>
    public CanvasLayer CanvasLayer;

    /// <summary> The audio player when the character is in combat. </summary>
    public AudioStreamPlayer AudioPlayerCombat;
    /// <summary> The audio player when the character is in combat but this is a calm version. </summary>
    public AudioStreamPlayer AudioPlayerCombatCalm;

    public bool AudioCombatIsPlaying => AudioPlayerCombat.Playing && AudioPlayerCombatCalm.Playing;
    private bool isCombat = false;
    /// <summary> If the character is in combat. </summary>
    public bool IsCombat
    {
        get => isCombat;
        set
        {
            isCombat = value;
            if (!isCombat)
            {
                GetTree().CreateTimer(2).Timeout += () =>
                {
                    Array.ForEach(PlayerVariables.Instance.Allies, e => e.Instance?.Revive());
                };
            }
            // PlayerVariables.Instance.Allies
            SwitchAudio(isCombat);
        }
    }

    private LevelInfo levelInfo;
    public LevelInfo LevelInfo
    {
        get => levelInfo;
        set
        {
            levelInfo = value;

            // Getting current level objects in current level.
            CurrentLevelObjects.Clear();
            PlayerSpawnPoints.Clear();

            ForEachLevelNode(x =>
            {
                if (x is ILevelObject obj)
                {
                    Logger.Log($"Updated current object to level: {x.Name}", GameLogger.LogType.Info);
                    CurrentLevelObjects.Add(obj);
                }
                if (x is PlayerSpawnPoint spawnPoint) PlayerSpawnPoints.Add(spawnPoint);
            });
            UpdateLevelObjects();

            InitializeSpawnPointsAndPlayer();

            // Set the titles for the game.
            DiscordController.Instance.CurrentRichPresence.Details = levelInfo.LevelName;
            if (levelInfo.Day > 0) DiscordController.Instance.CurrentRichPresence.State = $"Day {levelInfo.Day} - Playing";
            else DiscordController.Instance.CurrentRichPresence.State = null;

            GetTree().Root.Title = $"GalaTime - {levelInfo.LevelName}";
        }
    }

    /// <summary> The entities in the current level. </summary>
    public List<Entity> Entities = new();

    /// <summary> List of current level objects in the current level. </summary>
    public List<ILevelObject> CurrentLevelObjects = new();
    /// <summary> List of level objects in stored levels. Contains the state of each level object. </summary>
    public Dictionary<string, List<LevelObject>> LevelObjects = new();
    /// <summary> Player spawn points in the current level. </summary>
    public List<PlayerSpawnPoint> PlayerSpawnPoints = new();
    public int PlayerSpawnPointIndex = 0;

    public override void _Ready()
    {
        Instance = this;

        CanvasLayer = new CanvasLayer() { Layer = 10 }; // Layer 10, to make sure it's on top of the other UI elements.
        AddChild(CanvasLayer);
        var cheatsMenuScene = GD.Load<PackedScene>(CHEAT_MENU_SCENE_PATH);
        CheatsMenu = cheatsMenuScene.Instantiate<CheatsMenu>();
        CanvasLayer.AddChild(CheatsMenu);

        GameCheatList.InitializeCheats(CheatsMenu);

        // Initialize audio players by creating them and adding them to the scene.
        InitializeAudioPlayers();
    }

    public override void _Process(double delta)
    {
        RemoveInvalidEntities();
    }

    #region Level management
    /// <summary> Gets the level objects in current level. </summary>
    public List<LevelObject> GetCurrentLevelObjects() => LevelObjects[levelInfo.LevelName];
    /// <summary> Gets the level object in current level by name. </summary>
    public LevelObject GetLevelObject(string name) => GetCurrentLevelObjects().Find(x => x.Name == name);
    /// <summary> Gets the level object index in current level by name. </summary>
    public int GetLevelObjectIndex(string name) => GetCurrentLevelObjects().FindIndex(x => x.Name == name);
    /// <summary> Iterates over all nodes in current level. </summary>
    public void ForEachLevelNode(Action<Node> action) => levelInfo.GetParent().GetChildren().ToList().ForEach(action);

    /// <summary> Updates the level objects. This includes adding current level objects and changing state of level objects. </summary>
    public void UpdateLevelObjects()
    {
        // If level object doesn't exist for the current level, add it.
        if (!LevelObjects.ContainsKey(levelInfo.LevelName))
        {
            LevelObjects.Add(levelInfo.LevelName, new());
            // Adding level objects to the level with default data.
            CurrentLevelObjects.ForEach(x =>
            {
                // It's impossible to add object to level if it's not a Node.
                if (x is not Node obj) Logger.Log($"{x.GetType()} is not an Node.", GameLogger.LogType.Warning);
                else
                {
                    Logger.Log($"Added object to level: {obj.Name}", GameLogger.LogType.Info);
                    LevelObjects[levelInfo.LevelName].Add(new(obj.Name, Array.Empty<object>()));
                }
            });
        }
        else // Load level objects from the level.
        {
            CurrentLevelObjects.ForEach(x =>
            {
                // It's impossible to load level object if it's not a Node.
                if (x is not Node obj) Logger.Log($"{x.GetType()} is not an Node.", GameLogger.LogType.Warning);
                else
                {
                    var levelObj = GetLevelObject(obj.Name);
                    Logger.Log($"Loading object from level: {obj.Name}. Data has {levelObj.Data.Length} elements", GameLogger.LogType.Info);
                    if (levelObj.Data.Length > 0) x.LoadLevelObject(levelObj.Data);
                    else Logger.Log($"Level object {obj.Name} has no data. Level object will not be loaded", GameLogger.LogType.Info);
                }
            });
        }
    }

    /// <summary> Saves the state of the level object that can be synchronized between levels. </summary>
    public void SaveLevelObject(Node2D levelObject, object[] data, Action callback = null)
    {
        var objects = GetCurrentLevelObjects();

        var i = GetLevelObjectIndex(levelObject.Name);
        if (i != -1) // -1 means that level object doesn't exist
        {
            Logger.Log($"Saving level object: {levelObject.Name}. Data has {data.Length} elements", GameLogger.LogType.Info);

            // Adding data to the level object.
            var obj = GetCurrentLevelObjects()[i];
            obj.Data = data;
            GetCurrentLevelObjects()[i] = obj;

            callback?.Invoke();
        }
        else
            Logger.Log($"Level object not found: {levelObject.Name}", GameLogger.LogType.Warning);
    }

    private void InitializeSpawnPointsAndPlayer()
    {
        if (PlayerSpawnPoints.Count > 0)
        {
            var player = AssetsManager.Instance.GetSceneAsset<Player>("player");

            var spawn = PlayerSpawnPoints.Find(x => x.SpawnIndex == PlayerSpawnPointIndex);
            if (spawn == null)
            {
                Logger.Log("You tried to load player spawn point from level that doesn't exist, using first spawn point. Please add one.", GameLogger.LogType.Warning);
                spawn = PlayerSpawnPoints[0];
            }
            Callable.From(() =>
            {
                spawn.GetParent().AddChild(player);
                player.GlobalPosition = spawn.GlobalPosition;
            }).CallDeferred();
        }
        else
        {
            Logger.Log("Looks like there is no player spawn point in the level. Please add one.", GameLogger.LogType.Error);
        }
    }

    public void ReloadLevel()
    {
        GalatimeGlobals.Instance.LoadScene(LevelInfo.LevelInstance.SceneFilePath);
    }
    #endregion

    #region Entity management
    private void RemoveInvalidEntities()
    {
        var entitiesToRemove = new List<Entity>();

        foreach (var entity in Entities)
        {
            if (!IsInstanceValid(entity) || entity == null)
            {
                // Add the entity to the removal list.
                entitiesToRemove.Add(entity);
            }
        }

        // Remove the entities from the main list.
        foreach (var entity in entitiesToRemove)
        {
            Entities.Remove(entity);
        }
    }

    /// <summary> Registers an entity to the level. </summary>
    public void RegisterEntity(Entity entity)
    {
        Entities.Add(entity);

        // Disable the AI if the cheat is active.
        var disableAiCheat = CheatsMenu.GetCheat("disable_ai");
        entity.DisableAI = disableAiCheat != null && disableAiCheat.Active;
    }
    #endregion

    #region Audio management
    private void InitializeAudioPlayers()
    {
        AudioPlayerCombat = new() { VolumeDb = -80 };
        AudioPlayerCombatCalm = new() { VolumeDb = -80 };
        AddChild(AudioPlayerCombat);
        AddChild(AudioPlayerCombatCalm);
        AudioPlayerCombat.Bus = "Music";
        AudioPlayerCombatCalm.Bus = "Music";
    }

    /// <summary> Plays the audio for the combat. </summary>
    public void PlayAudioCombat(string audioPack)
    {
        // Check if the combat audios is played, if so, don't stop playing.
        if (AudioCombatIsPlaying) return;

        // Set the audio stream and play it
        // We can also just play calm or combat, but sound will be unsynchronized.
        AudioPlayerCombat.Stream = AudioPacks[audioPack].AudioCombat; AudioPlayerCombat.Play();
        AudioPlayerCombatCalm.Stream = AudioPacks[audioPack].AudioCombatCalm; AudioPlayerCombatCalm.Play();

        // Set the volume to calm or combat. 0 means is playing, -80 means silent.
        AudioPlayerCombatCalm.VolumeDb = 0;
    }

    public void EndAudioCombat()
    {
        SwitchAudio(false, AUDIO_ENDING_DURATION, true);
    }

    private void SwitchAudio(bool isCombat, float duration = AUDIO_SWITCHING_DURATION, bool stopMusic = false)
    {
        var tween = GetTree().CreateTween().SetParallel();

        // Set the player based on the combat state
        var primaryPlayer = isCombat ? AudioPlayerCombat : AudioPlayerCombatCalm;
        var secondaryPlayer = isCombat ? AudioPlayerCombatCalm : AudioPlayerCombat;

        // Tween the volume. 
        tween.TweenProperty(primaryPlayer, "volume_db", stopMusic ? -80 : 0, duration);
        tween.TweenProperty(secondaryPlayer, "volume_db", -80, duration).SetDelay(stopMusic ? 0 : duration / 1.25).Finished += () =>
        {
            // Stop the music if needed by waiting for the tween to finish.
            if (stopMusic)
            {
                primaryPlayer.Stop();
                secondaryPlayer.Stop();
            }
        };
    }
    #endregion

    #region Tools
    /// <summary> Smoothly fades the time scale. </summary>
    /// <param name="duration"> The duration of the transition. </param>
    public void TweenTimeScale(float duration = 0.5f)
    {
        var tween = GetTree().CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(x => Engine.TimeScale = x), 0.1f, 1f, duration);
        AudioPlayerCombat.PitchScale = 0.1f;
        AudioPlayerCombatCalm.PitchScale = 0.1f;
        tween.TweenProperty(AudioPlayerCombat, "pitch_scale", 1f, duration);
        tween.TweenProperty(AudioPlayerCombatCalm, "pitch_scale", 1f, duration);
    }
    #endregion
}
