using System.Collections.Generic;
using Galatime;
using Godot;


/// <summary> A singleton that manages the music in the game. </summary>
public partial class MusicManager : Node
{
    public static MusicManager Instance { get; private set; }

    /// <summary> The time, in seconds, which determines the switching duration between audios </summary>
    const float AUDIO_SWITCHING_DURATION = 0.5f;

    /// <summary> The audio pack, which contains the audios for the level, contains the both A and B audios. </summary>
    public Dictionary<string, AudioPack> AudioPacks;

    public string CurrentAudioPack { get; private set; }

    /// <summary> The A audio player. </summary>
    public AudioStreamPlayer AudioPlayerA;
    /// <summary> The B audio player. </summary>
    public AudioStreamPlayer AudioPlayerB;
    /// <summary> The C audio player. Used mostly for transition between audio packs. </summary>
    public AudioStreamPlayer AudioPlayerC;
    /// <summary> The D audio player. Used mostly for transition between audio packs. </summary>
    public AudioStreamPlayer AudioPlayerD;

    public AudioStreamPlayer[] CurrentAudioPlayers;

    public bool PlayersBit = true;

    public bool AudioIsPlaying => AudioPlayerA.Playing && AudioPlayerB.Playing;

    public override void _Ready()
    {
        Instance = this;
        InitializeAudioPlayers();
    }

    private void InitializeAudioPlayers()
    {
        AudioPacks = new() {
            {
                "dream_world", 
                new AudioPack
                (
                    "dream_world_audio",
                    "dream_world_audio",
                    audioBusB: "MuffledMusic"
                )
            },
            {
                "galatime", 
                new AudioPack
                (
                    "galatime_audio",
                    "galatime_audio",
                    audioBusB: "MuffledMusic"
                )
            }
        };

        AudioStreamPlayer Init(string l)
        {
            var player = new AudioStreamPlayer { Name = "Audio" + l, Bus = "Music", VolumeDb = -80 };
            AddChild(player);
            return player;
        }

        AudioPlayerA = Init("A");
        AudioPlayerB = Init("B");
        AudioPlayerC = Init("C");
        AudioPlayerD = Init("D");
    }

    /// <summary> Plays the audio from the given audio pack.  </summary>
    public void Play(string audioPack, float duration = AUDIO_SWITCHING_DURATION)
    {
        // Check if the combat audios is played, if so, don't stop playing.
        if (AudioIsPlaying && CurrentAudioPack == audioPack && !string.IsNullOrEmpty(CurrentAudioPack)) return;

        var aplayers = PlayersBit ? new AudioStreamPlayer[] { AudioPlayerA, AudioPlayerB } : new AudioStreamPlayer[] { AudioPlayerC, AudioPlayerD };
        var bplayers = !PlayersBit ? new AudioStreamPlayer[] { AudioPlayerA, AudioPlayerB } : new AudioStreamPlayer[] { AudioPlayerC, AudioPlayerD };

        SwitchAudio(true, aplayers[0], aplayers[1]);
        GetTree().CreateTimer(duration).Timeout += () => FadeBoth(duration, true, bplayers);

        CurrentAudioPlayers = aplayers;
        PlayersBit = !PlayersBit;

        // Set the audio stream and play it
        for (int i = 0; i < 2; i++)
        {
            var player = CurrentAudioPlayers[i];
            player.Stream = AudioPacks[audioPack].GetByIndex(i);
            player.Bus = AudioPacks[audioPack].GetBusByIndex(i);
            player.Play();
        }

        CurrentAudioPack = audioPack;
    }

    public void SwitchAudio(bool bit, float duration = AUDIO_SWITCHING_DURATION, bool stopMusic = false) => SwitchAudio(bit, CurrentAudioPlayers[0], CurrentAudioPlayers[1], duration, stopMusic);

    private void SwitchAudio(bool bit, AudioStreamPlayer A, AudioStreamPlayer B, float duration = AUDIO_SWITCHING_DURATION, bool stopMusic = false)
    {
        var tween = GetTree().CreateTween().SetParallel();

        // Set the player based on the bit.
        var primaryPlayer = bit ? A : B;
        var secondaryPlayer = bit ? B : A;

        // Tween the volume. 
        tween.TweenProperty(primaryPlayer, "volume_db", stopMusic ? -80 : 0, duration);
        tween.TweenProperty(secondaryPlayer, "volume_db", -80, duration).SetDelay(stopMusic ? 0 : duration).Finished += () =>
        {
            // Stop the music if needed by waiting for the tween to finish.
            if (stopMusic)
            {
                primaryPlayer.Stop();   
                secondaryPlayer.Stop();

                CurrentAudioPack = string.Empty;
            }
        };
    }

    private void FadeBoth(float duration = AUDIO_SWITCHING_DURATION, bool stopMusic = false, params AudioStreamPlayer[] players)
    {
        var tween = GetTree().CreateTween().SetParallel();
        foreach (var player in players)
            tween.TweenProperty(player, "volume_db", stopMusic ? -80 : 0, duration);
    }
}
