using Godot;
using System;
using System.Collections.Generic;

namespace Galatime 
{
    public abstract partial class GalatimeAbility : Node2D
    {
        public float duration;
        public float reload;
        public Texture2D texture;
        public Dictionary<string, float> costs = new Dictionary<string, float>() { { "stamina", 0 }, { "mana", 0 } };

        protected GalatimeAbility(Texture2D texture, float reload, float duration, Dictionary<string, float> ?costs)
        {
            this.texture = texture;
            this.reload = reload;
            this.duration = duration;
            this.costs = costs;
    }

        public abstract void execute(HumanoidCharacter p, float physicalAttack, float magicalAttack);
    }
}
