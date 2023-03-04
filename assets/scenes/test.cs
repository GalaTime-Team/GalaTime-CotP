using Galatime;
using Godot;
using System;

public partial class test : Node2D
{
	public override void _Ready()
	{
		PlayerVariables.player.startDialog("test_0");
	}
}
