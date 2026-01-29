using Galatime;

namespace Galatime.AI.Controller;

/// <summary>
/// Condition that checks if a specific ability is ready to use.
/// </summary>
public class AbilityReadyCondition : AICondition
{
    /// <summary> Index of the ability to check (0-2). </summary>
    public int AbilityIndex { get; set; }

    public AbilityReadyCondition(int abilityIndex = 0) : base($"AbilityReady{abilityIndex}")
    {
        AbilityIndex = abilityIndex;
    }

    public override bool Evaluate(Entity entity)
    {
        if (entity == null || entity.Abilities == null) return false;
        if (AbilityIndex < 0 || AbilityIndex >= entity.Abilities.Count) return false;
        
        var ability = entity.Abilities[AbilityIndex];
        if (ability == null || ability.IsEmpty) return false;
        
        return ability.IsReloaded && ability.Charges > 0;
    }
}
