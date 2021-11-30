using Godot;
using System;
using System.Collections.Generic;

public class Globals : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public List<Gun> Guns = new List<Gun>();
    public Gun EquppedGun;

    public float Accuracy;
    private int EquippedGunID;
    public Vector2 Velocity;

    public class Gun{
        public string Name = "Unnamed Gun"; // Gun name
        public float Range = 1500; // Range bullet goes
        public float AccuracyRecoil = 15; // Amount of unacuracy each shot adds
        public float Kickback = 100; // Physical Kickback
        public float Damage = 10; // Damage inflicted
        public float DamageMaxRange = 4; // Max +- damage range
        public float DamageFallOff = 3000; //Distance for damage to be 0% 
        public int ReshootFrames = 9; // Amount of frames that pass before being able to reshoot
        public bool AutoReshoot = false; // If gun is auto
        public float Screenshake = 0.3f;
        public string SoundEffect = "Pistol"; // Sound effect name
        public float BulletRangeRange = 200;
        public int BulletsPerShot = 1;
        public float MultiBulletSpread = 0;
    }
    public override void _Ready()
    {
        Guns.Add(new Gun(){
            Name = "Glock"
        });
        Guns.Add(new Gun(){
            Name = "Ak-47",
            Range = 2300,
            AccuracyRecoil = 17,
            Kickback = 130,
            Damage = 26,
            DamageMaxRange = 7,
            DamageFallOff = 7000,
            ReshootFrames = 7,
            AutoReshoot = true,
            Screenshake = 0.1f,
            SoundEffect = "Rifle"
        });
        Guns.Add(new Gun(){
            Name = "Nova",
            Range = 500,
            AccuracyRecoil = 40,
            Kickback = 860,
            Damage = 20,
            DamageMaxRange = 4,
            DamageFallOff = 850,
            ReshootFrames = 35,
            AutoReshoot = true,
            Screenshake = 0.3f,
            SoundEffect = "Shotgun",
            BulletsPerShot = 7,
            MultiBulletSpread = 20f,
            BulletRangeRange = 100
        });
        EquppedGun = Guns[2];
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

}
