using System;
using System.Collections.Generic;
using Godot;

namespace Galatime.AI;

/// <summary> Represents an attack cycle for an AI. </summary>
public class AttackCycle
{
    public string ID;
    /// <summary> Chance of attack being selected. 0 to 1. </summary>
    public float Chance = 1;
    /// <summary> Override attack if condition is met. </summary>
    public Func<bool> OverrideAttackIf;
    /// <summary> Called when attack is selected. </summary>
    public Action OnAttack;
    /// <summary> Called when attack is finished or canceled by the switcher. Recomended to implement this for any attack because for example, enemy can die when attacking, but not necessary. </summary>
    public Action OnAttackEnd;

    public AttackCycle(string id, Action pAttack, Action pAttackEnd = null, float pChance = 1, Func<bool> pOverrideAttackIf = null) =>
        (ID, OnAttack, OnAttackEnd, OverrideAttackIf, Chance) = (id, pAttack, pAttackEnd, pOverrideAttackIf, pChance);
}

/// <summary> Class for switching between attacks depending on conditions or random. </summary>
public partial class AttackSwitcher : Node
{
    public GameLogger Logger = new("AttackSwitcher", GameLogger.ConsoleColor.Magenta);

    /// <summary> Type of attack switcher to use. </summary>
    public enum SwitchType
    {
        /// <summary> Selects an attack randomly, but can be overridden if a condition is met. </summary>
        Random,
        /// <summary> Same as <see cref="Random"/> but always chooses not the same attack. </summary>
        RandomUnique,
        /// <summary> Selects an attack manually, meaning only methods can be called. </summary>
        Manual
    }
    

    public Timer NextCycleTimer;
    /// <summary> List of attacks to switch between. </summary>
    public List<AttackCycle> AttackCycles = new();
    /// <summary> Current attack cycle index. </summary>
    public AttackCycle CurrentAttackCycle;
    /// <summary> Type of attack switcher to use. Looks at <see cref="AttackCycles"/>. </summary>
    [Export] public SwitchType Type = SwitchType.Random;
    /// <summary> Delay between attacks in seconds. </summary>
    [Export] public float NextCycleDelay = .5f;
    private bool enabled = true;
    [Export] public bool Enabled
    {
        get => enabled;
        set
        {
            if (value && enabled != value) NextCycle();
            if (!value && enabled != value)
            {
                Logger.Log($"Disabled attack switcher. {CurrentAttackCycle?.ID ?? "None"}", GameLogger.LogType.Warning);
                ResetCurrentAttack();

                NextCycleTimer.Stop();
            }
            enabled = value;
        }
    }

    public override void _Ready()
    {
        NextCycleTimer = GetNode<Timer>("Timer");
        NextCycleTimer.Timeout += StartAttack;
    }

    public void NextCycle()
    {
        ResetCurrentAttack();

        // Start next attack.
        NextCycleTimer.Start(NextCycleDelay);
    }

    /// <summary> Resets the current attack and calls <see cref="AttackCycle.OnAttackEnd"/>. </summary>
    public void ResetCurrentAttack()
    {
        if (CurrentAttackCycle != null)
        {
            CurrentAttackCycle.OnAttackEnd?.Invoke();
            CurrentAttackCycle = null;
        }
        else
            Logger.Log($"Current attack is null, cannot reset", GameLogger.LogType.Warning);
    }

    public void StartAttack()
    {
        if (!Enabled)
        {
            NextCycleTimer.Stop();
            return;
        }

        // Select attack based on chances.
        var rnd = new Random();
        float roll = (float)rnd.NextDouble();
        float cumulative = 0.0f;

        foreach (var attack in AttackCycles)
        {
            if (attack.OverrideAttackIf != null && attack.OverrideAttackIf()) // Override attack if condition is met.
            {
                SetAndCallCurrentCycle(attack);
                break;
            }

            cumulative += attack.Chance;
            if (roll < cumulative)
            {
                SetAndCallCurrentCycle(attack);
                break;
            }
        }
    }

    /// <summary> Starts the timer for the current attack. If there's no current attack, does nothing. Equivalent almost to <see cref="SceneTree.CreateTimer(float)"/>, but it's shorter. </summary>
    /// <param name="id">The ID of the attack to start the timer for. </param>
    public void StartTimer(string id, Action callback = null, float time = 1f) =>
        GetTree().CreateTimer(time, false).Timeout += () => { if (IsAttackCycleActive(id)) callback?.Invoke(); };

    /// <summary> Starts the timer and calls the callback after it has finished. Same as <see cref="SceneTree.CreateTimer(float)"/>, but it's shorter. </summary>
    public void StartTimer(Action callback = null, float time = 1f) => 
        GetTree().CreateTimer(time, false).Timeout += callback;

    private void SetAndCallCurrentCycle(AttackCycle attack)
    {
        if (attack == null) return;

        CurrentAttackCycle = attack;
        CurrentAttackCycle.OnAttack?.Invoke();
    }

    /// <summary> Registers an attack cycle. </summary>
    public void RegisterAttackCycles(params AttackCycle[] attacks) => AttackCycles.AddRange(attacks);
    /// <summary> Returns the attack cycle with the given index. </summary>
    public AttackCycle GetAttackCycle(int i) => AttackCycles[i];
    /// <summary> Returns the attack cycle with the given ID. </summary>
    public AttackCycle GetAttackCycle(string id) => AttackCycles.Find(x => x.ID == id);
    /// <summary> Returns true if the attack cycle with the given ID is the current attack cycle. </summary>
    public bool IsAttackCycleActive(string id) => CurrentAttackCycle != null && CurrentAttackCycle.ID == id;
}