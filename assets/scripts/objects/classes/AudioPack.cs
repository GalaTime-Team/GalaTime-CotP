using Godot;

namespace Galatime;

public class AudioPack
{
    /// <summary> Path to audio files for combat and calm audio. </summary>
    public string AudioCombatPath, AudioCombatCalmPath;

    public AudioStream AudioCombat, AudioCombatCalm;

    public AudioPack(string audioCombat, string audioCombatCalmPath)
    {
        // Assign the audio paths.
        (AudioCombatPath, AudioCombatCalmPath) = (audioCombat, audioCombatCalmPath);
    }

    public void Load()
    {
        // Load the combat and calm audio streams.
        AudioCombat = GD.Load<AudioStream>(AudioCombatPath);
        AudioCombatCalm = GD.Load<AudioStream>(AudioCombatCalmPath);
    }
}
