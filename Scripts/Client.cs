using Godot;
using System;

public class Client : Node2D
{
    private int MyId;
    private NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
    private KinematicBody2D SpecialDummy;
    private PackedScene DummyRes;
    private Node2D HumanHell;
    public override void _Ready()
    {
        HumanHell = (Node2D)GetNode("HumanHell");
        DummyRes = (PackedScene)ResourceLoader.Load("res://Scenes/Dummy.tscn");
        SpecialDummy = (KinematicBody2D)GetNode("Player");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) //NON FPS TIED TICK FOR NET-TICK
    {
        if (GetTree().NetworkPeer != null){
            RpcUnreliable("UpdateDummy",MyId,SpecialDummy.Position,SpecialDummy.Position.AngleTo(GetGlobalMousePosition()));
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
        var Inst = DummyRes.Instance(); //Instance networking dummy
        Inst.Name = NetId.ToString();
        HumanHell.AddChild(Inst);
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
    public void UpdateDummy(int NetId, Vector2 PositionUpdate,float RotationUpdate){
        Node2D WorkingDummy = (Node2D)HumanHell.GetNode(NetId.ToString());
        WorkingDummy.Position = PositionUpdate;
        WorkingDummy.Rotation = RotationUpdate;
    }
}
