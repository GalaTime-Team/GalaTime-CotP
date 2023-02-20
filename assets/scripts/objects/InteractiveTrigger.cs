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
        [Export] bool changeState = true;

        [Signal] delegate void dialog(string id);

        public bool canInteract = true;

        private Node2D _node;
        private Node _executeNode;

        private Area2D _collisionArea;
        private ShaderMaterial _shader;

        private Player _playerNode;

        private Tween tween;
        public override void _Ready()
        {
            tween = GetNode<Tween>("Tween");

            _collisionArea = GetNode<Area2D>("CollisionArea");
            _collisionArea.Connect("body_entered", this, "_onEntered");
            _collisionArea.Connect("body_exited", this, "_onExit");

            _shader = GD.Load<ShaderMaterial>("res://assets/shaders/outline.tres").Duplicate() as ShaderMaterial;
            _playerNode = PlayerVariables.player;

            _playerNode.Connect("on_interact", this, "_OnInteract");
            _playerNode.Connect("on_dialog_end", this, "_OnDialogEnd");

            _node = GetNode<Node2D>(visualNode);
            _executeNode = GetNode<Node>(executeNode);

            GD.Print(changeState + " fawfawfawf " + method);
        }

        public void _onEntered(Node node)
        {
            if (node == _playerNode.body)
            {
                _interactShaderInterpolate(0, 0.02f, 0.1f);
            }
        }

        public void _onExit(Node node)
        {
            if (canInteract)
            {
                if (node == _playerNode.body)
                {
                    _interactShaderInterpolate(0.02f, 0, 0.1f);
                }
            }
            if (changeState) canInteract = true;
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
                if (bodies.Contains(_playerNode.body))
                {
                    GD.Print(_executeNode.HasMethod(method));
                    _executeNode.Call(method, args);
                    if (changeState) canInteract = false;
                    if (changeState) _interactShaderInterpolate(0.02f, 0, 0.1f);
                }
            }
        }
        public async void _OnDialogEnd()
        {
            GD.Print("end");
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            if (changeState) canInteract = true;
        }
    }
}

