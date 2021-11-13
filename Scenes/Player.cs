using Godot;
using System;

public class Player : KinematicBody2D
{
    private Sprite PlayerSprite;
    private Sprite GunSprite;
    private PackedScene GunParticles;
    private Vector2 Velocity = Vector2.Zero; 
    private AudioStreamPlayer2D ShootSound;
    private const float Speed = 69;
    public override void _Ready()
    {
        PlayerSprite = (Sprite)GetNode("PlayerSprite");
        GunSprite = (Sprite)GetNode("GunSprite");
        GunParticles = (PackedScene)ResourceLoader.Load("res://Scenes/GunParticles.tscn");
        ShootSound = (AudioStreamPlayer2D)GetNode("GunSprite/ShootSound");
    }
    public override void _Process(float Delta)
    {
        Vector2 MousePos = GetGlobalMousePosition();
        GunSprite.Rotation = GetAngleTo(MousePos) - Mathf.Pi/2;
        
        GunSprite.Offset = GunSprite.Offset.LinearInterpolate(new Vector2(0,13),0.2f); 
        
        if (Input.IsActionJustPressed("FireGun")){
            Particles2D GunParticlesInstance = (Particles2D)GunParticles.Instance();
            GunParticlesInstance.Position = (MousePos-Position).Normalized()*15f;
            GunParticlesInstance.Rotation = GunSprite.Rotation;
            AddChild(GunParticlesInstance);
            GunSprite.Offset = new Vector2(GunSprite.Offset.x,0);
            ShootSound.Play();
            Velocity -= (MousePos-Position).Normalized()*105f; // Kickback
        }
        

        bool RightLeft = (Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left"));
        bool UpDown = (Input.IsActionPressed("move_down") || Input.IsActionPressed("move_up"));
        if (Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left")){
            if (UpDown){
                Velocity.x += Speed/1.414f;
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
        MoveAndSlide(Velocity*Delta*60,Vector2.Up);
        Velocity = Velocity.LinearInterpolate(Vector2.Zero,0.17f);

    }
}
