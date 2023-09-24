using Godot;

public partial class SpawnerTest : Marker2D
{
    private const string slimePath = "res://assets/objects/enemy/slime.tscn";
    private PackedScene slimeScene;

    public override void _Ready()
    {
        slimeScene = GD.Load<PackedScene>(slimePath);
    }

    public void spawn()
    {
        var slimeNode = slimeScene.Instantiate<Slime>();
        slimeNode.GlobalPosition = GlobalPosition;
        GetParent().AddChild(slimeNode);
    }
}
