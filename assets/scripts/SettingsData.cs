using YamlDotNet.Serialization;
using System;

namespace Galatime.Settings;

public class SettingsData {
    /// <summary> The master volume of the game. </summary>
    [YamlMember(Alias = "master_volume"), SettingProperties("Master Volume"), RangeSetting(-80, 0, 4)]
    public double MasterVolume = 1;

    /// <summary> The music volume of the game. </summary>
    [YamlMember(Alias = "music_volume"), SettingProperties("Music Volume"), RangeSetting(-80, 0, 4)]
    public double MusicVolume = 1;

    /// <summary> If the Discord presence is disabled. </summary>
    [YamlMember(Alias = "discord_presence_disabled"), SettingProperties("Discord Presence Disabled"), RangeSetting(-80, 0, 4)]
    public bool DiscordActivityDisabled = false;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class SettingPropertiesAttribute : Attribute
{
    public SettingPropertiesAttribute(string name) => Name = name;
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