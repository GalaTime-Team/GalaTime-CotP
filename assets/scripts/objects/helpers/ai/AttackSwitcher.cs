using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public Action Attack;

    public AttackCycle(string id, Action pAttack, float pChance = 1, Func<bool> pOverrideAttackIf = null) =>
        (ID, Attack, OverrideAttackIf, Chance) = (id, pAttack, pOverrideAttackIf, pChance);
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
        CurrentAttackCycle = null;
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
        CurrentAttackCycle.Attack?.Invoke();
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