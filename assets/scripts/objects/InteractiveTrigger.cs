using Godot;
using System;
using Galatime;

public class InteractiveTrigger : Node2D
{
    [Export] NodePath visualNode;

    public bool canInteract = true;

    private Node2D _node;
    private Area2D _collisionArea;
    private ShaderMaterial _shader;
    private KinematicBody2D _playerBody;
    private Node2D _playerNode;

    private Tween tween;
    public override void _Ready()
    {
        _collisionArea = GetNode<Area2D>("CollisionArea");
        _collisionArea.Connect("body_entered", this, "_onEntered");
        _collisionArea.Connect("body_exited", this, "_onExit");

        _shader = GD.Load<ShaderMaterial>("res://assets/shaders/outline.tres");
        _playerBody = GetNode<KinematicBody2D>(Galatime.GalatimeConstants.playerBodyPath);
        _playerNode = GetNode<Node2D>(Galatime.GalatimeConstants.playerPath);

        _playerNode.Connect("on_interact", this, "_OnInteract");

        _node = GetNode<Node2D>(visualNode);
        tween = GetNode<Tween>("Tween");
    }

    public void _onEntered(Node node)
    {
        if (node == _playerBody)
        {
            _interactShaderInterpolate(0, 0.02f, 0.1f);
        }
    }

    public void _onExit(Node node)
    {
        if (canInteract)
        {
            if (node == _playerBody)
            {
                _interactShaderInterpolate(0.02f, 0, 0.1f);
            }
        }
        canInteract = true;
    }

    private void _interactShaderInterpolate(float from, float to, float durationSec)
    {
        tween.InterpolateProperty(_shader, "shader_param/precision",
        from, to, durationSec,
        Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        _node.Material = _shader;
    }

    public void _OnInteract()
    {
        if (canInteract)
        {
            Godot.Collections.Array bodies = _collisionArea.GetOverlappingBodies();
            if (bodies.Contains(_playerBody))
            {
                _playerNode.Call("startDialog", "test_1");
                canInteract = false;
                _interactShaderInterpolate(0.02f, 0, 0.05f);
            }
        }
    }
}
