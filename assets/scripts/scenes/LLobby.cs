using Godot;
using Galatime.Global;

public partial class LLobby : Node2D
{   
    public override void _Ready() {
        MusicManager.Instance.Play("dream_world");
    }
}
