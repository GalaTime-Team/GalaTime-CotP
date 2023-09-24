using Godot;
using Galatime;

namespace Galatime.Helpers;

/// <summary> Where the enemies spawn. Used by the <see cref="EnemySpawnPosition"/> node. </summary>
[Tool]
public partial class EnemySpawnPosition : Node2D {
    #region Exports
    /// <summary> The Idintifier of the enemy instance. </summary>
    [Export] public string SpawningEnemyId = "";
    #endregion

    #region Nodes
    public Label EnemyNameLabel;
    #endregion

    public override void _Ready() {
        EnemyNameLabel = GetNode<Label>("EnemyNameLabel");
    }

    public override void _Process(double delta) {
        // Displaying the spawn position information in the editor
        if (Engine.IsEditorHint()) {
            EnemyNameLabel.Text = SpawningEnemyId;
        }
    }
}   