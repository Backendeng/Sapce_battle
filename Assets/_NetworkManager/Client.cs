using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour {

    public string serverIP = "127.0.0.1";
    byte error;
    int reliableChannelId;
    int hostId;
    int outPort = 8888;
    int connectionId;
    int clientID = -1;
    static int bufferSize = 1024;
    byte[] recBuffer = new byte[bufferSize];
    int dataSize;

    Dictionary<int, NetworkEntity> netEntities = new Dictionary<int, NetworkEntity>();

    public GameObject localPlayer;         // player prefab
    public GameObject remotePlayer;
    public GameObject missile;

    // Use this for initialization
    void Start() {
        NetworkTransport.Init();
        Connect();
    }

    public void Connect() {
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);
        Debug.Log("Channel open id: " + reliableChannelId);

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, 0);
        Debug.Log("Socket Open. HostId is: " + hostId);

        connectionId = NetworkTransport.Connect(hostId, serverIP, outPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId);
    }

    public void Disconnect() {
        NetworkTransport.Disconnect(hostId, connectionId, out error);
    }

    void OnDestroy() {
        Disconnect();
    }

    public void SendStateToHost(int selfEntityId, Vector3 pos, Quaternion rot) {
        //create movementMessage/... and send it to server
        SC_MovementData msg = new SC_MovementData(selfEntityId, Time.time, pos, rot);
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendStateToHost error: " + error.ToString() + " channelID: " + reliableChannelId);
    }

    public void SendMissileShotToHost(int selfEntityId, Vector3 pos, Quaternion rot) {
        //create movementMessage/... and send it to server
        Debug.Log("shoot" + (int)NetworkEntity.ObjType.Missile);
        SC_EntityCreated msg = new SC_EntityCreated(-1 , Time.time, pos, rot, clientID, (byte)NetworkEntity.ObjType.Missile);
        Debug.Log("shootMsg " + msg.ObjectType);
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        SC_EntityCreated m = (SC_EntityCreated)MessagesHandler.NetMsgUnpack(buffer);
        Debug.Log("shootMsg " + m.ObjectType);
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendcreateRequestToHost error: " + error.ToString() + " channelID: " + reliableChannelId);
    }

    // Update is called once per frame
    private void Update() {
        Listen();
    }

    private void Listen() {
        NetworkEventType recData = NetworkEventType.Nothing;
        byte error;
        int recHostId;
        int recConnectionId;
        int channelId;

        do {
            System.Array.Clear(recBuffer, 0, dataSize);
            recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData) {
                case NetworkEventType.ConnectEvent:
                    if (recHostId == hostId &&
                        recConnectionId == connectionId &&
                       (NetworkError)error == NetworkError.Ok) {
                        Debug.Log("Connected");
                    }
                    //problem: how to pass entity id here?
                    break;
                case NetworkEventType.DataEvent:
                    //distribute messages to network entities
                    NetMsg msg = MessagesHandler.NetMsgUnpack(recBuffer);
                    byte type = msg.Type;
                    switch (type) {
                        case (byte)NetMsg.MsgType.SC_AllocClientID:
                            ProccessAllocClientID(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_EntityCreated://BUG: 2nd player gets ERROR, update for netEntity that does not exist in client 
                            ProccessEntityCreated(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_MovementData:
                            ProccessMovementData(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                            ProccessEntityDestroyed(msg);
                            break;
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    if (recHostId == hostId &&
                      recConnectionId == connectionId) {
                        Debug.Log("Connected, error:" + error.ToString());
                    }
                    break;
            } // end of switch case

        } while (recData != NetworkEventType.Nothing);

    }

    private void ProccessAllocClientID(NetMsg msg) {
        SC_AllocClientID allocIdMsg = (SC_AllocClientID)msg;

        clientID = allocIdMsg.ClientID;
        Debug.Log("Client got his ID: " + clientID);

        SC_AllocClientID ack = new SC_AllocClientID(-1, Time.fixedTime, clientID);
        byte[] buffer = MessagesHandler.NetMsgPack(ack);
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendInputToHost error: " + error.ToString() + " channelID: " + reliableChannelId);

    }

    private void ProccessEntityCreated(NetMsg msg) {
        SC_EntityCreated createMsg = (SC_EntityCreated)msg;
        int type = createMsg.ObjectType;
        GameObject newObject = null;
        Debug.Log("Entity Created, ofType: " + type);
        switch (type) {
            case (byte)NetworkEntity.ObjType.Player:
                if (clientID == createMsg.ClientID) { 
                    newObject = Instantiate(localPlayer, createMsg.Position, createMsg.Rotation);//localPlayer
                } else {
                    newObject = Instantiate(remotePlayer, createMsg.Position, createMsg.Rotation);//remotePlayer
                }
                break;
            case (byte)NetworkEntity.ObjType.Missile:
                newObject = Instantiate(missile, createMsg.Position, createMsg.Rotation);//missile
                break;

        }
        if (newObject != null)
            newObject.GetComponent<NetworkEntity>().EntityID = createMsg.EntityID;
        else
            Debug.LogError("Entity Creation failed, id: " + createMsg.EntityID);
        netEntities.Add(createMsg.EntityID, newObject.GetComponent<NetworkEntity>());
        Debug.Log("Entity Created, id: " + createMsg.EntityID);
    }

    private void ProccessMovementData(NetMsg msg) {
        SC_MovementData moveMsg = (SC_MovementData)msg;
        if (netEntities.ContainsKey(moveMsg.EntityID))
            netEntities[moveMsg.EntityID].AddRecMessage(moveMsg);
        else
            Debug.Log("ERROR, update for netEntity that does not exist in client with entityId:" + moveMsg.EntityID);
    }

    private void ProccessEntityDestroyed(NetMsg msg) {
        SC_EntityDestroyed destroyMsg = (SC_EntityDestroyed)msg;
        if (netEntities.ContainsKey(destroyMsg.EntityID)) {
            netEntities[destroyMsg.EntityID].AddRecMessage(destroyMsg);
            netEntities.Remove(destroyMsg.EntityID);
            Debug.Log("Entity Destroyed, id: " + destroyMsg.EntityID);
        }
        else
            Debug.Log("ERROR, destroy for netEntity that does not exist in client with entityId:" + destroyMsg.EntityID);
    }

    }
