using Discord;
using Galatime;
using Godot;
using System;

public partial class test : Node2D
{
    public override void _Ready()
    {
        PlayerVariables.player.startDialog("test_5");
        //if (!GalatimeGlobals.DiscordActivityDisabled)
        //{
        //    GalatimeGlobals.currentActivity = new Activity()
        //    {
        //        Assets =
        //    {
        //        LargeImage = "default",
        //        LargeText = "GalaTime " + GalatimeConstants.version,
        //        SmallImage = "day_1",
        //        SmallText = "1st day"
        //    },
        //        Timestamps =
        //    {
        //        Start = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
        //    },
        //        State = "In Play",
        //        Details = ""
        //    };

        //    GalatimeGlobals.activityManager.UpdateActivity(GalatimeGlobals.currentActivity, (res) =>
        //    {
        //        if (res == Discord.Result.Ok)
        //        {
        //            GD.Print("Everything is fine!");
        //        }
        //    });
        //}

        GetTree().Root.Title = "GalaTime - Test room";
    }
}
