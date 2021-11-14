using Godot;
using System;
using System.Collections.Generic;

public class Client : Node2D
{
    private int MyId;
    private NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
    private KinematicBody2D SpecialDummy;
    private PackedScene DummyRes;
    private Node2D HumanHell;
    private List<Vector2> OldMovement = new List<Vector2>();
    private List<int> LobbyIDs = new List<int>();
    private List<int> PredictiveFrameBuffer = new List<int>();
    public override void _Ready()
    {
        HumanHell = (Node2D)GetNode("HumanHell");
        DummyRes = (PackedScene)ResourceLoader.Load("res://Scenes/Dummy.tscn");
        SpecialDummy = (KinematicBody2D)GetNode("Player");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) //NON FPS TIED TICK FOR NET-TICK
    {
        if (GetTree().NetworkPeer != null && !Input.IsActionPressed("ui_home")){ // Only send RPC if connected to server
            RpcUnreliable("UpdateDummy",MyId,SpecialDummy.Position,SpecialDummy.GetAngleTo(GetGlobalMousePosition()) - Mathf.Pi/2,SpecialDummy.Call("GetPlayerVelo"));
        } // arg[0] = the id, arg[1] is position, arg[2] is shooting angle, arg[3] is current velo for predictive movement
        //GD.Print(PredictiveFrameBuffer.Count);
        //GD.Print(PredictiveFrameBuffer.Count);
        for (int i = 0; i < PredictiveFrameBuffer.Count; i++)
        {
            
            if (PredictiveFrameBuffer[i] <= 0){ //Only act from 0-40 missed moverment frames before just giving up lol
                Node2D WorkingDummy = (Node2D)HumanHell.GetNode(LobbyIDs[i].ToString());
                WorkingDummy.Position += OldMovement[i]; // Adds old velo vector ontop of it;
                OldMovement[i] = OldMovement[i].LinearInterpolate(Vector2.Zero,0.05f);
                //GD.Print("DroppedPacketsAlertweewooweewoo");
            } else {
                PredictiveFrameBuffer[i]--;
            }
        }
    }
    public void _StartConnection(){
        var Tree = GetTree();
        Tree.Connect("network_peer_connected", this, nameof(_OnPeerConnected));
        Tree.Connect("network_peer_disconnected", this, nameof(_OnPeerDisconnected));

        GetTree().NetworkPeer = null;

        Peer.CreateClient("88.97.17.238",25567);

        GetTree().NetworkPeer = Peer;
        
        MyId = Tree.NetworkPeer.GetUniqueId();
        GD.Print(MyId + "-netid");
    }
    public void _OnPeerConnected(int NetId){
        GD.Print(NetId, " joined.");
        if (NetId != 1){ //Dont create server dummy (netid 1 is server)
            var Inst = DummyRes.Instance(); //Instance networking dummy
            Inst.Name = NetId.ToString();
            HumanHell.AddChild(Inst);
            LobbyIDs.Add(NetId);
            OldMovement.Add(Vector2.Zero);
            PredictiveFrameBuffer.Add(60);
        }
    }
    public void _OnPeerDisconnected(int NetId){
        GD.Print(NetId, " left.");
        HumanHell.GetNode(NetId.ToString()).QueueFree(); // Kill Dummy
    }
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    [Remote]
    public void UpdateDummy(int NetId, Vector2 PositionUpdate,float RotationUpdate, Vector2 PlayerVelo){
        int IDindex = LobbyIDs.IndexOf(NetId);
        Node2D WorkingDummy = (Node2D)HumanHell.GetNode(NetId.ToString());
        OldMovement[IDindex] = PlayerVelo; // Sets old movement vector.
        PredictiveFrameBuffer[IDindex] = 2;
        WorkingDummy.Position = PositionUpdate;
        WorkingDummy.Rotation = RotationUpdate;
    }
}
