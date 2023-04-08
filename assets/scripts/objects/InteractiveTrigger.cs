using Godot;
using System;
using Galatime;

namespace Galatime
{
    public partial class InteractiveTrigger : Node2D
    {
        [Export] NodePath visualNode;
        [Export] NodePath executeNode;
        [Export] string method;
        [Export] string[] args = new string[0];
        [Export] bool changeState = true;

        [Signal] public delegate void dialogEventHandler(string id);

        public bool canInteract = true;

        private Node2D _node;
        private Node _executeNode;

        private Area2D _collisionArea;
        private ShaderMaterial _shader;

        private Player _playerNode;
        public override void _Ready()
        {
            _collisionArea = GetNode<Area2D>("CollisionArea");
            _collisionArea.Connect("body_entered",new Callable(this,"_onEntered"));
            _collisionArea.Connect("body_exited",new Callable(this,"_onExit"));

            _shader = GD.Load<ShaderMaterial>("res://assets/shaders/outline.tres").Duplicate() as ShaderMaterial;
            _playerNode = GetTree().GetNodesInGroup("player")[0] as Player;

            _playerNode.Connect("on_interact",new Callable(this,"_OnInteract"));
            _playerNode.Connect("on_dialog_end",new Callable(this,"_OnDialogEnd"));

            _node = GetNode<Node2D>(visualNode);
            _executeNode = GetNode<Node>(executeNode);

            GD.Print(changeState + " fawfawfawf " + method);
        }

        public void _onEntered(Node node)
        {
            if (node is Player)
            {
                GD.PrintRich("INTERACTIVE TRIGGER: [color=green]Player entered[/color]");

                _interactShaderInterpolate(0, 0.02f, 0.1f);
            }
        }

        public void _onExit(Node node)
        {
            if (canInteract)
            {
                if (node is Player)
                {
                    GD.PrintRich("INTERACTIVE TRIGGER: [color=aqua]Player exited[/color]");
                    _interactShaderInterpolate(0.02f, 0, 0.1f);
                }
            }
            if (changeState) canInteract = true;
        }

        private void _interactShaderInterpolate(float from, float to, float durationSec)
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(_shader, "shader_parameter/precision",
            to, durationSec);
            _node.Material = _shader;
        }

        public void _OnInteract()
        {
            if (canInteract)
            {
                Godot.Collections.Array<Node2D> bodies = _collisionArea.GetOverlappingBodies();
                if (bodies.Contains(_playerNode.body))
                {
                    if (_executeNode.HasMethod(method))
                    {
                        if (args.Length > 0)
                        {
                            _executeNode.Call(method, args); 
                            GD.PrintRich("INTERACTIVE TRIGGER: [color=aqua]Called method with multiple args[/color]");
                        }
                        else
                        {
                            _executeNode.Call(method);
                            GD.PrintRich("INTERACTIVE TRIGGER: [color=aqua]Called method without args[/color]");
                        }
                        if (changeState) canInteract = false;
                        if (changeState) _interactShaderInterpolate(0.02f, 0, 0.1f);
                        GD.PrintRich("INTERACTIVE TRIGGER: [color=green]Interacted successful![/color]");
                    }
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

