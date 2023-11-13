using Godot;
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
    [Export] string[] Args = System.Array.Empty<string>();
    /// <summary> Whether the trigger should change its state (CanInteract) after each activation. </summary>
    [Export] bool ChangeState = true;

    /// <summary> Whether the trigger can be interacted with or not. </summary>
    public bool CanInteract = true;
    public bool PlayerIsHovering;

    private Node2D VisualNode;
    private Node ExecuteNode;

    private Area2D TriggerArea;
    private ShaderMaterial OutlineShader;

    private Player Player;

    public override void _Ready()
    {
        TriggerArea = GetNode<Area2D>("CollisionArea");
        TriggerArea.BodyEntered += OnEntered;
        TriggerArea.BodyExited += OnExit;

        OutlineShader = GD.Load<ShaderMaterial>("res://assets/shaders/outline.tres").Duplicate() as ShaderMaterial;
        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        Player = playerVariables.Player;

        VisualNode = GetNode<Node2D>(VisualNodePath);
        ExecuteNode = GetNode<Node>(ExecuteNodePath);
    }

    /// <summary> Called when a player enters a node. </summary>
    public void OnEntered(Node node)
    {
        if (node is Player player)
        {
            PlayerIsHovering = true;
            Player = player;

            GD.PrintRich("INTERACTIVE TRIGGER: [color=green]Player entered[/color]");
            InterpolateOutline(0.02f, 0.1f);
        }
    }

    /// <summary> Called when a player exits a node. </summary>
    public void OnExit(Node node)
    {
        if (CanInteract && node is Player)
        {
            PlayerIsHovering = false;
            Player = null;

            GD.PrintRich("INTERACTIVE TRIGGER: [color=aqua]Player exited[/color]");
            InterpolateOutline(0, 0.1f);
        }
        if (ChangeState) CanInteract = true;
    }
            /// <summary> Interpolates the outline of a <see cref="VisualNode"/> over a specified duration. </summary>
    private void InterpolateOutline(float to, float durationSec)
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(OutlineShader, "shader_parameter/precision", to, durationSec);
        VisualNode.Material = OutlineShader;
    }

    /// <summary> Handles player interactions with the node. </summary>
    public void OnInteract()
    {
        if (CanInteract && ExecuteNode.HasMethod(Method))
        {
            var args = new Variant[Args.Length]; Array.Copy(Args, args, Args.Length);
            if (args.Length > 0)
            {
                ExecuteNode.Call(Method, args);
                GD.PrintRich("INTERACTIVE TRIGGER: [color=aqua]Called method with multiple args[/color]");
            }
            else
            {
                ExecuteNode.Call(Method);
                GD.PrintRich("INTERACTIVE TRIGGER: [color=aqua]Called method without args[/color]");
            }
            if (ChangeState)
            {
                CanInteract = false;
                InterpolateOutline(0, 0.1f);
            }
            GD.PrintRich("INTERACTIVE TRIGGER: [color=green]Interacted successful![/color]");
        }
    }

    /// <summary> Called when a dialog ends. </summary>
    public async void OnDialogEnd()
    {
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        if (ChangeState) CanInteract = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") && PlayerIsHovering) OnInteract();
    }
}