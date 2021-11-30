using Godot;
using System;
using System.Collections.Generic;

public class Client : Node2D
{
    private int MyId;
    private WebSocketClient Peer = new WebSocketClient();
    private KinematicBody2D SpecialDummy;
    private PackedScene DummyRes;
    private TextEdit UsernameBox;
    private Node2D HumanHell;
    private List<Vector2> OldMovement = new List<Vector2>(); // Stores predictive movement
    private List<int> LobbyIDs = new List<int>();
    private List<int> PredictiveFrameBuffer = new List<int>();
    private Label HealthLabel;
    private int Health = 100;
    private Node2D Cursor;
    private bool DebugMode = false;
    private Globals Globals;
    
    public override void _Ready()
    {
        Globals = (Globals)GetNode("/root/Globals");
        UsernameBox = (TextEdit)GetNode("CanvasLayer/UsernameEdit");
        HealthLabel = (Label)GetNode("CanvasLayer/Health");
        HumanHell = (Node2D)GetNode("HumanHell");
        DummyRes = (PackedScene)ResourceLoader.Load("res://Scenes/Dummy.tscn");
        SpecialDummy = (KinematicBody2D)GetNode("Player");
        Cursor = (Node2D)GetNode("Cursor");
        Input.SetMouseMode(Input.MouseMode.Hidden);
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) //NON FPS TIED TICK FOR NET-TICK
    {
        
        if (GetTree().NetworkPeer != null && !Input.IsActionPressed("ui_home")){ // Only send RPC if connected to server
            RpcUnreliable("UpdateDummy",SpecialDummy.Position,SpecialDummy.GetAngleTo(GetGlobalMousePosition()) - Mathf.Pi/2,Globals.Velocity);
        } // arg[0] = the id, arg[1] is position, arg[2] is shooting angle, arg[3] is current velo for predictive movement
        //GD.Print(PredictiveFrameBuffer.Count);
        //GD.Print(PredictiveFrameBuffer.Count);
        for (int i = 0; i < PredictiveFrameBuffer.Count; i++)
        {
            if (PredictiveFrameBuffer[i] <= 0){ //Only start to move predictivly after 2 missed frames
                Node2D WorkingDummy = (Node2D)HumanHell.GetNode(LobbyIDs[i].ToString());
                WorkingDummy.Position += OldMovement[i]; // Adds old velo vector ontop of it;
                OldMovement[i] = OldMovement[i].LinearInterpolate(Vector2.Zero,0.05f);
                //GD.Print("DroppedPacketsAlertweewooweewoo");
            } else {
                PredictiveFrameBuffer[i]--;
            }
        }
        if (Health <= 0){
            Health = 100;
            SpecialDummy.Position = Vector2.Zero;
        }
    }
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ToggleFullscreen")){
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
        if (Peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected || Peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connecting){
            Peer.Poll();
        }
        if (Input.IsActionJustPressed("opendebug")){
            DebugMode = !DebugMode;
        } 

        HealthLabel.Text = Health.ToString() + " health.\nGun Name:" + Globals.EquppedGun.Name;
        if (DebugMode){
            HealthLabel.Text+= "\nv1.0a \nAccuracy:" + Mathf.Round(Globals.Accuracy*100)/100;
        }
        Cursor.Position = GetGlobalMousePosition();
        Cursor.Rotation = SpecialDummy.GetAngleTo(GetGlobalMousePosition()) + Mathf.Pi/2;

    }
    public void _StartConnection(){
        var Tree = GetTree();
        if (Tree.NetworkPeer == null){
            //GetTree().NetworkPeer = null;
            //var url = "ws://88.97.17.238:25567" + ""; // You use "ws://" at the beginning of the address for WebSocket connections
            Peer = new WebSocketClient();

            Tree.Connect("network_peer_connected", this, nameof(_OnPeerConnected));
            Tree.Connect("network_peer_disconnected", this, nameof(_OnPeerDisconnected));

            Peer.ConnectToUrl("ws://88.97.17.238:2457",new string[0],true);

            GetTree().NetworkPeer = Peer;
            
            MyId = Tree.NetworkPeer.GetUniqueId();
            GD.Print(MyId + "-netid");

            Rpc("SetUsername",UsernameBox.Text);
            UsernameBox.Hide();
        }
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
            Rpc("SetUsername",UsernameBox.Text);
        }
    }
    public void _OnPeerDisconnected(int NetId){
        GD.Print(NetId, " left.");
        HumanHell.GetNode(NetId.ToString()).QueueFree(); // Kill Dummy
        int LobbyIndex = LobbyIDs.IndexOf(NetId);
        LobbyIDs.RemoveAt(LobbyIndex);
        OldMovement.RemoveAt(LobbyIndex);
        PredictiveFrameBuffer.RemoveAt(LobbyIndex);
    }
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    [Remote]
    public void UpdateDummy(Vector2 PositionUpdate,float RotationUpdate, Vector2 PlayerVelo){
        int NetId = GetTree().GetRpcSenderId();
        int IDindex = LobbyIDs.IndexOf(NetId);
        Node2D WorkingDummy = (Node2D)HumanHell.GetNode(NetId.ToString());
        OldMovement[IDindex] = PlayerVelo; // Sets old movement vector.
        PredictiveFrameBuffer[IDindex] = 2;
        WorkingDummy.Position = PositionUpdate;
        WorkingDummy.Call("SetGunRotation",RotationUpdate);
    }
    [Remote]
    public void DummyTrail(Vector2 StartPos, Vector2 EndPos){
        //int LobbyIndex = LobbyIDs.IndexOf(NetId);
        int NetId = GetTree().GetRpcSenderId();
        HumanHell.GetNode(NetId.ToString()).Call("DummyShoot",StartPos,EndPos);
    }
    [Remote]
    public void ReceiveHit(int Damage,Vector2 Hitback){
        SpecialDummy.Call("AddPlayerVelo",Hitback);
        Health -= Damage;
    }
    [Remote]
    public void SetUsername(string Username){
        int NetId = GetTree().GetRpcSenderId();
        HumanHell.GetNode(NetId.ToString()).Call("SetUsername",Username);
    }
}
