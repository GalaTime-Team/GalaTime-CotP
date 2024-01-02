using Godot;
using Galatime.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Galatime.Global;

/// <summary> Manages the level. Contains the audios for the level and managing the time scale. </summary>
public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    /// <summary> The audio pack, which contains the audios for the level, contains the both calm and combat versions. </summary>
    public Dictionary<string, AudioPack> AudioPacks = new() {
            {"classicalbreak", new AudioPack(
                "res://assets/audios/soundtracks/dream_world.mp3",
                "res://assets/audios/soundtracks/dream_world.mp3"
            )}
        };

    /// <summary> The audio player when the character is in combat. </summary>
    public AudioStreamPlayer AudioPlayerCombat;
    /// <summary> The audio player when the character is in combat but this is a calm version. </summary>
    public AudioStreamPlayer AudioPlayerCombatCalm;

    /// <summary> The time, in seconds, which determines the switching duration between calm and combat. </summary>
    const float AUDIO_SWITCHING_DURATION = 0.5f;

    /// <summary> The time, in seconds, which determines the duration of the end of the combat audios. </summary>
    const float AUDIO_ENDING_DURATION = 3f;
    public const string CHEAT_MENU_SCENE_PATH = "res://assets/objects/gui/CheatsMenu.tscn";

    public bool AudioCombatIsPlaying => AudioPlayerCombat.Playing && AudioPlayerCombatCalm.Playing;
    private bool isCombat = false;
    /// <summary> If the character is in combat. </summary>
    public bool IsCombat
    {
        get => isCombat;
        set
        {
            isCombat = value;
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

            // Set the titles for the game.
            DiscordController.Instance.CurrentRichPresence.Details = levelInfo.LevelName;
            if (levelInfo.Day > 0) DiscordController.Instance.CurrentRichPresence.State = $"Day {levelInfo.Day} - Playing";
            else DiscordController.Instance.CurrentRichPresence.State = null;

            GetTree().Root.Title = $"GalaTime - {levelInfo.LevelName}";
        }
    }

    /// <summary> The entities in the current level. </summary>
    public List<Entity> Entities = new();
    public CheatsMenu CheatsMenu;
    /// <summary> The canvas layer. Used to add global independent UI elements. </summary>
    public CanvasLayer CanvasLayer;

    public override void _Ready()
    {
        Instance = this;

        CanvasLayer = new CanvasLayer() { Layer = 10 }; // Layer 10, to make sure it's on top of the other UI elements.
        AddChild(CanvasLayer);
        var cheatsMenuScene = GD.Load<PackedScene>(CHEAT_MENU_SCENE_PATH);
        CheatsMenu = cheatsMenuScene.Instantiate<CheatsMenu>();
        CanvasLayer.AddChild(CheatsMenu);

        // Initialize audio players by creating them and adding them to the scene.
        InitializeAudioPlayers();

        CheatsMenu.RegisterCheat(
            new Cheat("disable_ai", "Disable entities AI", "cheat_disable_ai", (bool active) => Entities.ForEach(entity => entity.DisableAI = active)),
            new Cheat("kill_all_entities", "Kill all entities", "cheat_kill_all", (_) => Entities.ForEach(entity => entity.TakeDamage(99999, 99999, new GalatimeElement())), Cheat.CheatType.Button),
            new Cheat("reload_all", "Remove all ability cooldowns", "cheat_reload_all", (_) =>
            {
                var player = Player.CurrentCharacter;
                var playerAbilities = player.Abilities;

                playerAbilities.ForEach(ability =>
                {
                    ability.Charges = ability.MaxCharges;
                    player.OnAbilityReload?.Invoke(playerAbilities.IndexOf(ability), 0);
                });
            }, Cheat.CheatType.Button),
            new Cheat("restore_all", "Restore all resources", "cheat_restore_all", (_) =>
            {
                var player = Player.CurrentCharacter;

                player.Health = player.Stats[EntityStatType.Health].Value;
                player.Mana.Value = player.Stats[EntityStatType.Mana].Value;
                player.Stamina.Value = player.Stats[EntityStatType.Stamina].Value;
            }, Cheat.CheatType.Button),
            new Cheat("add_allies", "Add all allies", "cheat_add_allies", (bool active) =>
            {
                var pv = PlayerVariables.Instance;
                for (int i = 0; i < GalatimeGlobals.AlliesList.Count; i++)
                {
                    pv.Allies[i] = GalatimeGlobals.AlliesList[i];
                }
                pv.Player.LoadCharacters("arthur");
            }, Cheat.CheatType.Button)
        );
    }

    public override void _Process(double delta)
    {
        RemoveInvalidEntities();
    }

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


    public void ReloadLevel()
    {
        GalatimeGlobals.Instance.LoadScene(LevelInfo.LevelInstance.SceneFilePath);
    }

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

    /// <summary> Registers an entity to the level. </summary>
    public void RegisterEntity(Entity entity)
    {
        Entities.Add(entity);

        // Disable the AI if the cheat is active.
        var disableAiCheat = CheatsMenu.GetCheat("disable_ai");
        entity.DisableAI = disableAiCheat != null && disableAiCheat.Active;
    }
}
