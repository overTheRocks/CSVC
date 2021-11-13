using Godot;
using System;

public class Bullet : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private RayCast2D Tracer;
    private Line2D Trail;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Tracer.ForceRaycastUpdate();
        
        if (Tracer.IsColliding()){
            Trail.AddPoint(Tracer.GetCollisionPoint()-Position); // Draw To Colision Point
        } else {
            Trail.AddPoint(Tracer.CastTo); // Draw to destination if doesnt hit 
        }
        
    }
    public void SetDestination(Vector2 Destination){
        Tracer = (RayCast2D)GetNode("BulletTracer");
        Trail = (Line2D)GetNode("BulletTrail");
        Trail.AddPoint(Position);
        Tracer.CastTo = Destination;

        
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Trail.Modulate = new Color (Trail.Modulate.r,Trail.Modulate.g,Trail.Modulate.b,Trail.Modulate.a-0.175f); // Fade Trail
        if (Trail.Modulate.a <= 0){
            QueueFree(); // Remove bullet if trail has faded fully
        }
    }
}
