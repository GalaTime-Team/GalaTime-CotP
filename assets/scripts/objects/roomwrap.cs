namespace Galatime;

using Godot;
using NodeExtensionMethods;


/// <summary>
/// Represents a trigger, which transitions to a new room (Scene)
/// </summary>
/// <remarks>Don't confuse with <see cref="GalatimeGlobals.LoadScene(string)"/>, because it's loads a scene, but that node is trigger for the room transition</remarks>
public partial class Roomwrap : Node2D
{
    [Export(PropertyHint.File)] public string Scene;
    [Export] public float AnimationDuration = 0.5f;
    public Color CustomColor;
    private Area2D TriggerArea;

    public override void _Ready()
    {
        TriggerArea = GetNode<Area2D>("TriggerArea");
        TriggerArea.BodyEntered += OnEnter;
    }

    public override void _ExitTree() {
        TriggerArea.BodyEntered -= OnEnter;
    }

    private void OnEnter(Node node)
    {
        // if (node is Player p)
        if (node.IsPossessed())
        {
            var p = node as TestCharacter;
            p.CanMove = false;
            PlayerVariables.Instance.Player.PlayerGui.OnFade(true, AnimationDuration, OnFadeEnded);
        }
    }
    private void OnFadeEnded()
    {
        var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
        globals.LoadScene(Scene);
    }
}