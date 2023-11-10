using Galatime;
using Galatime.Global;
using Galatime.Settings;
using Godot;
using System;
using YamlDotNet.Serialization;

namespace Galatime.Global;

public partial class SettingsGlobals : Node
{
    public static SettingsData Settings = new();

    public override void _Ready() => LoadSettings();

    /// <summary> Loads the settings from the settings.yml file. </summary>
    /// <returns> As option, returns loaded settings, otherwise use static type to get settings. </returns>
    public Galatime.Settings.SettingsData LoadSettings()
    {
        var file = Godot.FileAccess.Open(GalatimeConstants.settingsPath, Godot.FileAccess.ModeFlags.Read);

        var error = Godot.FileAccess.GetOpenError();
        if (error != Error.Ok)
        {
            GD.PrintErr($"SETTINGS: Error when loading a config: {error}. Creating a new config.");
            SaveSettings();
            return new();
        }

        var settingsText = file.GetAsText();
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        Settings = deserializer.Deserialize<SettingsData>(settingsText);

        return Settings;
    }

    /// <summary> Saves the settings to the settings.yml file. </summary>
    public void SaveSettings()
    {
        var saveProcessSceneInstance = GalatimeGlobals.saveProcessScene.Instantiate<SavingProcess>();
        GetTree().Root.AddChild(saveProcessSceneInstance);

        var serializer = new Serializer();
        var saveYaml = serializer.Serialize(Settings);

        var file = Godot.FileAccess.Open(GalatimeConstants.settingsPath, Godot.FileAccess.ModeFlags.Write);
        var error = Godot.FileAccess.GetOpenError();
        if (error != Error.Ok)
        {
            saveProcessSceneInstance.PlayFailedAnimation();
            GD.Print("Error when saving a config: " + Godot.FileAccess.GetOpenError().ToString());
        }
        else
        {
            file.StoreString(saveYaml);
            file.Close();
        }
    }

    public void UpdateSettings()
    {
        // Audio volume
        AudioServer.SetBusVolumeDb(0, (float)Settings.MasterVolume);
        AudioServer.SetBusVolumeDb(1, (float)Settings.MusicVolume);

        // Discord RPC
        var DiscordController = GetNode<DiscordController>("/root/DiscordController");
        if (Settings.DiscordActivityDisabled) DiscordController.Client.ClearPresence();
        // Restoring the previous presence when not Discord RPC is not disabled.
        else DiscordController.CurrentRichPresence = DiscordController.CurrentRichPresence;
    }
}