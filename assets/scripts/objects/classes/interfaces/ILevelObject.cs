namespace Galatime.Interfaces;

/// <summary> 
/// Interface, which contains methods for objects that can be synced between levels. 
/// It could be for example a enemies arena that is cleared from enemies.
/// </summary>
public interface ILevelObject
{
    /// <summary> Loads the current state of the level object. First time it's not called. </summary>
    public void LoadLevelObject(object[] state);
}