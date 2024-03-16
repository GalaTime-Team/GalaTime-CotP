using System;
using YamlDotNet.Serialization;

namespace Galatime.Settings;

public class SettingsData
{
    [YamlMember(Alias = "video"), SettingProperty("Video")]
    public VideoSettingsData Video = new();
    [YamlMember(Alias = "binds"), SettingProperty("Binds")]
    public BindsSettingsData Binds = new();

    [YamlMember(Alias = "audio"), SettingProperty("Audio")]
    public AudioSettingsData Audio = new();

    [YamlMember(Alias = "misc"), SettingProperty("Misc")]
    public MiscSettingsData Misc = new();
}

public class VideoSettingsData
{
    /// <summary> The resolution of the game. </summary>
    [YamlMember(Alias = "resolution"), SettingProperty("Resolution"), OptionsSetting(new string[] {"Current Resolution", "640x480", "800x600", "1024x768", "1152x864", "1280x960", "1280x720", "1920x1080" })]
    public string Resolution = "1280x720";
    [YamlMember(Alias = "max_fps"), SettingProperty("Max FPS"), OptionsSetting(new string[]{ "No Limit", "15", "30", "60", "90", "120", "144", "240" })]
    public string MaxFps = "No Limit";
    /// <summary> If the game is fullscreen. </summary>
    [YamlMember(Alias = "fullscreen"), SettingProperty("Fullscreen")]
    public bool Fullscreen = false;
    /// <summary> If the game is vsync enabled. </summary>
    [YamlMember(Alias = "vsync"), SettingProperty("Vsync")]
    public bool Vsync = true;
}

public class AudioSettingsData
{
    /// <summary> The master volume of the game. </summary>
    [YamlMember(Alias = "master_volume"), SettingProperty("Master Volume")]
    public double MasterVolume = 1;

    /// <summary> The music volume of the game. </summary>
    [YamlMember(Alias = "music_volume"), SettingProperty("Music Volume")]
    public double MusicVolume = 1;
}

public class MiscSettingsData
{
    /// <summary> If the Discord presence is disabled. </summary>
    [YamlMember(Alias = "discord_presence_disabled"), SettingProperty("Discord Presence Disabled")]
    public bool DiscordActivityDisabled;
    [YamlMember(Alias = "disable_damage_indicators"), SettingProperty("Disable Damage Indicator")]
    public bool DisableDamageIndicator;
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
    public long CharacterWheel = (long)Godot.Key.F;

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

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class OptionsSettingAttribute : Attribute
{
    public OptionsSettingAttribute(string[] names) => Names = names;
    public string[] Names;
}