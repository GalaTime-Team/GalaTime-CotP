using Godot;
using System;
using System.Collections.Generic;

namespace Galatime 
{
    public abstract class GalatimeAbility : Node2D
    {
        public float duration;
        public float reload;
        public Texture texture;
        public Dictionary<string, float> costs = new Dictionary<string, float>() { { "stamina", 0 }, { "mana", 0 } };

        protected GalatimeAbility(Texture texture, float reload, float duration, Dictionary<string, float> ?costs)
        {
            this.texture = texture;
            this.reload = reload;
            this.duration = duration;
            this.costs = costs; 
        }

        public abstract void execute(Player p);
    }
}
