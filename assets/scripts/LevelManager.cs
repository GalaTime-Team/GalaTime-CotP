using Galatime;
using Galatime.Global;
using Godot;
using System.Collections.Generic;

/// <summary> Manages the level. Contains the audios for the level and managing the time scale. </summary>
public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    /// <summary> The audio pack, which contains the audios for the level, contains the both calm and combat versions. </summary>
    public Dictionary<string, AudioPack> AudioPacks = new() {
            {"classicalbreak", new AudioPack(
                "res://assets/audios/soundtracks/enemies_test_level/classicalbreak.wav",
                "res://assets/audios/soundtracks/enemies_test_level/classicalbreakcalm.wav"
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

    public override void _Ready()
    {
        Instance = this;

        // Initialize audio players by creating them and adding them to the scene.
        InitializeAudioPlayers();
    }

    public void ReloadLevel() 
    {
        GalatimeGlobals.Instance.LoadScene(LevelInfo.LevelInstance.SceneFilePath);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
        {
            // This is for debugging purposes.
            if (keyEvent.Keycode == Key.F9) IsCombat = !IsCombat;
            if (keyEvent.Keycode == Key.F10) EndAudioCombat();
        }
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
}
