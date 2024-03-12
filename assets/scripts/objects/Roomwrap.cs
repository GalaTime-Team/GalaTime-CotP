namespace Galatime;

using Galatime.Global;
using Godot;
using NodeExtensionMethods;


/// <summary>
/// Represents a trigger, which transitions to a new room (Scene)
/// </summary>
/// <remarks> Don't confuse with <see cref="GalatimeGlobals.LoadScene(string)"/>, because it's loads a scene, but that node is trigger for the room transition </remarks>
[Tool] public partial class Roomwrap : Node2D
{
    private string scene;
    [Export(PropertyHint.File, "*.tscn")] public string Scene
    {
        get => scene;
        set
        {
            scene = value;
            UpdateConfigurationWarnings();
        }
    }
    [Export] public float AnimationDuration = 0.5f;
    /// <summary> Determines the spawn point of the player in the next room. </summary>
    [Export(PropertyHint.Range, "0,255,1")] public byte PlayerSpawnPoint = 0;
    public Color CustomColor;
    private Area2D TriggerArea;

    public override void _Ready()
    {
        TriggerArea = GetNode<Area2D>("TriggerArea");
        TriggerArea.BodyEntered += OnEnter;
    }

    public override void _ExitTree() 
    {
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
        LevelManager.Instance.PlayerSpawnPointIndex = PlayerSpawnPoint;
        var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
        globals.LoadScene(Scene);
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (Scene.Length == 0)
            return new string[] { "Please specify a scene or it will not be loaded" };
        else
            return System.Array.Empty<string>();
    }
}