using Godot;
using System;

public partial class LeaveLabel : Label
{
    private double escPressedTime = 0;
    private bool activated = false;

    public override void _Ready()
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
    }

    public override void _Process(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.Space))
        {
            escPressedTime += delta;
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, (float)Math.Min(escPressedTime / 1, 1));
            if (escPressedTime >= 1 && !activated)
            {
                var globals = GetNode<GalatimeGlobals>("/root/GalatimeGlobals");
                activated = true;
                globals.LoadScene();
                LevelManager.Instance.EndAudioCombat();
            }
        }
        else
        {
            escPressedTime = Mathf.Max(escPressedTime - delta, 0);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, (float)Math.Min(escPressedTime / 1, 1));
        }
    }
}
