using Godot;
using System;
using Galatime;

namespace Galatime
{
    public class InteractiveTrigger : Node2D
    {
        [Export] NodePath visualNode;
        [Export] NodePath executeNode;
        [Export] string method;
        [Export] string[] args;  

        [Signal] delegate void dialog(string id);

        public bool canInteract = true;

        private Node2D _node;
        private Node2D _executeNode;

        private Area2D _collisionArea;
        private ShaderMaterial _shader;

        private KinematicBody2D _playerBody;
        private Node2D _playerNode;

        private Tween tween;
        public override void _Ready()
        {
            tween = GetNode<Tween>("Tween");

            _collisionArea = GetNode<Area2D>("CollisionArea");
            _collisionArea.Connect("body_entered", this, "_onEntered");
            _collisionArea.Connect("body_exited", this, "_onExit");

            _shader = GD.Load<ShaderMaterial>("res://assets/shaders/outline.tres");
            _playerBody = GetNode<KinematicBody2D>(Galatime.GalatimeConstants.playerBodyPath);
            _playerNode = GetNode<Node2D>(Galatime.GalatimeConstants.playerPath);

            _playerNode.Connect("on_interact", this, "_OnInteract");

            _node = GetNode<Node2D>(visualNode);
            _executeNode = GetNode<Node2D>(executeNode);
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
                    GD.Print(_executeNode.HasMethod(method));
                    _executeNode.Call(method, args);
                }
            }
        }
    }
}

