using YamlDotNet.Serialization;
using System;

namespace Galatime.Settings;

public class Settings {
    /// <summary> The master volume of the game. </summary>
    [YamlMember(Alias = "master_volume"), SettingProperties("Master Volume")]
    public double MasterVolume = 1;

    /// <summary> The music volume of the game. </summary>
    [YamlMember(Alias = "music_volume"), SettingProperties("Music Volume")]
    public double MusicVolume = 1;

    /// <summary> If the Discord presence is disabled. </summary>
    [YamlMember(Alias = "discord_presence_disabled"), SettingProperties("Discord Presence Disabled")]
    public bool DiscordActivityDisabled = false;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class SettingPropertiesAttribute : Attribute
{
    public SettingPropertiesAttribute(string name) => Name = name;
    public string Name;
}