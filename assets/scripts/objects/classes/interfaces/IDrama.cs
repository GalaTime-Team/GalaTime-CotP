using Galatime.Global;
using Godot;

namespace Galatime.Interfaces;

/// <summary> Interface, which contains methods for cutscenes objects. </summary>
public interface IDrama
{
    /// <summary> Gets the drama object ID, which is used to identify the drama object for playing cutscenes. </summary>
    [Export] public string DramaID { get; set; }
    [Export] public Node2D DramaNode { get; set; }
    /// <summary> Plays the animation with the name. Returns true if the animation was played, false if animation was not found. </summary>
    public bool PlayDramaAnimation(string animationName);
    /// <summary> Stops the currently playing animation. </summary>
    public void StopDramaAnimation();
    /// <summary> Sets the drama object to the cutscene manager. </summary>
    public void SetDramaObject();

}