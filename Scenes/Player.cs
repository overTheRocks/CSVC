using Godot;
using System;

public class Player : KinematicBody2D
{
    private Sprite PlayerSprite;
    private Sprite GunSprite;
    private PackedScene GunParticles;
    private PackedScene Bullet;
    private Vector2 Velocity = Vector2.Zero; 
    private AudioStreamPlayer2D ShootSound;
    private const float Speed = 100; //nice
    private Node2D BulletHell;
    public Vector2 MeasuredVelo = Vector2.Zero;
    public float Accuracy = 0;
    private Node2D Cursor;
    private Random RNG = new Random();
    public override void _Ready()
    {
        Cursor = (Node2D)GetNode("../Cursor");
        BulletHell = (Node2D)GetNode("../BulletHell");
        PlayerSprite = (Sprite)GetNode("PlayerSprite");
        GunSprite = (Sprite)GetNode("GunSprite");
        GunParticles = (PackedScene)ResourceLoader.Load("res://Scenes/GunParticles.tscn");
        Bullet = (PackedScene)ResourceLoader.Load("res://Scenes/Bullet.tscn");
        ShootSound = (AudioStreamPlayer2D)GetNode("GunSprite/ShootSound");
    }
    public override void _PhysicsProcess(float delta)
    {
        Accuracy = Accuracy * 0.85f;
        Sprite CursorL = (Sprite)Cursor.GetNode("L");
        Sprite CursorR = (Sprite)Cursor.GetNode("R");
        CursorL.Offset = new Vector2(-500-Accuracy*50,-260);
        CursorR.Offset = new Vector2(Accuracy*50,-260);
    }
    public override void _Process(float Delta)
    {
        Vector2 MousePos = GetGlobalMousePosition();

        GunSprite.Rotation = GetAngleTo(MousePos) - Mathf.Pi/2; // Mathf.pi/2 is 90 degrees
        
        GunSprite.Offset = GunSprite.Offset.LinearInterpolate(new Vector2(0,13),0.2f);  // Reset Gun kickback over time
        
        if (Input.IsActionPressed("FireGun")){
            // Instance shooting particles:
            Accuracy += 15;
            Particles2D GunParticlesInstance = (Particles2D)GunParticles.Instance();
            GunParticlesInstance.Position = (MousePos-Position).Normalized()*15f;
            GunParticlesInstance.Rotation = GunSprite.Rotation;
            AddChild(GunParticlesInstance); // Add particles as child
            //GD.Print(Bullet.ResourceName+"ee");
            Node2D BulletInstance = (Node2D)Bullet.Instance();
            BulletInstance.Call("SetDestination",RadianToVector2(GetAngleTo(MousePos)+(((float)RNG.NextDouble()-0.5f)*Accuracy/100))*1000f,false);
            BulletInstance.Position = Position;
            
            BulletHell.AddChild(BulletInstance);

            GunSprite.Offset = new Vector2(GunSprite.Offset.x,0); // Sprite Kickback
            ShootSound.Play();
            Velocity -= (MousePos-Position).Normalized()*105f; // Kickback
        }
        
        Velocity = Velocity.LinearInterpolate(Vector2.Zero,0.17f); // Decel player

        bool RightLeft = (Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left"));
        bool UpDown = (Input.IsActionPressed("move_down") || Input.IsActionPressed("move_up"));
        if (Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left")){
            if (UpDown){
                Velocity.x += Speed/1.414f; //Move at sqrt2 when diagonal
            } else {
                Velocity.x += Speed;
            }
        }
        if (Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right")){
            if (UpDown){
                Velocity.x -= Speed/1.414f;
            } else {
                Velocity.x -= Speed;
            }
        }
        if (Input.IsActionPressed("move_down") && !Input.IsActionPressed("move_up")){
            if (RightLeft){
                Velocity.y += Speed/1.414f;
            } else {
                Velocity.y += Speed;
            }
        }
        if (Input.IsActionPressed("move_up") && !Input.IsActionPressed("move_down")){
            if (RightLeft){
                Velocity.y -= Speed/1.414f;
            } else {
                Velocity.y -= Speed;
            }
        }
        MeasuredVelo = Position;
        MoveAndSlide(Velocity*Delta*60,Vector2.Up);
        MeasuredVelo-=Position;
        MeasuredVelo=-MeasuredVelo;

    }
    public Vector2 GetPlayerVelo(){
        return MeasuredVelo;
    }
    public void AddPlayerVelo(Vector2 VeloAdd){
        Velocity += VeloAdd;
    }
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
}
