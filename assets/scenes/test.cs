using Galatime;
using Godot;

public partial class test : Node2D
{
    public override void _Ready()
    {
        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        GetTree().Root.Title = "GalaTime - Test room";
    }
}
