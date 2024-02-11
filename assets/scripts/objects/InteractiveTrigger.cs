using Godot;
using NodeExtensionMethods;
using System;
using System.Linq;

namespace Galatime;

/// <summary> This node can create interactive triggers that can call methods on other nodes. </summary>
/// <remarks> To made this node work you should assign <see cref="VisualNodePath"/>, <see cref="ExecuteNodePath"/>, <see cref="Method"/> to work. </remarks>
public partial class InteractiveTrigger : Node2D
{
    /// <summary> The path to the node that shows the visual representation of the trigger. </summary>
    [Export] NodePath VisualNodePath;
    /// <summary> The path to the node that will execute the method when the trigger is activated. </summary>
    [Export] NodePath ExecuteNodePath;
    /// <summary> The name of the method that will be called on the ExecuteNode. </summary>
    [Export] string Method;
    /// <summary> The arguments that will be passed to the method. </summary>
    [Export] string[] Args = Array.Empty<string>();
    /// <summary> Whether the trigger should change its state (CanInteract) after each activation. </summary>
    [Export] bool ChangeState = true;
    private string interactText = "Interact";
    /// <summary> The text that will be shown when the player hovers over the trigger. </summary>
    [Export]
    public string InteractText
    {
        get => interactText;
        set
        {
            interactText = value;

            if (InteractLabel != null) PlaceInteractLabel();
        }
    }
    [Export] float InteractTextPositionOffset = 10f;

    /// <summary> Whether the trigger can be interacted with or not. </summary>
    public bool CanInteract = true;
    private bool playerIsHovering = false;
    public bool PlayerIsHovering
    {
        get => playerIsHovering;
        set
        {
            // We don't need to do anything if the value is the same.
            if (value == playerIsHovering)
            {
                playerIsHovering = value;
                return;
            }
            playerIsHovering = value;

            if (playerIsHovering)
            {
                SetTween();
                InterpolateOutline(.02f, .2f);
                InterpolateTextAlpha(1f, .2f);

                InteractLabel.Visible = true;
                PlaceInteractLabel();
            }
            else
            {
                SetTween();
                InterpolateOutline(0f, .2f);
                InterpolateTextAlpha(0f, .2f).Finished += () => InteractLabel.Visible = false;
            }
        }
    }
    /// <summary> Called when the trigger is interacted with. </summary>
    public Action<TestCharacter> OnInteract;
    /// <summary> Called when the trigger is entered. Is false if exited, or true if entered. </summary>
    public Action<bool> OnAreaAction;
    /// <summary> If the trigger should be disabled, by the this condition. </summary>
    public Func<bool> DisableIf;
    /// <summary> The character that is interacting with the trigger. </summary>
    private TestCharacter CurrentCharacter;
    private Tween Tween;

    /// <summary> The node, that shows the outline of the trigger and the text. </summary>
    private Node2D VisualNode;
    private Node ExecuteNode;

    private Area2D TriggerArea;
    private ShaderMaterial OutlineShader;
    private Label InteractLabel;

    public override void _Ready()
    {
        TriggerArea = GetNode<Area2D>("CollisionArea");
        TriggerArea.BodyEntered += OnEntered;
        TriggerArea.BodyExited += OnExit;

        OutlineShader = GD.Load<ShaderMaterial>("res://assets/shaders/outline.tres").Duplicate() as ShaderMaterial;
        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

        VisualNode = GetNodeOrNull<Node2D>(VisualNodePath);
        ExecuteNode = GetNodeOrNull<Node>(ExecuteNodePath);
        InteractLabel = GetNodeOrNull<Label>("Label");
    }

    private void SetTween()
    {
        if (Tween != null) Tween?.Kill();
        Tween = GetTree().CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic);
    }

    /// <summary> Called when a player enters a node. </summary>
    public void OnEntered(Node node)
    {
        if (node.IsPossessed() && CanInteract && DisableIf?.Invoke() == false)
        {
            CurrentCharacter = node as TestCharacter;
            PlayerIsHovering = true;
        }

        OnAreaAction?.Invoke(PlayerIsHovering);
    }

    /// <summary> Called when a player exits a node. </summary>
    public void OnExit(Node node)
    {
        if (node.IsPossessed())
        {
            CurrentCharacter = null;
            PlayerIsHovering = false;
        }

        OnAreaAction?.Invoke(PlayerIsHovering);
    }

    public override void _Process(double delta)
    {
        if (CurrentCharacter != null && !CurrentCharacter.IsPossessed())
        {
            CurrentCharacter = null;
            PlayerIsHovering = false;
            OnAreaAction?.Invoke(PlayerIsHovering);
        }
    }

    /// <summary> Places the interact label on the node. </summary>
    public void PlaceInteractLabel()
    {
        if (VisualNode is Sprite2D sprite)
        {
            // Get the size of the texture. Since the sprite may be scaled, we need to multiply it by the scale.
            var size = sprite.Texture.GetSize() * sprite.Scale;

            InteractLabel.Text = InteractText;

            // Some calculations to place the label correctly. It places it above the sprite.
            InteractLabel.Size = Vector2.Zero; // Reset the size of the label, because we need to fit text in it.
            InteractLabel.Size = new Vector2(size.X, InteractLabel.Size.Y);
            InteractLabel.GlobalPosition = sprite.GlobalPosition - new Vector2(size.X, (size.Y / 2) + InteractLabel.Size.Y * 2 + InteractTextPositionOffset);
        }
    }

    /// <summary> Interpolates the outline of a <see cref="VisualNode"/> over a specified duration. </summary>
    private void InterpolateOutline(float to, float durationSec)
    {
        Tween.TweenProperty(OutlineShader, "shader_parameter/precision", to, durationSec);
        VisualNode.Material = OutlineShader;
    }

    private MethodTweener InterpolateTextAlpha(float to, float durationSec) => Tween.TweenMethod(Callable.From<float>(x => InteractLabel.Modulate = new Color(1, 1, 1, x)), InteractLabel.Modulate.A, to, durationSec);

    /// <summary> Handles player interactions with the node. </summary>
    public void Interact()
    {
        if (CanInteract && ExecuteNode != null && ExecuteNode.HasMethod(Method))
        {
            var args = new Variant[Args.Length];
            for (int i = 0; i < Args.Length; i++) args[i] = Args[i];

            if (args.Length > 0)
                ExecuteNode?.Call(Method, args);
            else
                ExecuteNode?.Call(Method);

            CallInteract();
        }
        else if (CanInteract) CallInteract();
    }

    private void CallInteract()
    {
        if (ChangeState)
        {
            CanInteract = false;
            PlayerIsHovering = false;
        }
        OnInteract?.Invoke(CurrentCharacter);
    }

    /// <summary> Called when a dialog ends. </summary>
    public async void OnDialogEnd()
    {
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        if (ChangeState) CanInteract = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") && PlayerIsHovering) Interact();
    }
}