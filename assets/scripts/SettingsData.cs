using System;
using YamlDotNet.Serialization;

namespace Galatime.Settings;

public class SettingsData
{
    [YamlMember(Alias = "audio"), SettingProperty("Audio")]
    public AudioSettingsData Audio = new();

    [YamlMember(Alias = "misc"), SettingProperty("Misc")]
    public MiscSettingsData Misc = new();
}

public class AudioSettingsData
{
    /// <summary> The master volume of the game. </summary>
    [YamlMember(Alias = "master_volume"), SettingProperty("Master Volume"), RangeSetting(-80, 0, 4)]
    public double MasterVolume = 1;

    /// <summary> The music volume of the game. </summary>
    [YamlMember(Alias = "music_volume"), SettingProperty("Music Volume"), RangeSetting(-80, 0, 4)]
    public double MusicVolume = 1;
}

public class MiscSettingsData
{
    /// <summary> If the Discord presence is disabled. </summary>
    [YamlMember(Alias = "discord_presence_disabled"), SettingProperty("Discord Presence Disabled")]
    public bool DiscordActivityDisabled = false;
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingPropertyAttribute : Attribute
{
    public SettingPropertyAttribute(string name) => Name = name;
    public string Name;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class RangeSettingAttribute : Attribute
{
    public RangeSettingAttribute(double min, double max, double step = 0.1) => (Min, Max, Step) = (min, max, step);
    public double Min;
    public double Max;
    public double Step;
}
