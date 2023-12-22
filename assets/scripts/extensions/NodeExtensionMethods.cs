using System.Collections.Generic;
using System.Linq;
using Godot;

namespace NodeExtensionMethods;

public static class NodeExtensionMethods
{
    /// <summary> Checks if the node is a character, controlled by the player. </summary>
    public static bool IsPossessed(this Node node) 
    {
        if (node is HumanoidCharacter hc && hc is TestCharacter tc && tc.Possessed)
            return true;
        return false;
    }
}