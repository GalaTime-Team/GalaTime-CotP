using System;
using Galatime.Settings;
using Godot;
using YamlDotNet.Serialization;

namespace Galatime.Global;

public partial class SettingsGlobals : Node
{
    public static SettingsGlobals Instance { get; private set; }
    public static SettingsData Settings = new();

    public override void _Ready()
    {
        Instance = this;
        LoadSettings();
    }

    /// <summary> Loads the settings from the settings.yml file. </summary>
    /// <returns> As option, returns loaded settings, otherwise use static type to get settings. </returns>
    public Galatime.Settings.SettingsData LoadSettings()
    {
        var file = Godot.FileAccess.Open(GalatimeConstants.SettingsPath, Godot.FileAccess.ModeFlags.Read);

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
        var saveProcessSceneInstance = GalatimeGlobals.SaveProcessScene.Instantiate<SavingProcess>();
        GetTree().Root.AddChild(saveProcessSceneInstance);

        var serializer = new Serializer();
        var saveYaml = serializer.Serialize(Settings);

        var file = Godot.FileAccess.Open(GalatimeConstants.SettingsPath, Godot.FileAccess.ModeFlags.Write);
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
        #region Audio

        // The ease function used in the volume calculation. Cubic function because it is more smooth and less abrupt.
        double EaseOutCubic(double x) => 1 - Math.Pow(1 - x, 3);
        // Clamp to avoid going above 0dB, it could be painful for your ears if you do more than 0dB.
        float CalculateVolume(double value) => (float)Mathf.Clamp(-80f + EaseOutCubic(value) * 80f, -80f, 0f);

        // Audio volume
        AudioServer.SetBusVolumeDb(0, CalculateVolume(Settings.Audio.MasterVolume));
        AudioServer.SetBusVolumeDb(1, CalculateVolume(Settings.Audio.MusicVolume));

        #endregion

        #region Video

        void CenterWindow(Window window) => window.Position = DisplayServer.ScreenGetSize() / 2 - window.Size / 2;

        // Fullscreen mode
        var windowMode = Settings.Video.Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
        if (windowMode != DisplayServer.WindowGetMode())
            DisplayServer.WindowSetMode(windowMode);

        // Resolution
        var res = Settings.Video.Resolution;
        var window = GetWindow();

        if (res != "Current Resolution")
        {
            // Try to parse the resolution.
            var resSplit = res.Split('x');

            var x = int.Parse(resSplit[0]);
            var y = int.Parse(resSplit[1]);

            if (window.Size.X != x || window.Size.Y != y)
            {
                window.Size = new Vector2I(x, y);
                CenterWindow(window);
            }
        }
        else
        {
            window.Size = DisplayServer.ScreenGetSize();
            CenterWindow(window);
        }

        // Max FPS
        if (Settings.Video.MaxFps != Engine.MaxFps.ToString() || (Settings.Video.MaxFps == "No Limit" && Engine.MaxFps != 0))
        {
            if (Settings.Video.MaxFps == "No Limit")
                Engine.MaxFps = 0; // 0 means no limit.
            else
                // TryParse just in case it's not a number or something else. Fps not gonna be limited if it's not a number.
                Engine.MaxFps = int.TryParse(Settings.Video.MaxFps, out int maxFps) ? maxFps : 0;
        }

        // VSync
        var vsync = Settings.Video.Vsync ? DisplayServer.VSyncMode.Adaptive : DisplayServer.VSyncMode.Disabled;
        if (vsync != DisplayServer.WindowGetVsyncMode())
            DisplayServer.WindowSetVsyncMode(vsync);

        #endregion

        #region Misc

        // Discord RPC
        var discordController = DiscordController.Instance;
        if (Settings.Misc.DiscordActivityDisabled) discordController.Client.ClearPresence();
        // Restoring the previous presence when not Discord RPC is not disabled.
        else discordController.CurrentRichPresence = discordController.CurrentRichPresence;

        #endregion
    }
}