using YamlDotNet.Serialization;
using System;

namespace Galatime.Settings;

public class Settings {
    /// <summary> The master volume of the game. </summary>
    [YamlMember(Alias = "master_volume"), SettingName("Master Volume")]
    public double MasterVolume = 1;
    /// <summary> The music volume of the game. </summary>
    [YamlMember(Alias = "music_volume"), SettingName("Music Volume")]
    public double MusicVolume = 1;

    /// <summary> If the Discord presence is disabled. </summary>
    [YamlMember(Alias = "discord_presence_disabled"), SettingName("Discord Presence Disabled")]
    public bool DiscordActivityDisabled = false;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class SettingNameAttribute : Attribute
{
    public SettingNameAttribute(string name) => Name = name;
    public string Name;
}