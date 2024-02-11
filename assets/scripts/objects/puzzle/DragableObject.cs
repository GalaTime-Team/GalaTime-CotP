using Galatime;
using Galatime.Helpers;
using Godot;

public partial class DragableObject : CharacterBody2D
{
    GameLogger Logger = new("DragableObject", GameLogger.ConsoleColor.Orange);

    public InteractiveTrigger InteractiveTrigger;
    public AudioStreamPlayer2D PushingAudio;
    public Sprite2D Sprite;

    private static TestCharacter CurrentCharacter;

    public VectorShaker PullShake = new()
    {
        ShakeAmplitude = new(5f, 5f)
    };
    private bool isDragging = false;
    private bool IsDragging
    {
        get => isDragging;
        set
        {
            isDragging = value;
            if (CurrentCharacter != null) CurrentCharacter.IsPushing = value;
            if (!value) CurrentCharacter = null;
            InteractiveTrigger.InteractText = IsDragging ? "Stop Pushing" : "Push";
        }
    }

    public override void _Ready()
    {
        InteractiveTrigger = GetNode<InteractiveTrigger>("InteractiveTrigger");
        PushingAudio = GetNode<AudioStreamPlayer2D>("PushingAudio");
        Sprite = GetNode<Sprite2D>("Sprite");

        InteractiveTrigger.OnInteract += OnInteract;
        InteractiveTrigger.OnAreaAction += OnAreaAction;
        InteractiveTrigger.DisableIf = () => CurrentCharacter != null;
    }

    public void OnInteract(TestCharacter character)
    {
        Logger.Log($"Interacted with {character.Name}", GameLogger.LogType.Success);
        CurrentCharacter = character;
        IsDragging = !IsDragging;
    }

    public void OnAreaAction(bool entered)
    {
        Logger.Log($"Area action: {entered}");
        if (!entered && IsDragging) IsDragging = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDragging)
        {
            if (Velocity.Length() > 0)
            {
                if (!PushingAudio.Playing)
                    PushingAudio.Play();

                PushingAudio.VolumeDb = 0;
                PullShake.Enabled = true;
                PullShake.ShakeProcess(delta);
                Sprite.Position = PullShake.ShakenVector;
            }
            else
            {
                if (PushingAudio.Playing) PushingAudio.VolumeDb = -80;
            }
            Velocity = CurrentCharacter != null ? CurrentCharacter.Velocity : Vector2.Zero;
            MoveAndSlide();
        }
        else
        {
            if (PushingAudio.Playing) PushingAudio.Stop();
        }
    }
}
