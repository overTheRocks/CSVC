using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        KinematicBody2D TargetedNode = (KinematicBody2D)GetNode("../Player");
        Position = Position.LinearInterpolate(TargetedNode.Position,0.1f);
    }
}
