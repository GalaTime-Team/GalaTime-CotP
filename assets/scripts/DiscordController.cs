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
        set
        {
            currentRichPresence = value;

            var SettingsGlobals = GetNode<SettingsGlobals>("/root/SettingsGlobals");
            if (!SettingsGlobals.Settings.DiscordActivityDisabled) Client.SetPresence(currentRichPresence);
        }
    }

    public override void _Ready()
    {
        Instance = this;

        // Creating a new Discord client with the given ID and setting auto events to false to able to sync with Main Loop.
        Client = new DiscordRpcClient(GalatimeConstants.DISCORD_ID, autoEvents: false);

        // Setting up events to log.
        Client.OnReady += (_, e) => GD.Print($"Discord RPC is ready! Hello, {e.User.Username}!");
        Client.OnPresenceUpdate += (_, e) => GD.Print($"Discord RPC is updated. Details: {e.Presence.Details}, State: {e.Presence.State}");

        // Connecting to the Discord.
        Client.Initialize();

        // Setting up presence.
        CurrentRichPresence = new RichPresence()
        {
            Details = "Day 1",
            State = "Playing",

            // "XX:XX elapsed"
            Timestamps = new Timestamps() { Start = DateTime.UtcNow },

            Assets = new Assets()
            {
                LargeImageKey = "default",
                LargeImageText = $"GalaTime {GalatimeConstants.Version}",
                // SmallImageKey = "day_1",
                // SmallImageText = "Day 1"
            }
        };
    }

    // Invoke Client each frame to update presence.
    public override void _Process(double delta) => Client.Invoke();
}
