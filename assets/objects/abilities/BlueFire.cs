using Galatime;
using Godot;
using System.Collections.Generic;

public partial class BlueFire : GalatimeAbility
{
    private HumanoidCharacter p;
    private int visualsCount = 5;

    private PackedScene blueFireVisualScene = GD.Load<PackedScene>("res://assets/objects/abilities/BlueFireVisuals.tscn");
    private List<BlueFireVisuals> visuals = new List<BlueFireVisuals>();

    private Timer spawnVisualsTimer = new Timer();

    public override void _Ready()
    {
        spawnVisualsTimer.WaitTime = 0.63f;
        spawnVisualsTimer.Timeout += spawnVisualsTimerTimeout;
        AddChild(spawnVisualsTimer);
    }

    public override async void Execute(HumanoidCharacter p)
    {
        this.p = p;

        var newStats = p.Stats;

        p.Stats[EntityStatType.MagicalAttack].Value += 30;
        p.Stats[EntityStatType.PhysicalAttack].Value += 30;

        spawnVisualsTimer.Start();

        await ToSignal(GetTree().CreateTimer(Data.Duration), "timeout");
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
        p.Stats[EntityStatType.MagicalAttack].Value -= 30;
        p.Stats[EntityStatType.MagicalAttack].Value -= 30;

        foreach (var v in visuals)
        {
            v.destroy();
        }
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        QueueFree();
    }

    public override void _Process(double delta)
    {
        GlobalPosition = p.Weapon.GlobalPosition;
    }
}
