using Godot;
using Galatime.Global;
using Galatime;

public partial class LLobby : Node2D
{   
    public override async void _Ready() 
    {
        MusicManager.Instance.Play("dream_world");

        void b(string n, bool v) { Callable.From(() => GetNode<StaticBody2D>(n).GetNode<CollisionShape2D>("Collision").Disabled = v).CallDeferred(); }

        var qm = QuestManager.Instance;
        if (qm.StartQuest(new Quest("tutorial_0", "Tutorial")))
        {
            // Move spawn point to the left to avoid collision with the blockers.
            var cc = GetNode<Node2D>("PlayerSpawnPoint2");
            cc.GlobalPosition = new Vector2(cc.GlobalPosition.X - 128, cc.GlobalPosition.Y);

            // Activate blockers.
            b("Blocker1", false); b("Blocker2", false);

            // Spawn slime.
            var slime = EnemiesList.Enemies["slime"].Instantiate<Entity>();
            slime.GlobalPosition = GetNode<Node2D>("SlimeTutorialSpawn").GlobalPosition;
            AddChild(slime);

            // End tutorial when slime dies.
            slime.OnDeath += () =>
            {
                b("Blocker1", true); b("Blocker2", true);

                LevelManager.Instance.TweenTimeScale(0.5f);
                qm.FinishQuest("tutorial_0");
            };
        }
    }
}
