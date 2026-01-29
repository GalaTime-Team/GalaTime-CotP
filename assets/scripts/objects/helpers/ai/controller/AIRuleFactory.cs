using System;
using Godot;
using Godot.Collections;

namespace Galatime.AI.Controller;

/// <summary>
/// Factory class for creating AI rules, conditions, and behaviors from exportable data.
/// Converts editor-configured AIRuleData into functional AIRule objects.
/// </summary>
public static class AIRuleFactory
{
	/// <summary>
	/// Creates an AIRule from exportable AIRuleData.
	/// </summary>
	public static AIRule CreateRule(AIRuleData data, Entity entity)
	{
		if (data == null) return null;
		
		// Create behavior with ability selection support
		var behavior = CreateBehavior(data.BehaviorType, data.BehaviorParams, data.AbilityId, data.AbilityIndex, entity);
		if (behavior == null)
		{
			GD.PushWarning($"Failed to create behavior type: {data.BehaviorType}");
			return null;
		}
		
		// Create rule
		var rule = new AIRule(data.RuleName, behavior, data.Priority, data.Probability);
		rule.Enabled = data.Enabled;
		
		// Add conditions
		foreach (var conditionData in data.Conditions)
		{
			var condition = CreateCondition(conditionData.ConditionType, conditionData.ConditionParams);
			if (condition != null)
			{
				rule.AddCondition(condition);
			}
			else
			{
				GD.PushWarning($"Failed to create condition type: {conditionData.ConditionType}");
			}
		}
		
		return rule;
	}
	
	/// <summary>
	/// Creates an AIBehavior from behavior type and parameters.
	/// Supports ability selection via abilityId or abilityIndex.
	/// </summary>
	public static AIBehavior CreateBehavior(AIBehaviorType type, Dictionary parameters, string abilityId = "", int abilityIndex = -1, Entity entity = null)
	{
		parameters ??= new Dictionary();
		
		// Determine which ability index to use for RangedAttack
		int finalAbilityIndex = abilityIndex;
		if (type == AIBehaviorType.RangedAttack)
		{
			// If abilityId is specified, try to find it in entity's abilities
			if (!string.IsNullOrEmpty(abilityId) && entity != null)
			{
				finalAbilityIndex = FindAbilityIndex(entity, abilityId);
				if (finalAbilityIndex == -1)
				{
					GD.PushWarning($"Ability '{abilityId}' not found in entity, using index {abilityIndex}");
					finalAbilityIndex = abilityIndex >= 0 ? abilityIndex : 0;
				}
			}
			// Otherwise use the provided index, or default from parameters
			else if (finalAbilityIndex < 0)
			{
				finalAbilityIndex = GetIntParam(parameters, "ability_index", 0);
			}
		}
		
		return type switch
		{
			AIBehaviorType.Idle => new IdleBehavior(
				cooldown: GetFloatParam(parameters, "cooldown", 0f)),
			
			AIBehaviorType.MeleeAttack => new MeleeAttackBehavior(
				stopDistance: GetFloatParam(parameters, "stop_distance", 60f),
				cooldown: GetFloatParam(parameters, "cooldown", 0f)),
			
			AIBehaviorType.RangedAttack => new RangedAttackBehavior(
				abilityIndex: finalAbilityIndex,
				strafe: GetBoolParam(parameters, "strafe", true),
				optimalDistance: GetFloatParam(parameters, "optimal_distance", 300f),
				cooldown: GetFloatParam(parameters, "cooldown", 1f)),
			
			AIBehaviorType.Strafe => new StrafeBehavior(
				optimalDistance: GetFloatParam(parameters, "optimal_distance", 250f),
				clockwise: GetBoolParam(parameters, "clockwise", true),
				cooldown: GetFloatParam(parameters, "cooldown", 0f)),
			
			AIBehaviorType.Dodge => new DodgeBehavior(
				dodgeDistance: GetFloatParam(parameters, "dodge_distance", 200f),
				consumeStamina: GetBoolParam(parameters, "consume_stamina", false),
				staminaCost: GetFloatParam(parameters, "stamina_cost", 0f),
				cooldown: GetFloatParam(parameters, "cooldown", 3f)),
			
			AIBehaviorType.Flee => new FleeBehavior(
				fleeDistance: GetFloatParam(parameters, "flee_distance", 400f),
				cooldown: GetFloatParam(parameters, "cooldown", 2f)),
			
			AIBehaviorType.FollowPlayer => new FollowPlayerBehavior(
				followDistance: GetFloatParam(parameters, "follow_distance", 120f),
				cooldown: GetFloatParam(parameters, "cooldown", 0f)),
			
			_ => null
		};
	}
	
