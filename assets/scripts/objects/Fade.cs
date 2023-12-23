using Godot;

public partial class fade : Node2D
{
    // Variables
    private string _fadeType = "out";

    // Nodes
    private AnimationPlayer _animation;
    private Node2D _player;
    private Sprite2D _fade;

    public override void _Ready()
    {
        // Get Nodes
        _animation = GetNode<AnimationPlayer>("animation");
        _fade = GetNode<Sprite2D>("fade");
        _player = GetParent<Node2D>();

        // Connect
        _player.Connect("fade", new Callable(this, "_Fade"));
        _animation.Connect("animation_finished", new Callable(this, "setVisibility"));

        // Start
        _fade.Visible = true;
        _animation.Play("fade_out");
    }

    public override void _Draw()
    {
        base._Draw();
    }
    public void _Fade(string type)
    {
        _fadeType = type;
        switch (type)
        {
            case "in":
                GD.Print("in");
                _animation.Play("fade_in");
                break;
            case "out":
                GD.Print("out");
                _animation.Play("fade_out");
                break;
        }
    }

    public void setVisibility(string type)
    {
        switch (type)
        {
            case "in":
                GD.Print("in visibility");
                _fade.Modulate = new Color(1, 1, 1, 1);
                break;
            case "out":
                GD.Print("out visibility");
                _fade.Modulate = new Color(1, 1, 1, 0);
                break;
        }
    }
}
