using Godot;
using System.Collections.Generic;

namespace Galatime
{
    public abstract partial class GalatimeAbility : Node2D
    {
        /// <summary> The ability data for this ability. Contains the costs, duration and reload. </summary>
        public AbilityData Data;

        public abstract void Execute(HumanoidCharacter p);
    }
}
