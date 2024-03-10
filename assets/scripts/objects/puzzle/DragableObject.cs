using Galatime;
using Galatime.Global;
using Galatime.Helpers;
using Galatime.Interfaces;
using Godot;

namespace Galatime.Puzzle;

public partial class DragableObject : CharacterBody2D, ILevelObject
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
    /// <summary> Returns true if the object is currently being dragged. You can set this to true to start dragging or false to stop. </summary>
    public bool IsDragging
    {
        get => isDragging;
        set
        {
            isDragging = value;
            if (CurrentCharacter != null) CurrentCharacter.IsPushing = value;
            if (!value) 
            {
                LevelManager.Instance.SaveLevelObject(this, new object[] { GlobalPosition });
                CurrentCharacter = null;
            }
            InteractiveTrigger.InteractText = IsDragging ? "Stop Pushing" : "Push";
        }
    }

    public override void _Ready()
    {
        #region Get nodes
        InteractiveTrigger = GetNode<InteractiveTrigger>("InteractiveTrigger");
        PushingAudio = GetNode<AudioStreamPlayer2D>("PushingAudio");
        Sprite = GetNode<Sprite2D>("Sprite");
        #endregion

        InteractiveTrigger.OnInteract += OnInteract;
        InteractiveTrigger.OnAreaAction += OnAreaAction;
        InteractiveTrigger.DisableIf = () => CurrentCharacter != null;
    }

    public void LoadLevelObject(object[] state)
    {
        var position = (Vector2)state[0];
        GlobalPosition = position;
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
            if (PlayerVariables.Instance.Player.IsPlayerFrozen && IsDragging) // Don't move if the player is frozen.
                IsDragging = false;

            // If the character is pushing and moving.
            if (Velocity.Length() > 0)
            {
                // Play pushing audio
                if (!PushingAudio.Playing)
                    PushingAudio.Play();
                PushingAudio.VolumeDb = 0;

                // Shake looks cool.
                PullShake.Enabled = true;
                PullShake.ShakeProcess(delta);
                Sprite.Position = PullShake.ShakenVector;
            }
            else
            {
                // Otherwise stop the audio
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
