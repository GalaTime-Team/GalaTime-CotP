using Godot;
using System;
using Galatime;

public partial class LLobby : Node2D
{   
    public override void _Ready() {
        var LevelManager = GetNode<LevelManager>("/root/LevelManager");
        // LevelManager.PlayAudioCombat("classicalbreak");
        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        playerVariables.Player.startDialog("tutorial_0");
    }
}
