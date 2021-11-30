using Godot;
using System;

public class Server : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private int MyId;
    //WebSocketServer Peer;
    private Label Label;
    WebSocketServer WSServer;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Label = (Label)GetNode("Label");
        GetTree().NetworkPeer = null;
        var Tree = GetTree();

        WSServer = new WebSocketServer();
        Tree.Connect("network_peer_connected", this, nameof(_OnPeerConnected));
        Tree.Connect("network_peer_disconnected", this, nameof(_OnPeerDisconnected));
        //Tree.Connect("connection_error", this, nameof(_OnPeerDisconnected));

        //
        string[] EmptyArgs = new string[0];
        GD.Print(WSServer.Listen(2457,EmptyArgs,true));

        GetTree().NetworkPeer = WSServer;

        //MyId = Tree.NetworkPeer.GetUniqueId();
        //GD.Print(MyId + "-netid");
    }
    public override void _Process(float delta){
        if (WSServer.IsListening()){
            WSServer.Poll();
        }
    } 
    public void _OnPeerConnected(int NetId){
        GD.Print(NetId, " joined.");
        Label.Text = NetId.ToString();
    }
    public void _OnPeerDisconnected(int NetId){
        GD.Print(NetId, " left.");
    }
    [Remote]
    public void UpdateDummy(Vector2 PositionUpdate,float RotationUpdate, Vector2 PlayerVelo){
    
    }
    [Remote]
    public void DummyTrail(Vector2 StartPos, Vector2 EndPos){
        
    }
    [Remote]
    public void ReceiveHit(int Damage,Vector2 Hitback){
        
    }
    [Remote]
    public void SetUserName(string Username){
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
