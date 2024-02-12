using Godot;

namespace Galatime.UI;

/// <summary> Represents a resource for a page in the diary book. </summary>
[GlobalClass] public partial class DiaryPage : Resource
{
    [Export] public string Id;
    /// <summary> The path to the control of the page. </summary>
    [Export] public NodePath Control; public Control ControlNode;
    /// <summary> The path to the button of the page that can be pressed to switch to the page. </summary>
    [Export] public NodePath Button; public Control ButtonNode;
}