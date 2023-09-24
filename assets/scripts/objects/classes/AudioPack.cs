using System;
using Godot;

namespace Galatime;

public struct AudioPack
{
    public string AudioCombatPathFile;
    public string AudioCombatCalmPathFile;

    public AudioStream AudioCombat;
    public AudioStream AudioCombatCalm;

    public readonly bool IsLoaded => AudioCombat != null && AudioCombatCalm != null;

    public AudioPack(string audioCombat, string audioCombatCalm) {
        AudioCombatPathFile = audioCombat;
        AudioCombatCalmPathFile = audioCombatCalm;
        AudioCombat = GD.Load<AudioStream>(audioCombat);
        AudioCombatCalm = GD.Load<AudioStream>(audioCombatCalm);
    }
}
