using Godot;
using Galatime;
using Galatime.Global;
using Galatime.Helpers;
using System;

/// <summary>
/// Non-Player Character entity that can be configured with custom AI behaviors and abilities.
/// Can be used for allies or enemies depending on Team setting.
/// </summary>
public partial class NPCharacter : Entity
{
    /// <summary> If true, the NPC will follow the player when not in combat. </summary>
    [Export] public bool FollowPlayer = false;
    
    /// <summary> Navigation agent for pathfinding. </summary>
    public NavigationAgent2D Navigation;
    
    /// <summary> Target controller for finding and tracking targets. </summary>
    public TargetController TargetController;
    
    /// <summary> Timer for controlling ability usage. </summary>
    public Timer AbilityTimer;
    
    /// <summary> Delay between ability uses in seconds. </summary>
    [Export] public float AbilityUseDelay = 2f;

    public override void _Ready()
    {
        base._Ready();
        
        Body = this;
        
        // Try to get navigation and target controller if they exist
        Navigation = GetNodeOrNull<NavigationAgent2D>("Navigation");
        TargetController = GetNodeOrNull<TargetController>("TargetController");
        
        // Setup ability timer
        AbilityTimer = new Timer
        {
            WaitTime = AbilityUseDelay,
            OneShot = false
        };
        AddChild(AbilityTimer);
        AbilityTimer.Timeout += TryUseAbility;
        AbilityTimer.Start();
    }

    public override void _AIProcess(double delta)
    {
        // Call base AI behaviors first
        base._AIProcess(delta);
        
        // Default AI: Basic combat behavior if no custom behaviors are set
        if (AIBehaviors.Count == 0)
        {
            DefaultAI(delta);
        }
    }

    /// <summary> Default AI behavior for NPCs. </summary>
    private void DefaultAI(double delta)
    {
        if (DeathState) return;
        
        // If we have a target controller and a target, engage in combat
        if (TargetController != null && TargetController.CurrentTarget != null)
        {
            CombatBehavior();
        }
        // Otherwise, follow player if enabled
        else if (FollowPlayer && Navigation != null)
        {
            FollowBehavior();
        }
    }

    /// <summary> Combat AI behavior. </summary>
    private void CombatBehavior()
    {
        if (Navigation == null || TargetController.CurrentTarget == null) return;
        
        var target = TargetController.CurrentTarget;
        Navigation.TargetPosition = target.GlobalPosition;
        
        var distance = GlobalPosition.DistanceTo(target.GlobalPosition);
        
        // Move towards target if too far
        if (distance > 150 && CanMove)
        {
            var direction = GlobalPosition.DirectionTo(Navigation.GetNextPathPosition());
            Body.Velocity = direction * Speed;
        }
        else
        {
            Body.Velocity = Vector2.Zero;
        }
    }

    /// <summary> Follow player behavior. </summary>
    private void FollowBehavior()
    {
        var player = Player.CurrentCharacter;
        if (player == null || Navigation == null) return;
        
        var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
        
        // Only follow if player is far enough away
        if (distance > 100)
        {
            Navigation.TargetPosition = player.GlobalPosition;
            var direction = GlobalPosition.DirectionTo(Navigation.GetNextPathPosition());
            Body.Velocity = direction * Speed;
        }
        else
        {
            Body.Velocity = Vector2.Zero;
        }
    }

    /// <summary> Tries to use a random available ability. </summary>
    private void TryUseAbility()
    {
        if (DisableAI) return;
        if (DeathState || Abilities.Count == 0) return;
        if (TargetController == null || TargetController.CurrentTarget == null) return;
        
        // Find all usable abilities
        var usableAbilities = new System.Collections.Generic.List<int>();
        for (int i = 0; i < Abilities.Count && i < 3; i++)
        {
            if (!Abilities[i].IsEmpty && Abilities[i].IsReloaded && Abilities[i].Charges > 0)
            {
                usableAbilities.Add(i);
            }
        }
        
        // Use a random ability if any are available
        if (usableAbilities.Count > 0)
        {
            var random = new Random();
            var index = usableAbilities[random.Next(usableAbilities.Count)];
            UseAbility(index);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        // Clean up ability timers
        foreach (var ability in Abilities)
        {
            if (ability.CooldownTimer != null && GodotObject.IsInstanceValid(ability.CooldownTimer))
            {
                ability.CooldownTimer.QueueFree();
            }
        }
    }
}