	/// <summary>
	/// Finds the index of an ability in an entity by ability ID.
	/// Returns -1 if not found.
	/// </summary>
	private static int FindAbilityIndex(Entity entity, string abilityId)
	{
		for (int i = 0; i < entity.Abilities.Count; i++)
		{
			var ability = entity.Abilities[i];
			if (ability != null && ability.ID == abilityId)
			{
				return i;
			}
		}
		return -1;
	}
	
	/// <summary>
	/// Creates an AICondition from condition type and parameters.
	/// </summary>
	public static AICondition CreateCondition(AIConditionType type, Dictionary parameters)
	{
		parameters ??= new Dictionary();
		
		return type switch
		{
			AIConditionType.HasTarget => new HasTargetCondition(),
			
			AIConditionType.NoTarget => new NoTargetCondition(),
			
			AIConditionType.LowHealth => new LowHealthCondition(
				threshold: GetFloatParam(parameters, "threshold", 0.3f)),
			
			AIConditionType.LowMana => new LowManaCondition(
				threshold: GetFloatParam(parameters, "threshold", 0.3f)),
			
			AIConditionType.LowStamina => new LowStaminaCondition(
				threshold: GetFloatParam(parameters, "threshold", 0.3f)),
			
			AIConditionType.TargetDistance => new TargetDistanceCondition(
				type: GetDistanceType(parameters),
				distance: GetFloatParam(parameters, "distance", 100f),
				maxDistance: GetFloatParam(parameters, "distance2", 200f)),
			
			AIConditionType.AbilityReady => new AbilityReadyCondition(
				abilityIndex: GetIntParam(parameters, "ability_index", 0)),
			
			_ => null
		};
	}
	
	// Helper methods to safely extract parameters from Godot Dictionary (Variant values)
	private static float GetFloatParam(Dictionary dict, string key, float defaultValue)
	{
		if (dict.ContainsKey(key))
		{
			var value = dict[key];
			// Handle Variant to float conversion
			if (value.VariantType == Variant.Type.Float || value.VariantType == Variant.Type.Int)
			{
				return value.AsSingle();
			}
		}
		return defaultValue;
	}
	
	private static int GetIntParam(Dictionary dict, string key, int defaultValue)
	{
		if (dict.ContainsKey(key))
		{
			var value = dict[key];
			// Handle Variant to int conversion
			if (value.VariantType == Variant.Type.Int || value.VariantType == Variant.Type.Float)
			{
				return value.AsInt32();
			}
		}
		return defaultValue;
	}
	
	private static bool GetBoolParam(Dictionary dict, string key, bool defaultValue)
	{
		if (dict.ContainsKey(key))
		{
			var value = dict[key];
			// Handle Variant to bool conversion
			if (value.VariantType == Variant.Type.Bool)
			{
				return value.AsBool();
			}
		}
		return defaultValue;
	}
	
	private static string GetStringParam(Dictionary dict, string key, string defaultValue)
	{
		if (dict.ContainsKey(key))
		{
			var value = dict[key];
			// Handle Variant to string conversion
			if (value.VariantType == Variant.Type.String)
			{
				return value.AsString();
			}
		}
		return defaultValue;
	}
	
	private static TargetDistanceCondition.DistanceType GetDistanceType(Dictionary dict)
	{
		var typeStr = GetStringParam(dict, "distance_type", "LessThan");
		return Enum.TryParse<TargetDistanceCondition.DistanceType>(typeStr, out var result)
			? result
			: TargetDistanceCondition.DistanceType.LessThan;
	}
}
