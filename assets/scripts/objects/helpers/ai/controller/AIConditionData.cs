using Godot;
using Godot.Collections;

namespace Galatime.AI.Controller;

/// <summary>
/// Exportable data for a single AI condition.
/// Separated into its own file for better Godot editor recognition.
/// </summary>
[GlobalClass]
public partial class AIConditionData : Resource
{
	/// <summary> Type of condition to check. </summary>
	[Export] public AIConditionType ConditionType { get; set; } = AIConditionType.HasTarget;
	
	/// <summary> Parameters for the condition (e.g., threshold, distance). </summary>
	[Export] public Dictionary ConditionParams { get; set; } = new();
}
