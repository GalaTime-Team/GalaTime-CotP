using Godot;
using Galatime.Interfaces;

namespace Galatime;

/// <summary> A door block that blocks the player. </summary>
public partial class Doorblock : StaticBody2D, IActivatable
{
    public AudioStreamPlayer2D AudioStreamPlayer { get; set; }

    public bool Active { get => IsOpen; set => IsOpen = value; }
    private bool isOpen = true;
    /// <summary> If the door is open. </summary>
    [Export] public bool IsOpen
    {
        get => isOpen;
        set
        {
            isOpen = value;
            CollisionLayer = (uint)(isOpen ? 0 : 1);
            Visible = !isOpen;
            if (!isOpen) AudioStreamPlayer.Play();
        }
    }

    public override void _Ready()
    {
        AudioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer");
    }
}