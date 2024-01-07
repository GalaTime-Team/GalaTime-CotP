using System;
using YamlDotNet.Serialization;

namespace Galatime.Settings;

public class SettingsData
{
    [YamlMember(Alias = "binds"), SettingProperty("Binds")]
    public BindsSettingsData Binds = new();

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
    public bool DiscordActivityDisabled;
}

public class BindsSettingsData
{
    [YamlMember(Alias = "move_up"), SettingProperty("Move up"), KeybindSetting("game_move_up")]
    public long MoveUp = (long)Godot.Key.W;

    [YamlMember(Alias = "move_down"), SettingProperty("Move down"), KeybindSetting("game_move_down")]
    public long MoveDown = (long)Godot.Key.S;

    [YamlMember(Alias = "move_left"), SettingProperty("Move left"), KeybindSetting("game_move_left")]
    public long MoveLeft = (long)Godot.Key.A;

    [YamlMember(Alias = "move_right"), SettingProperty("Move right"), KeybindSetting("game_move_right")]
    public long MoveRight = (long)Godot.Key.D;

    [YamlMember(Alias = "dodge"), SettingProperty("Dodge"), KeybindSetting("game_dodge")]
    public long Dodge = (long)Godot.Key.Shift;

    [YamlMember(Alias = "inventory"), SettingProperty("Inventory"), KeybindSetting("game_inventory")]
    public long Pause = (long)Godot.Key.B;

    [YamlMember(Alias = "character_wheel"), SettingProperty("Character wheel"), KeybindSetting("game_character_wheel")]
    public long Jump = (long)Godot.Key.F;

    [YamlMember(Alias = "potion_wheel"), SettingProperty("Potion wheel"), KeybindSetting("game_potion_wheel")]
    public long PotionWheel = (long)Godot.Key.G;

    [YamlMember(Alias = "cheats_menu"), SettingProperty("Cheats menu"), KeybindSetting("cheats_menu")]
    public long CheatsMenu = (long)Godot.Key.Quoteleft;
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

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class KeybindSettingAttribute : Attribute
{
    public KeybindSettingAttribute(string actionName) => ActionName = actionName;
    public string ActionName;
}