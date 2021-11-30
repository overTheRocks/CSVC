using Godot;
using System;

public class Bullet : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private RayCast2D Tracer;
    private Line2D Trail;
    private bool Dummy;
    private Camera2D Camera;
    private Sprite BulletHole;
    private int Frames = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BulletHole = (Sprite)GetNode("BulletHole");
        Camera = (Camera2D)GetNode("../../Camera2D");
        Camera.Call("AddTrauma",0.3f);
        if (Dummy){
            Trail.AddPoint(Tracer.CastTo);
        } else {
            Tracer.ForceRaycastUpdate();
            if (Tracer.IsColliding()){
                Node2D Collider = (Node2D)Tracer.GetCollider();
                Collider = (Node2D)Collider.GetParent();
                Trail.AddPoint(Tracer.GetCollisionPoint()-Position); // Draw To Colision Point
                if (GetTree().NetworkPeer != null && Collider.IsInGroup("Damageable")){
                    //GD.Print(Collider.Name.ToInt());
                    GetNode("/root/World").RpcId(Collider.Name.ToInt(),"ReceiveHit",4,Tracer.CastTo.Normalized()*400f);
                }
            } else {
                Trail.AddPoint(Tracer.CastTo); // Draw to destination if doesnt hit 
            }
            if (GetTree().NetworkPeer != null){
                GetNode("/root/World").Rpc("DummyTrail",Trail.Points[0],Trail.Points[1]);
            }
        }
        BulletHole.Position = Trail.Points[1];
        
    }
    public void SetDestination(Vector2 Destination,bool IsDummy){
        Tracer = (RayCast2D)GetNode("BulletTracer");
        Trail = (Line2D)GetNode("BulletTrail");
        Trail.AddPoint(Position);
        Tracer.CastTo = Destination;
        Dummy = IsDummy; // Dummy meaning networked visual only bullet
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

        Trail.Modulate = new Color (Trail.Modulate.r,Trail.Modulate.g,Trail.Modulate.b,Trail.Modulate.a-0.175f); // Fade Trail
        Frames++;
        if (Frames > 300){
        BulletHole.Modulate = new Color (BulletHole.Modulate.r,BulletHole.Modulate.g,BulletHole.Modulate.b,BulletHole.Modulate.a-0.025f); // Fade BulletHole
        }
        if (BulletHole.Modulate.a <= 0){
            QueueFree(); // Remove bullet if trail has faded fully
        }
    }
}
