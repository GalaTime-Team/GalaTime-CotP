using Godot;
using Galatime.Global;
using Galatime;

public partial class LLobby : Node2D
{
    QuestManager QuestManager;
    PlayerVariables PlayerVariables;

    public override void _Ready()
    {
        QuestManager = QuestManager.Instance;
        PlayerVariables = PlayerVariables.Instance;

        MusicManager.Instance.Play("dream_world");

        if (QuestManager.StartQuest(new Quest("tutorial_0", "Tutorial")))
        {
            // Move spawn point to the left to avoid collision with the blockers.
            var cc = GetNode<Node2D>("PlayerSpawnPoint2");
            cc.GlobalPosition = new Vector2(cc.GlobalPosition.X - 128, cc.GlobalPosition.Y);

            // Activate blockers.
            SetTutorialBlocker("Blocker1", true); SetTutorialBlocker("Blocker2", true);

            PlayerVariables.Instance.PlayerIsReady += PlayerIsReady;
        }
    }

    void SetTutorialBlocker(string n, bool v) { Callable.From(() => GetNode<StaticBody2D>(n).GetNode<CollisionShape2D>("Collision").Disabled = !v).CallDeferred(); }

    void PlayerIsReady()
    {
        PlayerVariables.Instance.PlayerIsReady -= PlayerIsReady;

        var p = PlayerVariables.Instance.Player;
        Slime slime = null;

        PlayerVariables.Instance.Player.PlayerGui.DialogBox.StartDialog("tutorial_1_1", dialogNextPhraseCallback: (int phraseId) =>
        {
            if (phraseId == 0)
            {
                // Spawn slime.
                slime = EnemiesList.Enemies["slime"].Instantiate<Slime>();
                slime.GlobalPosition = GetNode<Node2D>("SlimeTutorialSpawn").GlobalPosition;
                AddChild(slime);

                // End tutorial when slime dies.
                slime.OnDeath += () =>
                {
                    SetTutorialBlocker("Blocker1", false); SetTutorialBlocker("Blocker2", false);

                    LevelManager.Instance.TweenTimeScale(0.5f);
                    QuestManager.FinishQuest("tutorial_0");
                };
            }
        });
    }
}
