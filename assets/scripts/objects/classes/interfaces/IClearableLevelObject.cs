using Godot;

namespace Galatime.Interfaces;


/// <summary> 
/// Interface, which contains methods for objects that can be cleared from level. 
/// It could be for example a enemies arena that is cleared from enemies.
/// </summary>
public interface IClearableLevelObject
{
    /// <summary> Clears the object from the level. </summary>
    public void ClearFromLevel();
}