using Godot;
using System;
using Galatime;

public partial class LLobby : Node2D
{   
    public override void _Ready() {
        var playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
        playerVariables.Player.PlayerGui.DialogBox.StartDialog("tutorial_0");
        var levelManager = GetNode<LevelManager>("/root/LevelManager");
        levelManager.PlayAudioCombat("classicalbreak");
    }
}
