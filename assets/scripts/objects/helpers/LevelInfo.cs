using Godot;
using Galatime.Global;

namespace Galatime.Helpers;

/// <summary> Stores level information. When it added to the scene, it will be used in <see cref="LevelManager"/> </summary>
public partial class LevelInfo : Node
{
    [Export] public string LevelName = "?";
    /// <summary> Day of the level. If it's 0, then there's no day. </summary>
    [Export(PropertyHint.Range, "0,31")] public int Day = 0;
    /// <summary> Stored level instance that will be used in <see cref="LevelManager"/>. </summary>
    public Node LevelInstance { get; private set; }
    

    public override void _Ready()
    {
        LevelInstance = GetParent();
        GD.Print("Level instance: ", LevelInstance.Name);

        LevelManager.Instance.LevelInfo = this;
    }
}
