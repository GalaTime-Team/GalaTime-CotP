using Godot;
using System;

public partial class LLobby : Node2D
{   
    public override void _Ready() {
        var LevelManager = GetNode<LevelManager>("/root/LevelManager");
        // LevelManager.PlayAudioCombat("classicalbreak");
    }
}
