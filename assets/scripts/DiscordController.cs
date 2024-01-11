using DiscordRPC;
using Godot;
using System;

namespace Galatime.Global;

/// <summary> A singleton that handles the Discord Rich Presence integration for the game. Use <see cref="Client"/> to interact with Discord RPC. </summary>
public partial class DiscordController : Node
{
    public static DiscordController Instance { get; private set; }

    /// <summary> The Discord RPC client instance. </summary>
    public DiscordRpcClient Client;
    private RichPresence currentRichPresence;
    public RichPresence CurrentRichPresence
    {
        get => currentRichPresence;
        set => SetCurrentRichPresence(value);
    }

    public void SetCurrentRichPresence(RichPresence value, bool update = true)
    {
        currentRichPresence = value;
        if (!SettingsGlobals.Settings.Misc.DiscordActivityDisabled && update) Client.SetPresence(currentRichPresence);
    }

    public override void _Ready()
    {
        Instance = this;

        // Creating a new Discord client with the given ID and setting auto events to false to able to sync with Main Loop.
        Client = new DiscordRpcClient(GalatimeConstants.DISCORD_ID, autoEvents: false);

        // Setting up events to log.
        Client.OnReady += (_, e) => GD.PrintRich($"[color=cyan]DISCORD[/color]: RPC is ready! Hello, {e.User.Username}!");
        Client.OnPresenceUpdate += (_, e) =>
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append("[color=cyan]DISCORD[/color]: Updated.");
            if (!string.IsNullOrEmpty(e.Presence.Details)) stringBuilder.Append(" Details: ").Append(e.Presence.Details);
            if (!string.IsNullOrEmpty(e.Presence.State)) stringBuilder.Append(", State: ").Append(e.Presence.State);
            GD.PrintRich(stringBuilder.ToString());
        };

        // Connecting to the Discord.
        Client.Initialize();

        // Setting up presence.
        SetCurrentRichPresence(new RichPresence()
        {
            State = "Playing",

            // "XX:XX elapsed"
            Timestamps = new Timestamps() { Start = DateTime.UtcNow },

            Assets = new Assets()
            {
                LargeImageKey = "default",
                LargeImageText = $"GalaTime {GalatimeConstants.Version}",
            }
        }, false);
    }

    // Invoke Client each frame to update presence.
    public override void _Process(double delta) => Client.Invoke();
}
