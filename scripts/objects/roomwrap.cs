using Godot;
using System;

public class roomwrap : Node2D
{
    // Exports
    [Export (PropertyHint.File)] public string Scene;
    [Export] public string PlayerName;

    // Nodes
    private Area2D _area;

    // Signals
    [Signal] delegate void wrap();

    public override void _Ready()
    {
        // Get Nodes
        _area = GetNode<Area2D>("wrap");
        _area.Connect("body_entered", this, "_onEnter");
    }

    private void _onEnter(Node node)
    {
        node.Connect("wrap", node.GetParent<Node2D>(), "_onWrap");
        node.EmitSignal("wrap");
        if (node.Name == PlayerName)
        {
            _changeScene();
        }
    }

    private void _changeScene()
    {
        SceneTree tree = GetTree();
        GD.Print("To scene " + Scene);
        tree.ChangeScene(Scene);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
