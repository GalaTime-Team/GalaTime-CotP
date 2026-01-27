using Galatime;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Base class for AI conditions that can be evaluated to determine if a behavior should execute.
/// </summary>
public abstract class AICondition
{
    /// <summary> Name of the condition for debugging. </summary>
    public string Name { get; set; }

    /// <summary> Evaluates the condition based on the entity's current state. </summary>
    /// <param name="entity">The entity to evaluate the condition for.</param>
    /// <returns>True if the condition is met, false otherwise.</returns>
    public abstract bool Evaluate(Entity entity);

    protected AICondition(string name)
    {
        Name = name;
    }
}
