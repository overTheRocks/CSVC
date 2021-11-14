using Godot;
using System;

public class Server : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private int MyId;
    NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
    private Label Label;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Label = (Label)GetNode("Label");

        var Tree = GetTree();
        Tree.Connect("network_peer_connected", this, nameof(_OnPeerConnected));
        Tree.Connect("network_peer_disconnected", this, nameof(_OnPeerDisconnected));
        Peer = new NetworkedMultiplayerENet();
        GetTree().NetworkPeer = null;

        Peer.CreateServer(25567,10);
        GetTree().NetworkPeer = Peer;

        MyId = Tree.NetworkPeer.GetUniqueId();
        GD.Print(MyId + "-netid");
    }
    public void _OnPeerConnected(int NetId){
        GD.Print(NetId, " joined.");
        Label.Text = NetId.ToString();
    }
    public void _OnPeerDisconnected(int NetId){
        GD.Print(NetId, " left.");
    }
    [Remote]
    public void UpdateDummy(int NetId, Vector2 PositionUpdate,float RotationUpdate, Vector2 PlayerVelo){
    
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
