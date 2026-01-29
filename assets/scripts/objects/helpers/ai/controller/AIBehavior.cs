using Galatime;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Base class for AI behaviors that can be executed when conditions are met.
/// </summary>
public abstract class AIBehavior
{
    /// <summary> Name of the behavior for debugging. </summary>
    public string Name { get; set; }

    /// <summary> Cooldown in seconds before this behavior can be used again. </summary>
    public float Cooldown { get; set; } = 0f;

    /// <summary> Time when this behavior was last executed. </summary>
    protected double LastExecutionTime { get; set; } = -1000f;

    /// <summary> Checks if the behavior is ready to be executed (not on cooldown). </summary>
    public bool IsReady(double currentTime)
    {
        return currentTime - LastExecutionTime >= Cooldown;
    }

    /// <summary> Executes the behavior for the given entity. </summary>
    /// <param name="entity">The entity to execute the behavior for.</param>
    /// <param name="delta">Time delta for this frame.</param>
    public void Execute(Entity entity, double delta)
    {
        LastExecutionTime = Time.GetTicksMsec() / 1000.0;
        OnExecute(entity, delta);
    }

    /// <summary> Override this method to implement the behavior logic. </summary>
    protected abstract void OnExecute(Entity entity, double delta);

    protected AIBehavior(string name, float cooldown = 0f)
    {
        Name = name;
        Cooldown = cooldown;
    }
}
