namespace Galatime.Interfaces;

/// <summary> Interface for activatable objects. </summary>
public interface IActivatable
{
    /// <summary> Whether or not the object is active. </summary>
    public bool Active { get; set; }
}