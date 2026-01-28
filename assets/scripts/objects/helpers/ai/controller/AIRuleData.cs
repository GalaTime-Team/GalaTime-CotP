using Godot;
using Godot.Collections;

namespace Galatime.AI.Controller;

/// <summary>
/// Exportable data structure for defining AI rules in the Godot editor.
/// Allows full customization of AI behavior without code changes.
/// </summary>
[GlobalClass]
public partial class AIRuleData : Resource
{
	/// <summary> Name of the rule for debugging and identification. </summary>
	[Export] public string RuleName { get; set; } = "Unnamed Rule";
	
	/// <summary> Priority (higher = evaluated first). Typical ranges: 100=emergency, 50-90=combat, 10-40=movement, 0-10=idle. </summary>
	[Export] public int Priority { get; set; } = 50;
	
	/// <summary> Probability (0-1) that this rule executes when conditions met. 1.0 = always. </summary>
	[Export(PropertyHint.Range, "0,1,0.1")] public float Probability { get; set; } = 1.0f;
	
	/// <summary> Whether this rule is enabled. </summary>
	[Export] public bool Enabled { get; set; } = true;
	
	/// <summary> Behavior to execute when conditions are met. </summary>
	[Export] public AIBehaviorType BehaviorType { get; set; } = AIBehaviorType.Idle;
	
	/// <summary> Parameters for the behavior (e.g., distance, speed, ability index). </summary>
	[Export] public Dictionary BehaviorParams { get; set; } = new();
	
	/// <summary> Conditions that must ALL be true for this rule to execute. </summary>
	[Export] public Array<AIConditionData> Conditions { get; set; } = new();
}

/// <summary>
/// Exportable data for a single AI condition.
/// </summary>
[GlobalClass]
public partial class AIConditionData : Resource
{
	/// <summary> Type of condition to check. </summary>
	[Export] public AIConditionType ConditionType { get; set; } = AIConditionType.HasTarget;
	
	/// <summary> Parameters for the condition (e.g., threshold, distance). </summary>
	[Export] public Dictionary ConditionParams { get; set; } = new();
}

/// <summary>
/// Available AI behavior types that can be configured in the editor.
/// </summary>
public enum AIBehaviorType
{
	Idle,
	MeleeAttack,
	RangedAttack,
	Strafe,
	Dodge,
	Flee,
	FollowPlayer
}

/// <summary>
/// Available AI condition types that can be configured in the editor.
/// </summary>
public enum AIConditionType
{
	HasTarget,
	NoTarget,
	LowHealth,
	LowMana,
	LowStamina,
	TargetDistance,
	AbilityReady
}
