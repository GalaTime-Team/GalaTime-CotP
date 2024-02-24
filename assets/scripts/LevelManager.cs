using Godot;
using Galatime.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

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
            new Cheat(name: "[color=yellow]Game cheats[/color]", type: Cheat.CheatType.Separator),
            new Cheat(name: "Global data", type: Cheat.CheatType.Separator),
            new Cheat("list_items", "Print list of all items", "", "cheat_list_items", (bool active, string input) => {
                var items = GalatimeGlobals.ItemList;
                var str = new StringBuilder();
                    
                for (var i = 0; i < items.Count; i++) str.Append($"{items[i].ID}{(i < items.Count - 1 ? ", " : "")}");

                CheatsMenu.Log(str.ToString(), CheatsMenu.LogLevel.Result);
            }),
            new Cheat("abilities_list", "Print list of all abilities", "", "cheat_abilities_list", (bool active, string input) => {
                var abilities = GalatimeGlobals.AbilitiesList;
                var str = new StringBuilder();

                for (var i = 0; i < abilities.Count; i++) str.Append($"{abilities[i].ID}{(i < abilities.Count - 1 ? ", " : "")}");

                CheatsMenu.Log(str.ToString(), CheatsMenu.LogLevel.Result);
            }),
            new Cheat(name: "Gameplay cheats", type: Cheat.CheatType.Separator),
            new Cheat("god_mode", "God mode", "Toggles god mode, which makes the all characters invulnerable to all damage.", "cheat_god_mode", (active, _) => {
                var pv = PlayerVariables.Instance;
                Array.ForEach(pv.Allies, c => {
                    if (c.Instance != null) c.Instance.Invincible = active;
                });
            }, Cheat.CheatType.Toggle),
            new Cheat("ai_ignore_player", "Ai ignores player", "Toggles the Ai of entities to ignore the current selected player.", "cheat_ai_ignore_player", type: Cheat.CheatType.Toggle),
            new Cheat("add_ally", "Add ally", "Add an inputted ally to the player. To update allies, use 'cheat_add_allies'.", "cheat_add_ally", (_, input) => {
                var inputArguments = CheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var ally = GalatimeGlobals.GetAllyById(args[0]);
                if (ally == null)
                {
                    CheatsMenu.Log($"Ally {args[0]} not found", CheatsMenu.LogLevel.Warning);
                    return;
                }

                // Add the ally to any empty slot.
                for (int i = 0; i < PlayerVariables.Instance.Allies.Length; i++)
                {
                    var a = PlayerVariables.Instance.Allies[i];
                    if (a.IsEmpty) { PlayerVariables.Instance.Allies[i] = a; break; }
                }
            }, Cheat.CheatType.Input),
            new Cheat("remove_ally", "Remove ally", "Remove an inputted ally from the player. To update allies, use 'cheat_add_allies'.", "cheat_remove_ally", (_, input) => {
                var inputArguments = CheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var ally = PlayerVariables.Instance.Allies.FirstOrDefault(a => a.ID == args[0]);
                if (ally == null)
                {
                    CheatsMenu.Log($"Ally {args[0]} not found", CheatsMenu.LogLevel.Warning);
                    return;
                }

                // Remove the ally from the player.
                PlayerVariables.Instance.Allies[Array.IndexOf(PlayerVariables.Instance.Allies, ally)] = new AllyData();
            }, Cheat.CheatType.Input),
            new Cheat("update_allies", "Update all allies", "Updates all possible allies to the player and load them in the scene.", "cheat_update_allies", (bool active, string input) =>
            {
                var pv = PlayerVariables.Instance;
                var player = CheatsMenu.GetPlayer();
                if (player == null) return;

                for (int i = 0; i < GalatimeGlobals.AlliesList.Count; i++)
                {
                    pv.Allies[i] = GalatimeGlobals.AlliesList[i];
                }
                player.LoadCharacters("arthur");

                CheatsMenu.Log($"Updated allies", CheatsMenu.LogLevel.Result);
            }),
            new Cheat("give_xp", "Give XP", "Give an amount of XP to the player. Arguments: amount.", "", (bool active, string input) =>
            {
                var inputArguments = CheatsMenu.ParseCheatArguments(input, 1);

                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var player = CheatsMenu.GetPlayer();
                if (player == null) return;

                var xp = int.Parse(args[0]);
                PlayerVariables.Instance.Player.Xp += xp;

                CheatsMenu.Log($"Gave {xp} XP", CheatsMenu.LogLevel.Result);
            }, Cheat.CheatType.Input),
            new Cheat("give_item", "Give item", "Give an item to the player by specifying the item ID and quantity. Arguments: item_id quantity.", "cheat_give_item", (bool active, string input) =>
            {
                var inputArguments = CheatsMenu.ParseCheatArguments(input, 2);
                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var player = CheatsMenu.GetPlayer();
                if (player == null) return;

                var itemName = args[0];
                var itemQuantity = int.Parse(args[1]);
                var item = GalatimeGlobals.GetItemById(itemName);

                if (item.IsEmpty)
                {
                    CheatsMenu.Log($"Item not found: {itemName}", CheatsMenu.LogLevel.Error);
                    return;
                }

                PlayerVariables.Instance.AddItem(item, itemQuantity);

                CheatsMenu.Log($"Gave {itemQuantity}x {item.Name}", CheatsMenu.LogLevel.Result);
            }, Cheat.CheatType.Input),
            new Cheat("reload_all", "Remove all ability cooldowns", "Removes all ability cooldowns and charges.", "cheat_reload_all", (_, _) =>
            {
                var player = CheatsMenu.GetPlayer();
                if (player == null) return;

                var playerAbilities = player.Abilities;

                playerAbilities.ForEach(ability =>
                {
                    ability.Charges = ability.MaxCharges;
                    player.OnAbilityReload?.Invoke(playerAbilities.IndexOf(ability), 0);
                });

                CheatsMenu.Log($"Reloaded all abilities", CheatsMenu.LogLevel.Result);
            }),
            new Cheat("restore_all", "Restore all resources", "Restores all health, mana, and stamina.", "cheat_restore_all", (_, _) =>
            {
                var player = CheatsMenu.GetPlayer();
                if (player == null) return;

                var ally = Player.CurrentCharacter;

                ally.Health = ally.Stats[EntityStatType.Health].Value;
                ally.Mana.Value = ally.Stats[EntityStatType.Mana].Value;
                ally.Stamina.Value = ally.Stats[EntityStatType.Stamina].Value;

                CheatsMenu.Log($"Restored all resources for player", CheatsMenu.LogLevel.Result);
            }),
            new Cheat(name: "Entities cheats", type: Cheat.CheatType.Separator),
            new Cheat("disable_ai", "Disable entities AI", "Disables AI for all entities, meaning that they will not perform any actions.", "cheat_disable_ai", (bool active, string input) => Entities.ForEach(entity => entity.DisableAI = active), Cheat.CheatType.Toggle),
            new Cheat("kill_all_enemies", "Kill all enemies", "", "cheat_kill_all", (_, _) => {
                var enemies = Entities.Where(entity => !(entity is TestCharacter || entity is Player) && !entity.DeathState).ToList();
                if (enemies.Count() == 0) 
                {
                    CheatsMenu.Log("There's no enemies to kill", CheatsMenu.LogLevel.Warning);
                    return;
                }
                enemies.ForEach(entity => entity.SetHealth(-1f));
                CheatsMenu.Log($"Successfully killed all {enemies.Count()} enemies", CheatsMenu.LogLevel.Result);
            }),
            new Cheat(name: "Drama cheats", type: Cheat.CheatType.Separator),
            new Cheat("start_cutscene", "Start cutscene", "Starts a cutscene.", "cheat_start_cutscene", (_, input) => {
                var inputArguments = CheatsMenu.ParseCheatArguments(input, 1);
                var args = inputArguments.args;
                if (!inputArguments.result) return;

                var cutsceneName = args[0];
                CutsceneManager.Instance.StartCutscene(cutsceneName);
            }, Cheat.CheatType.Input)
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
