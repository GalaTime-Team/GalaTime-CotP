using Galatime.Global;
using Godot;

namespace Galatime;

/// <summary> Represents an audio pack that contains two audios, that should be played at the same time. </summary>
public class AudioPack
{
    /// <summary> Path to audio files for combat and calm audio. </summary>
    public string AudioAAssetName, AudioBAssetName;

    public AudioStream AudioA, AudioB;
    public string AudioBusA = "Music", AudioBusB = "Music";

    /// <summary> Initializes a new instance of the <see cref="AudioPack"/> and loads the same audio for A and B. </summary>
    public AudioPack(string audioAssetName)
    {
        // Assign the audio path.
        AudioAAssetName = AudioBAssetName = audioAssetName;
        Load();
    }

    /// <summary> Initializes a new instance of the <see cref="AudioPack"/> and loads the individual audios for A and B. </summary>
    public AudioPack(string audioAAssetName, string audioBAssetName)
    {
        // Assign the audio paths.
        (AudioAAssetName, AudioBAssetName) = (audioAAssetName, audioBAssetName);
        Load();
    }

    /// <summary> Initializes a new instance of the <see cref="AudioPack"/> and loads the individual audios for A and B with audio busses if specified. </summary>
    public AudioPack(string audioAAssetName, string audioBAssetName, string audioBusA = "Music", string audioBusB = "Music")
    {
        // Assign the audio paths.
        (AudioAAssetName, AudioBAssetName, AudioBusA, AudioBusB) = (audioAAssetName, audioBAssetName, audioBusA, audioBusB);
        Load();
    }

    public AudioStream GetByIndex(int index) => index == 0 ? AudioA : AudioB;
    public string GetBusByIndex(int index) => index == 0 ? AudioBusA : AudioBusB;

    public void Load()
    {
        var am = AssetsManager.Instance;

        // Load the combat and calm audio streams.
        AudioA = am.GetAsset<AudioStream>(AudioAAssetName);
        AudioB = am.GetAsset<AudioStream>(AudioBAssetName);
    }
}
