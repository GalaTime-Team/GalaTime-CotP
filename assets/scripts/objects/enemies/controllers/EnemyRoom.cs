using System.Collections.Generic;
using Godot;
using Galatime.Helpers;
using Galatime;
using Galatime.Global;
using NodeExtensionMethods;
using Galatime.Interfaces;

/// <summary> Node, which controls the enemy room. Spawns enemies and controls doors. </summary>
public partial class EnemyRoom : Node2D, ILevelObject
{
    public bool CanActivate = true;

    [Export] public Area2D TriggerArea;

    /// <summary> The spawn positions for the enemies. </summary>
    public List<EnemySpawnPosition> SpawnPositions = new();

    [Export] public Godot.Collections.Array<Doorblock> DoorBlocks = new();
    /// <summary> The door blocks that block the player. </summary>
    public List<Doorblock> DoorBlocksList = new();

    /// <summary> The current spawned enemies in the room. </summary>
    public List<Entity> CurrentEnemies = new();

    public override void _Ready()
    {
        DoorBlocksList.AddRange(DoorBlocks);

        // Adding the spawn positions for the children.
        foreach (var item in GetChildren())
        {
            switch (item)
            {
                // Add spawn position to the list.
                case EnemySpawnPosition spawn:
                    SpawnPositions.Add(spawn);
                    break;
            }
        }

        // Subscribe to the BodyEntered event of the trigger area if it exists.
        if (TriggerArea != null) TriggerArea.BodyEntered += OnEnter;
    }

    public void LoadLevelObject(object[] data)
    {
        var isActivated = (bool)data[0];
        CanActivate = isActivated;
    }

    private void OnEnter(Node node)
    {
        if (node.IsPossessed())
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
        if (!CanActivate) return;

        LevelManager.Instance.IsCombat = true;

        MusicManager.Instance.Pause();

        // Close all the doors
        foreach (var door in DoorBlocksList) door.IsOpen = false;
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        
        MusicManager.Instance.SwitchAudio(false, 0, playFromBeginning: true);

        // Spawn enemies at each spawn position
        foreach (var spawn in SpawnPositions)
        {
            // Instantiate the enemy entity
            var enemy = EnemiesList.Enemies[spawn.SpawningEnemyId].Instantiate<Entity>();
            // Set the enemy's position and add it to the scene
            enemy.GlobalPosition = spawn.GlobalPosition;
            GetParent().AddChild(enemy);
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
        LevelManager.TweenTimeScale();

        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        LevelManager.IsCombat = false;
        DoorBlocksList.ForEach(door => door.IsOpen = true);
        CurrentEnemies.Clear();

        MusicManager.Instance.SwitchAudio(true, 1f);

        // Save the level object, so it can be loaded again between levels.
        LevelManager.Instance.SaveLevelObject(this, new object[] { false });
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