using Godot;
using System;
using Galatime;

public class Slime : Entity
{
    public override void _Ready()
    {
        element = GalatimeElement.Aqua;
        body = GetNode<KinematicBody2D>("Body");
        damageEffectPoint = GetNode<Position2D>("Body/DamageEffectPoint");
    }
}
