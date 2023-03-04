using Godot;
using System;

namespace Galatime {
    public partial class roomwrap : Node2D
    {
        // Exports
        [Export(PropertyHint.File)] public string Scene;
        [Export] public string PlayerName;

        // Nodes
        private Area2D _area;

        private Node _player;

        // Signals
        [Signal] public delegate void wrapEventHandler();
        [Signal] public delegate void fadeEventHandler(string type);

        public override void _Ready()
        {
            // Get Nodes
            _area = GetNode<Area2D>("wrap");

            _area.Connect("body_entered",new Callable(this,"_onEnter"));

            // _player = GetNode("/root/Node2D/Player");
            // Connect("wrap",new Callable(_player,"_onWrap"));
        }

        private void _onEnter(Node node)
        {
            if (node.Name == PlayerName)
            {
                EmitSignal("wrap");

                Timer delay = new Timer();
                AddChild(delay);

                delay.OneShot = true;
                delay.WaitTime = 1.0f;

                delay.Start();

                delay.Connect("timeout",new Callable(this,"_onDelayTimeout"));
            }
        }
        private void _onDelayTimeout()
        {
            _changeScene();
        }

        private void _changeScene()
        {
            SceneTree tree = GetTree();
            GD.Print("To scene " + Scene);
            tree.ChangeSceneToFile(Scene);
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(double delta)
        //  {
        //      
        //  }
    }
}
