using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    private Sprite PlayerSprite;
    private Sprite GunSprite;
    private PackedScene GunParticles;
    private PackedScene Bullet;
    private Vector2 Velocity = Vector2.Zero; 
    private AudioStreamPlayer2D ShootSound;
    private const float Speed = 137; //nice
    private Node2D BulletHell;
    public Vector2 MeasuredVelo = Vector2.Zero;
    public float Accuracy = 0;
    private Node2D Cursor;
    private Random RNG = new Random();
    private AnimationPlayer PlayerAnim;
    private AudioStreamPlayer2D HitSound;
    private Globals Globals;
    private bool QueueShoot;
    private int ShotCooldownFrames;
    public override void _Ready()
    {
        Globals = (Globals)GetNode("/root/Globals");
        Cursor = (Node2D)GetNode("../Cursor");
        HitSound = (AudioStreamPlayer2D)GetNode("HitSound");
        PlayerAnim = (AnimationPlayer)GetNode("AnimationPlayer");
        BulletHell = (Node2D)GetNode("../BulletHell");
        PlayerSprite = (Sprite)GetNode("PlayerSprite");
        GunSprite = (Sprite)GetNode("GunSprite");
        GunParticles = (PackedScene)ResourceLoader.Load("res://Scenes/GunParticles.tscn");
        Bullet = (PackedScene)ResourceLoader.Load("res://Scenes/Bullet.tscn");
        ShootSound = (AudioStreamPlayer2D)GetNode("GunSprite/ShootSound");
        PlayerAnim.Play("Hit");
    }
    public override void _PhysicsProcess(float delta)
    {
        Vector2 MousePos = GetGlobalMousePosition();
        Accuracy = Accuracy * 0.98f;
        Accuracy = Accuracy - 1;
        Accuracy = Mathf.Clamp(Accuracy,0,500);
        Globals.Accuracy = Accuracy;
        Sprite CursorL = (Sprite)Cursor.GetNode("L");
        Sprite CursorR = (Sprite)Cursor.GetNode("R");
        CursorL.Offset = CursorL.Offset.LinearInterpolate(new Vector2(-500-Accuracy*50-5,-260),0.8f);
        CursorR.Offset = CursorR.Offset.LinearInterpolate(new Vector2(Accuracy*50-5,-260),0.8f);
        if (ShotCooldownFrames<=0){
            Cursor.Modulate = new Color(1,1,1);
        } else {
            Cursor.Modulate = new Color(1,0.6f,0.6f);
        }
        if (Input.IsActionJustPressed("FireGun") || (Input.IsActionPressed("FireGun") && Globals.EquppedGun.AutoReshoot)){
            QueueShoot = true;
        }
        if (!Input.IsActionPressed("FireGun") && Globals.EquppedGun.AutoReshoot){
            QueueShoot = false;
        }
        if (QueueShoot && ShotCooldownFrames<=0){
            ShotCooldownFrames = Globals.EquppedGun.ReshootFrames;
            QueueShoot = false;
            float BulletDirection = GetAngleTo(MousePos)+(((float)RNG.NextDouble()-0.5f)*Accuracy/100);
            // Instance shooting particles:
            Accuracy += Globals.EquppedGun.AccuracyRecoil;

            CPUParticles2D GunParticlesInstance = (CPUParticles2D)GunParticles.Instance();
            GunParticlesInstance.Position = (MousePos-Position).Normalized()*15f;
            GunParticlesInstance.Rotation = GunSprite.Rotation;
            AddChild(GunParticlesInstance); // Add particles as child
            for (int i = 0; i < Globals.EquppedGun.BulletsPerShot; i++){
                Node2D BulletInstance = (Node2D)Bullet.Instance();
                BulletInstance.Call("SetDestination",RadianToVector2(BulletDirection+(((float)RNG.NextDouble()-0.5f)*Globals.EquppedGun.MultiBulletSpread/100))*(Globals.EquppedGun.Range+(((float)RNG.NextDouble()-0.5f)*Globals.EquppedGun.BulletRangeRange)),false);
                BulletInstance.Position = Position;
                BulletHell.AddChild(BulletInstance);
            }
            GunSprite.Offset = new Vector2(GunSprite.Offset.x,0); // Sprite Kickback
            ShootSound.Play();
            Velocity -= (MousePos-Position).Normalized()*Globals.EquppedGun.Kickback; // Kickback
        }
        if (ShotCooldownFrames>0){
            ShotCooldownFrames--;
        }
    }
    public override void _Process(float Delta)
    {
        Vector2 MousePos = GetGlobalMousePosition();
        Globals.Velocity = MeasuredVelo;
        GunSprite.Rotation = GetAngleTo(MousePos) - Mathf.Pi/2; // Mathf.pi/2 is 90 degrees
        
        GunSprite.Offset = GunSprite.Offset.LinearInterpolate(new Vector2(0,13),0.2f);  // Reset Gun kickback over time
        
        

        
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

    public void AddPlayerVelo(Vector2 VeloAdd){
        Velocity += VeloAdd;
        HitSound.Play();
        PlayerAnim.Play("Hit");
    }
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

}
