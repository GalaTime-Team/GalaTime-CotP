using Godot;

public partial class DangerNotifierEffect : Node2D
{
    public const string ScenePath = "res://assets/objects/gui/DangerNotifierEffect.tscn";
    public static DangerNotifierEffect GetInstance() => ResourceLoader.Load<PackedScene>(ScenePath).Instantiate<DangerNotifierEffect>();

    public AnimationPlayer AnimationPlayer;
    public AudioStreamPlayer2D Audio;

    public override void _Ready()
    {
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Audio = GetNode<AudioStreamPlayer2D>("Audio");

        Visible = false;
    }

    /// <summary> Starts the effect and makes it visible. </summary>
    public void Start() 
    {
        Visible = true;

        Audio.Play();
        AnimationPlayer.Play("loop");
    }

    /// <summary> Ends the effect and removes it from the scene. </summary>
    public void End() 
    {
        QueueFree();
    }
}
