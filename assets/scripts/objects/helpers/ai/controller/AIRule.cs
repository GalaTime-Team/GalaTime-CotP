using System.Collections.Generic;
using System.Linq;
using Galatime;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Represents a rule that links conditions to a behavior.
/// When all conditions are met, the behavior is executed.
/// </summary>
public class AIRule
{
    /// <summary> Name of the rule for debugging. </summary>
    public string Name { get; set; }

    /// <summary> Priority of this rule (higher = evaluated first). </summary>
    public int Priority { get; set; }

    /// <summary> Probability (0-1) that this rule will execute when conditions are met. </summary>
    public float Probability { get; set; } = 1f;

    /// <summary> List of conditions that must all be met for this rule to trigger. </summary>
    public List<AICondition> Conditions { get; set; } = new();

    /// <summary> The behavior to execute when all conditions are met. </summary>
    public AIBehavior Behavior { get; set; }

    /// <summary> Whether this rule is currently enabled. </summary>
    public bool Enabled { get; set; } = true;

    public AIRule(string name, AIBehavior behavior, int priority = 0, float probability = 1f)
    {
        Name = name;
        Behavior = behavior;
        Priority = priority;
        Probability = probability;
    }

    /// <summary> Adds a condition to this rule. </summary>
    public AIRule AddCondition(AICondition condition)
    {
        Conditions.Add(condition);
        return this;
    }

    /// <summary> Evaluates all conditions for this rule. </summary>
    public bool EvaluateConditions(Entity entity)
    {
        if (!Enabled) return false;
        return Conditions.All(condition => condition.Evaluate(entity));
    }

    /// <summary> Checks if this rule should execute based on probability. </summary>
    public bool ShouldExecute()
    {
        if (Probability >= 1f) return true;
        return GD.Randf() <= Probability;
    }
}
