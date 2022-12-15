using Godot;
using System;

namespace Galatime 
{
    public abstract class GalatimeAbility : Node2D
    {
        public float duration;
        public float rotation;

        public abstract void execute(player p);
    }
}
