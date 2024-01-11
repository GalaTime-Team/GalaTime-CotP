using Godot;

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

            ResourceLoader.LoadThreadedRequest(sceneName, "", false, ResourceLoader.CacheMode.Replace);

            animationPlayer.Play("loading");
            // tipsLabel.Text = $"Tip: {(string)GalatimeGlobals.tipsList.PickRandom()}";
        }

        public override void _Process(double delta)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(sceneName, progress);
            GD.Print($"{(float)progress[0] * 100}% - {status}. Scene: {sceneName}");
            progressBar.Value = (float)progress[0] * 100;
            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                var instance = (PackedScene)ResourceLoader.LoadThreadedGet(sceneName);
                GetTree().ChangeSceneToPacked(instance);
                GetTree().Paused = false;
                QueueFree();
                // GetTree().Root.AddChild(instance);
                // GetTree().CurrentScene = instance;
                // animationPlayer.Play("end");
            }
        }
    }

}
