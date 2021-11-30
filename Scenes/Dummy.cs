using Godot;
using System;

public class Dummy : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private Sprite PlayerSprite;
    private Sprite GunSprite;
    private PackedScene Bullet;
    private PackedScene GunParticles;
    private AudioStreamPlayer2D ShootSound;
    private Node2D BulletHell;
    private Label UsernameLabel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UsernameLabel = (Label)GetNode("UserName");
        PlayerSprite = (Sprite)GetNode("PlayerSprite");
        GunSprite = (Sprite)GetNode("GunSprite");
        GunParticles = (PackedScene)ResourceLoader.Load("res://Scenes/GunParticles.tscn");
        Bullet = (PackedScene)ResourceLoader.Load("res://Scenes/Bullet.tscn");
        ShootSound = (AudioStreamPlayer2D)GetNode("GunSprite/ShootSound");
        BulletHell = (Node2D)GetParent();
    }

    public void DummyShoot(Vector2 StartPos, Vector2 EndPos){
        GunSprite.Offset = new Vector2(GunSprite.Offset.x,0);
        ShootSound.Play();
        CPUParticles2D GunParticlesInstance = (CPUParticles2D)GunParticles.Instance();
        GunParticlesInstance.Position = EndPos.Normalized()*15f;
        GunParticlesInstance.Rotation = GetAngleTo(EndPos+Position) - Mathf.Pi/2;
        AddChild(GunParticlesInstance); // Add particles as child
        Node2D BulletInst = (Node2D)Bullet.Instance();
        BulletInst.Call("SetDestination",EndPos,true);
        BulletInst.GlobalPosition = GlobalPosition;
        BulletHell.AddChild(BulletInst);
    }
    public void SetGunRotation(float Radians){
        GunSprite.Rotation = Radians;
    }
    public override void _Process(float delta){
        GunSprite.Offset = GunSprite.Offset.LinearInterpolate(new Vector2(0,13),0.2f);
    }
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public void SetUsername(string Username){
        UsernameLabel.Text = Username;
    }
}
