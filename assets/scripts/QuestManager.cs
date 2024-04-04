using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Galatime.Global;

/// <summary> A class that represents a quest in the game. </summary>
public class Quest
{
    public string Id;
    public string Name;
    /// <summary> If the quest was completed. </summary>
    public bool Completed { get; private set; } = false;
    /// <summary> If the quest is visible in the HUD. Usually I recommend to hide it in most cases. </summary>
    public bool Visible = false;
    /// <summary> The progress of the quest from 0 to <see cref="MaxProgress"/>. </summary>
    public int Progress { get; private set; } = 0;
    /// <summary> The maximum progress of the quest. </summary>
    public int MaxProgress = 1;

    /// <summary> Called when the quest is completed. </summary>
    public Action OnComplete;
    /// <summary> Called when the quest is advanced. </summary>
    public Action OnAdvance;

    /// <summary> Advances the quest by the given amount. If your quest is completed, it will call <see cref="OnComplete"/>. </summary>
    /// <remarks> If your quest does not exceed 1 in <see cref="MaxProgress"/>, you can simply call <see cref="Finish"/>. </remarks>
    public void Advance(int amount)
    {
        Progress += amount;
        OnAdvance?.Invoke();
        if (Progress > MaxProgress) Progress = MaxProgress;
        if (Progress == MaxProgress) Finish();
    }
    
    /// <summary> Marks the quest as completed. Ignores <see cref="MaxProgress"/>. </summary>
    public void Finish()
    {
        Completed = true;
        OnComplete?.Invoke();
    }

    /// <summary> Creates a new quest with the given id and name. </summary>
    public Quest(string id, string name) 
        => (Id, Name) = (id, name);

    /// <summary> Creates a new quest with the given id, name and maximum progress. </summary>
    public Quest(string id, string name, int maxProgress)
        : this(id, name) => MaxProgress = maxProgress;
}

/// <summary> A singleton that manages all the quests in the game, including the player progress. </summary>
public partial class QuestManager : Node
{
    public static QuestManager Instance { get; private set; }
    public GameLogger Logger { get; private set; } = new GameLogger(nameof(QuestManager), GameLogger.ConsoleColor.Green);

    /// <summary> Currently active quests. </summary>
    public Dictionary<string, Quest> CurrentQuests { get; private set; } = new();
    /// <summary> Completed quests. </summary>
    public Dictionary<string, Quest> CompletedQuests { get; private set; } = new();


    /// <summary> Called when the player starts a quest. </summary>
    public Action<Quest> OnQuestStarted;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary> Starts the quest with the given id. </summary>
    /// <returns> If the quest was started. False if the quest was already completed or already started. </returns>
    public bool StartQuest(Quest quest)
    {
        if (IsQuestCompleted(quest.Id) || IsQuestStarted(quest.Id)) return false;

        CurrentQuests.Add(quest.Id, quest);
        OnQuestStarted?.Invoke(quest);

        Logger.Log($"Started quest: {quest.Name}", GameLogger.LogType.Success);

        return true;
    }

    public void FinishQuest(string questId)
    {
        var quest = CurrentQuests[questId];

        if (quest == null) Logger.Log($"Quest with id {questId} not found", GameLogger.LogType.Warning);
        else
        {
            CurrentQuests.Remove(questId);
            CompletedQuests.Add(quest.Id, quest);
            Logger.Log($"Finished quest: {quest.Name}", GameLogger.LogType.Success);
        }
    }

    /// <summary> Checks if the quest with the given id is completed. </summary>
    public bool IsQuestCompleted(string questId) => CompletedQuests.ContainsKey(questId);
    public bool IsQuestStarted(string questId) => CurrentQuests.ContainsKey(questId);
}
