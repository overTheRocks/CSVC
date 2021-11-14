using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    private OpenSimplexNoise SimplexX = new OpenSimplexNoise();
    private OpenSimplexNoise SimplexY = new OpenSimplexNoise();
    private OpenSimplexNoise SimplexR = new OpenSimplexNoise();
    public float Trauma = 0;
    public float TranslateScale = 0.4f;
    public float RotScale = 10;
    private float Time;
    public override void _Ready()
    {
        SimplexX.Seed = 1;
        SimplexY.Seed = 2;
        //SimplexR.Seed = 3;

        SimplexX.Octaves = 5;
        SimplexX.Period = 10.5f;
        SimplexX.Persistence = 0.256f;
        SimplexX.Lacunarity = 2;

        SimplexY.Octaves = 5;
        SimplexY.Period = 10.5f;
        SimplexY.Persistence = 0.256f;
        SimplexY.Lacunarity = 2f;

        // SimplexY.Octaves = 2;
        // SimplexY.Period = 3.5f;
        // SimplexY.Persistence = 0.256f;
        // SimplexY.Lacunarity = 2;
    }

    public override void _Process(float delta)
    {
        KinematicBody2D TargetedNode = (KinematicBody2D)GetNode("../Player");
        Position = Position.LinearInterpolate(TargetedNode.Position,0.1f);

        Trauma -= (delta * 0.73f);
        Trauma *= 0.98f;
        if (Trauma < 0){
            Trauma = 0;
        }
        if (Trauma != 0){ 
            Time += delta*1000;
        }
        OffsetH = Mathf.Pow(Trauma,1.3f) * TranslateScale * SimplexX.GetNoise1d(Time/16);
        OffsetV = Mathf.Pow(Trauma,1.3f) * TranslateScale * SimplexY.GetNoise1d(Time/16);
        RotationDegrees = Mathf.Pow(Trauma,1.3f) * RotScale * SimplexR.GetNoise1d(Time/8);
    }
    public void AddTrauma(float TraumaChange){
        Trauma += TraumaChange;
        if (Trauma > 2){
            Trauma = 1.2f;
        }
    }
}