using Godot;
using Newtonsoft.Json;

namespace Galatime;

[JsonObject(MemberSerialization.OptIn)]
public class AllyData
{
    [JsonProperty("id")]
    public string ID = "";

    [JsonProperty("name")]
    public string Name = "";

    public Texture2D Icon;

    public PackedScene Scene;

    private string iconPath;
    [JsonProperty("icon")]
    public string IconPath 
    {
        get => iconPath;
        set 
        {
            iconPath = value;
            Icon = GD.Load<Texture2D>(value);
        }
    }

    private string scenePath;
    [JsonProperty("scene")]
    public string ScenePath {
        get => scenePath;
        set
        {
            scenePath = value;
            Scene = GD.Load<PackedScene>(value);
        }
    }

    public bool IsEmpty => ID == "";

    public HumanoidCharacter Instance;
}