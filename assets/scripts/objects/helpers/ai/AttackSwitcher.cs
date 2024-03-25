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
        (ID, OnAttack, OverrideAttackIf, Chance) = (id, pAttack, pOverrideAttackIf, pChance);
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

                CurrentAttackCycle?.OnAttackEnd?.Invoke();
                CurrentAttackCycle = null;

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
        // Reset attack.
        CurrentAttackCycle?.OnAttackEnd?.Invoke();
        CurrentAttackCycle = null;

        // Start next attack.
        NextCycleTimer.Start(NextCycleDelay);
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