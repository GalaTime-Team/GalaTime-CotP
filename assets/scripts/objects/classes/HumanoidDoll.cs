using System;
using System.Collections.Generic;
using Godot;

using Galatime.Animation;

namespace Galatime.Helpers;

/// <summary> List of animations for the humanoid doll. </summary>
public enum HumanoidDollAnimations
{
    IdleUp, IdleDown, IdleLeft, IdleRight,
    WalkUp, WalkDown, WalkLeft, WalkRight,
    AttackUp, AttackDown, AttackLeft, AttackRight
};

// Define an enum for the direction of the animation velocity
enum AnimationDirection { Up, Down, Right, Left };

/// <summary> Represents a humanoid doll, which contains animations to play. </summary>
public partial class HumanoidDoll : Node2D
{
    #region Exports
    /// <summary> Path to the animation player node. </summary>
    [Export] public NodePath AnimationPlayerPath;
    #endregion

    #region Nodes
    /// <summary> Reference to the animation player. </summary>
    public AnimationPlayer AnimationPlayerReference;
    #endregion

    #region Variables

    private double CurrentAnimationTime = 0;
    private HumanoidStates CurrentState = HumanoidStates.Idle;

    public HumanoidDollAnimations CurrentAnimation;

    /// <summary> List of animations for the humanoid doll in each state. </summary>
    public static Dictionary<HumanoidStates, FourAxesAnimationsSet> Animations = new() {
        {
            HumanoidStates.Idle, new FourAxesAnimationsSet(
                HumanoidDollAnimations.IdleUp,
                HumanoidDollAnimations.IdleRight,
                HumanoidDollAnimations.IdleDown, 
                HumanoidDollAnimations.IdleLeft
            ) 
        },
        {
            HumanoidStates.Walk, new FourAxesAnimationsSet(
                HumanoidDollAnimations.WalkUp,
                HumanoidDollAnimations.WalkRight,
                HumanoidDollAnimations.WalkDown,
                HumanoidDollAnimations.WalkLeft
            )
        },
        {
            HumanoidStates.Attack, new FourAxesAnimationsSet(
                HumanoidDollAnimations.AttackUp,
                HumanoidDollAnimations.AttackRight,
                HumanoidDollAnimations.AttackDown,
                HumanoidDollAnimations.AttackLeft
            )
        }
    };

    #endregion

    public override void _Ready()
    {
        AnimationPlayerReference = GetNode<AnimationPlayer>(AnimationPlayerPath);
    }

    /// <summary> Sets the animation based on the velocity and the idle flag and the state. </summary>
    public void SetAnimation(Vector2 animationVelocity, HumanoidStates state = HumanoidStates.Idle)
    {
        // Stop the animation if idle
        // if (state == HumanoidStates.Idle) AnimationPlayerReference.Stop();

        // Setting current animation time to sync the 4 axes animation.
        if (state != CurrentState)
        {
            CurrentState = state;
            CurrentAnimationTime = 0;
        }

        AnimationDirection direction = GetDirectionByVelocity(animationVelocity);

        PlayAnimation(direction switch
        {
            AnimationDirection.Right => Animations[state].Right,
            AnimationDirection.Left => Animations[state].Left,
            AnimationDirection.Up => Animations[state].Up,
            AnimationDirection.Down => Animations[state].Down,
            _ => throw new ArgumentException(),
        });
    }

    /// <summary>
    /// Converts the animation velocity to a direction enum.
    /// </summary>
    /// <param name="animationVelocity">The animation velocity to convert.</param>
    /// <returns>Direction.</returns>
    /// <exception cref="ArgumentException"></exception>
    private static AnimationDirection GetDirectionByVelocity(Vector2 animationVelocity)
    {
        return animationVelocity switch
        {
            Vector2 v when v.X > 0 => AnimationDirection.Right,
            Vector2 v when v.X < 0 => AnimationDirection.Left,
            Vector2 v when v.Y > 0 => AnimationDirection.Down,
            Vector2 v when v.Y < 0 => AnimationDirection.Up,
            _ => AnimationDirection.Down
        };
    }

    public override void _Process(double delta)
    {
        CurrentAnimationTime += delta;
    }

    /// <summary>
    /// A helper function to play the animation based on the name and the idle flag.
    /// </summary>
    /// <param name="animationName">Animation name</param>
    /// <param name="idleFlag">If the animation is idle or not</param>
    private void PlayAnimation(HumanoidDollAnimations animationName)
    {
        // We convert the animation enum to string because why not and play the animation.
        var animation = animationName.ToString();
        AnimationPlayerReference.Play(animation);

        // Play animation based on previous animation time. Checking if the animation is not current one to prevent seek to not be repeated.
        if (CurrentAnimation != animationName) AnimationPlayerReference.Seek(CurrentAnimationTime);

        CurrentAnimation = animationName;
    }
}