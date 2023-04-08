using Godot;
using System;

namespace Galatime 
{
    public partial class Loading : CanvasLayer
    {
        public string sceneName = "res://assets/scenes/MainMenu.tscn";
        Godot.Collections.Array progress = new Godot.Collections.Array();

        ProgressBar progressBar;
        AnimationPlayer animationPlayer;
        RichTextLabel tipsLabel;

        public override void _Ready()
        {
            progressBar = GetNode<ProgressBar>("Control/ProgressBar");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            tipsLabel = GetNode<RichTextLabel>("Control/TipsLabel");

            ResourceLoader.LoadThreadedRequest(sceneName);

            animationPlayer.Play("loading");

            // tipsLabel.Text = $"Tip: {(string)GalatimeGlobals.tipsList.PickRandom()}";
        }

        public override void _Process(double delta)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(sceneName, progress);
            GD.Print(progress + " " + status + " Scene name: " + sceneName);  
            progressBar.Value = (float)progress[0] * 100;
            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                GetTree().ChangeSceneToPacked((PackedScene)ResourceLoader.LoadThreadedGet(sceneName));
                QueueFree();
            }
        }
    }

}
