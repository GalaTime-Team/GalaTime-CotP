using System.Collections.Generic;
using Godot;
using Galatime.Helpers;
using Galatime;

/// <summary> Node, which controls the enemy room. Spawns enemies and controls doors. </summary>
public partial class EnemyRoom : Node2D
{
    public Area2D TriggerArea;

    /// <summary> The spawn positions for the enemies. </summary>
    public List<EnemySpawnPosition> SpawnPositions = new();
    /// <summary> The door blocks that block the player. </summary>
    public List<Doorblock> DoorBlocks = new();
    /// <summary> The current spawned enemies in the room. </summary>
    public List<Entity> CurrentEnemies = new();

    public override void _Ready()
    {
        // Adding the spawn positions for the children.
        foreach (var item in GetChildren())
        {
            switch (item)
            {
                // Add spawn position to the list.
                case EnemySpawnPosition spawn:
                    SpawnPositions.Add(spawn);
                    break;
                // Add door block to the list.
                case Doorblock block:
                    DoorBlocks.Add(block);
                    break;
                // Set the trigger area if it not assigned.
                case Area2D area when TriggerArea == null:
                    TriggerArea = area;
                    break;
            }
        }

        // Subscribe to the BodyEntered event of the trigger area if it exists.
        if (TriggerArea != null) TriggerArea.BodyEntered += OnEnter;
    }

    private void OnEnter(Node node)
    {
        if (node is Player p)
        {
            StartBattle();

            // Don't activate the room if the player is in it
            TriggerArea.BodyEntered -= OnEnter;
        }
    }

    /// <summary>
    /// Starts the battle by spawning enemies.
    /// </summary>
    public async void StartBattle()
    {
        var LevelManager = GetNode<LevelManager>("/root/LevelManager");
        // Set the IsCombat flag to true
        LevelManager.IsCombat = true;

        // Close all the doors
        foreach (var door in DoorBlocks) door.IsOpen = false;
        // Wait for 1 second
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        // Spawn enemies at each spawn position
        foreach (var spawn in SpawnPositions)
        {
            // Instantiate the enemy entity
            var enemy = EnemiesList.Enemies[spawn.SpawningEnemyId].Instantiate<Entity>();
            // Set the enemy's position and add it to the scene
            enemy.GlobalPosition = spawn.GlobalPosition;
            GetParent().AddChild(enemy);
            // Add the enemy to the list of current enemies
            CurrentEnemies.Add(enemy);

            // Subscribe to the OnDeath event of the enemy to track enemies.
            enemy.OnDeath += OnEnemyDeath;

            // Wait for 0.2 seconds before spawning the next enemy
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        }
    }

    /// <summary>
    /// Ends the battle by performing necessary actions.
    /// </summary>
    public async void EndBattle()
    {
        var LevelManager = GetNode<LevelManager>("/root/LevelManager");

        // Start the time scale tween animation
        LevelManager.TweenTimeScale();

        // Wait for 1 second
        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        // Disable combat mode
        LevelManager.IsCombat = false;

        // Open all the doors
        foreach (var door in DoorBlocks)
        {
            door.IsOpen = true;
        }

        // Clear the CurrentEnemies list
        CurrentEnemies.Clear();
    }

    private void OnEnemyDeath()
    {
        // Count the number of enemies that have been killed
        var killedEnemiesCount = CurrentEnemies.FindAll(x => x.DeathState == true || x == null).Count;

        // If all enemies have been killed, end the battle
        if (killedEnemiesCount == CurrentEnemies.Count) 
        {
            EndBattle();
        }
    }
}