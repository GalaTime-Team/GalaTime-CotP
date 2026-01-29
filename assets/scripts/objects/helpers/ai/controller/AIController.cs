using System.Collections.Generic;
using System.Linq;
using Galatime;
using Godot;

namespace Galatime.AI.Controller;

/// <summary>
/// Main AI Controller that evaluates rules and executes behaviors based on conditions.
/// Provides a flexible, condition-based AI system for entities.
/// </summary>
public partial class AIController : Node
{
    public GameLogger Logger = new("AIController", GameLogger.ConsoleColor.Cyan);

    /// <summary> The entity this controller is managing. </summary>
    public Entity Entity { get; set; }

    /// <summary> List of all AI rules, evaluated in priority order. </summary>
    public List<AIRule> Rules { get; private set; } = new();

    /// <summary> Whether the AI controller is enabled. </summary>
    public bool Enabled { get; set; } = true;

    /// <summary> The currently executing behavior (if any). </summary>
    public AIBehavior CurrentBehavior { get; private set; }

    /// <summary> Whether to log rule evaluations for debugging. </summary>
    [Export] public bool DebugMode { get; set; } = false;

    public override void _Ready()
    {
        base._Ready();
        
        // Get parent entity if not set
        if (Entity == null && GetParent() is Entity parentEntity)
        {
            Entity = parentEntity;
        }
    }

    /// <summary> Adds a rule to the controller. </summary>
    public AIController AddRule(AIRule rule)
    {
        Rules.Add(rule);
        // Keep rules sorted by priority (highest first)
        Rules = Rules.OrderByDescending(r => r.Priority).ToList();
        return this;
    }

    /// <summary> Removes a rule from the controller. </summary>
    public void RemoveRule(AIRule rule)
    {
        Rules.Remove(rule);
    }

    /// <summary> Clears all rules. </summary>
    public void ClearRules()
    {
        Rules.Clear();
    }

    /// <summary> Evaluates all rules and executes the first matching behavior. </summary>
    public void Process(double delta)
    {
        if (!Enabled || Entity == null || Entity.DeathState || Entity.DisableAI)
        {
            return;
        }

        // Evaluate rules in priority order
        foreach (var rule in Rules)
        {
            if (!rule.Enabled) continue;

            // Check if all conditions are met
            if (rule.EvaluateConditions(Entity))
            {
                // Check probability
                if (!rule.ShouldExecute()) continue;

                // Check if behavior is ready (not on cooldown)
                if (!rule.Behavior.IsReady(Time.GetTicksMsec() / 1000.0)) continue;

                if (DebugMode)
                {
                    Logger.Log($"Executing rule '{rule.Name}' with behavior '{rule.Behavior.Name}'", GameLogger.LogType.Info);
                }

                // Execute the behavior
                CurrentBehavior = rule.Behavior;
                rule.Behavior.Execute(Entity, delta);
                
                // Only execute one behavior per frame
                return;
            }
        }

        // No rule matched
        CurrentBehavior = null;
    }

    /// <summary> Helper method to create a simple rule with a single condition. </summary>
    public static AIRule CreateRule(string name, AICondition condition, AIBehavior behavior, int priority = 0, float probability = 1f)
    {
        var rule = new AIRule(name, behavior, priority, probability);
        rule.AddCondition(condition);
        return rule;
    }

    /// <summary> Helper method to create a rule with multiple conditions. </summary>
    public static AIRule CreateRule(string name, List<AICondition> conditions, AIBehavior behavior, int priority = 0, float probability = 1f)
    {
        var rule = new AIRule(name, behavior, priority, probability);
        foreach (var condition in conditions)
        {
            rule.AddCondition(condition);
        }
        return rule;
    }
}
