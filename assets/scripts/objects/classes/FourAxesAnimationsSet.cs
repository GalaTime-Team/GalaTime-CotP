using Godot;
using Galatime;

namespace Galatime.Animation;

public class FourAxesAnimationsSet
{
    public HumanoidDollAnimations Right;
    public HumanoidDollAnimations Left;
    public HumanoidDollAnimations Up;
    public HumanoidDollAnimations Down;

    public FourAxesAnimationsSet(
        HumanoidDollAnimations up, HumanoidDollAnimations right,
        HumanoidDollAnimations down, HumanoidDollAnimations left
    ) => 
        (Up, Right, Down, Left) = (up, right, down, left);
}