using Galatime;
using Godot;
using System;
using System.Collections.Generic;

public partial class BlueFire : GalatimeAbility
{
	private HumanoidCharacter p;
	private int visualsCount = 5;

	private PackedScene blueFireVisualScene = GD.Load<PackedScene>("res://assets/objects/abilities/BlueFireVisuals.tscn");
	private List<BlueFireVisuals> visuals = new List<BlueFireVisuals>();

	private Timer spawnVisualsTimer = new Timer();

    public BlueFire() : base(
		GD.Load<Texture2D>("res://sprites/gui/abilities/ignis/blue_fire.png"),
		90,
		30,
		new System.Collections.Generic.Dictionary<string, float>()
		{
			{ "mana", 30f }
		}
	)
	{ }

	public override void _Ready()
	{
        spawnVisualsTimer.WaitTime = 0.63f;
        spawnVisualsTimer.Timeout += spawnVisualsTimerTimeout;
        AddChild(spawnVisualsTimer);
    }

	public override async void execute(HumanoidCharacter p, float physicalAttack, float magicalAttack)
	{
		this.p = p;

		p.Stats.magicalAttack.value += 30;
		p.Stats.physicalAttack.value += 30;

		spawnVisualsTimer.Start();

		await ToSignal(GetTree().CreateTimer(duration), "timeout");
		destroy();
	}

    private void spawnVisualsTimerTimeout()
	{
		var blueFireVisual = blueFireVisualScene.Instantiate<BlueFireVisuals>();
        blueFireVisual.Radius = 120;
        blueFireVisual.RotateSpeed = 2;
        var newPosition = new Vector2();
        newPosition.X -= blueFireVisual.Radius / 2;
        blueFireVisual.Position = newPosition;
		AddChild(blueFireVisual);
		visuals.Add(blueFireVisual);
		if (visuals.Count >= visualsCount)
		{
			spawnVisualsTimer.Stop();
		}
    }

    public async void destroy()
	{
        p.Stats.magicalAttack.value -= 30;
        p.Stats.physicalAttack.value -= 30;

		foreach (var v in visuals)
		{
			v.destroy();
		}
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        QueueFree();
	}

	public override void _Process(double delta)
	{
        GlobalPosition = p.weapon.GlobalPosition;
    }
}
