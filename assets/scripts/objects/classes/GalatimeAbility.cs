using Godot;
using System.Collections.Generic;

namespace Galatime
{
    public abstract partial class GalatimeAbility : Node2D
    {
        /// <summary> The ability data for this ability. Contains the costs, duration and reload. </summary>
        public AbilityData Data;

        /// <summary> Executes the ability for any entity. Override this in derived classes. </summary>
        /// <param name="entity">The entity that is using the ability.</param>
        public virtual void Execute(Entity entity)
        {
            // Default implementation - try to cast to HumanoidCharacter for backward compatibility
            if (entity is HumanoidCharacter humanoid)
            {
                Execute(humanoid);
            }
        }

        /// <summary> Executes the ability for a humanoid character. For backward compatibility. </summary>
        /// <param name="p">The humanoid character that is using the ability.</param>
        public virtual void Execute(HumanoidCharacter p)
        {
            // Call the Entity version for forward compatibility
            Execute((Entity)p);
        }
    }
}
