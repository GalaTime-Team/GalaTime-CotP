using Godot;
using Galatime.Puzzle;

public partial class DraggableObjectPedestal : Area2D
{
    GameLogger Logger = new GameLogger("DraggableObjectPedestal", GameLogger.ConsoleColor.Cyan);
    public Sprite2D Sprite;
    public AudioStreamPlayer2D ActivateAudio;

    public PuzzleActivator puzzleActivator;
    /// <summary> The puzzle activator that should be activated when this object is activated. </summary>
    [Export] public PuzzleActivator PuzzleActivator
    {
        get => puzzleActivator;
        set
        {
            puzzleActivator = value;
            if (value != null) value.Conditions.Add(() => Activated);
        }
    }
    /// <summary> The position offset where DraggableObject will be placed. By default is center of the object. </summary>
    [Export] public Vector2 PositionOffset = Vector2.Zero;
    public DragableObject CurrentDraggableObject;

    private bool activated;
    /// <summary> Is this object activated? </summary>
    public bool Activated
    {
        get => activated;
        set
        {
            activated = value;

            // Change sprite color based on activation
            if (IsInstanceValid(Sprite)) Sprite.Modulate = value ? new Color(1f, 1f, 1f) : new Color(1f, 0f, 0f);
            ActivateAudio.Play();

            if (activated) PuzzleActivator?.Activate();

            Logger.Log($"Activated: {value}", GameLogger.LogType.Info);
        }
    }

    public override void _Ready()
    {
        #region Get nodes
        Sprite = GetNode<Sprite2D>("Sprite");
        ActivateAudio = GetNode<AudioStreamPlayer2D>("ActivateAudio");
        #endregion

        BodyEntered += OnAreaEntered;
    }

    public void OnAreaEntered(Node2D body)
    {
        if (body is DragableObject draggableObject
            && CurrentDraggableObject == null) // To prevent double activation if other object is dragged to this one
        {
            CurrentDraggableObject = draggableObject;

            // Make the object is not draggable while it's being dragged
            draggableObject.IsDragging = false;
            draggableObject.GlobalPosition = GlobalPosition + PositionOffset;

            // Deactivate the object if it's dragged
            draggableObject.InteractiveTrigger.OnInteract += OnDraggableObjectInteract;

            Activated = true;
        }
    }

    public void OnDraggableObjectInteract(TestCharacter character)
    {
        CurrentDraggableObject.InteractiveTrigger.OnInteract -= OnDraggableObjectInteract;
        Activated = false; CurrentDraggableObject = null;
    }
}