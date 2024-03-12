using Godot;

namespace Galatime;

public partial class PlayerSpawnPoint : Node2D
{
    /// <summary> The index of the player spawn point. </summary>
    [Export(PropertyHint.Range, "0,255,1")] 
    public byte SpawnIndex = 0;
}
