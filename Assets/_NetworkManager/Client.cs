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

    Dictionary<int, NetworkEntity> netEntities = new Dictionary<int, NetworkEntity>();

    public GameObject localPlayer;         // player prefab
    public GameObject remotePlayer;

    // Use this for initialization
    void Start ()
    {
        NetworkTransport.Init();
        Connect();
    }

    public void Connect()
    {
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

    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostId, connectionId, out error);
    }

    void OnDestroy() {
        Disconnect();
    }

    public void SendInputToHost(int selfEntityId, float throttle, Vector3 angular_input)
    {
        //create movementMessage/... and send it to server
        CS_InputData msg = new CS_InputData(selfEntityId, Time.time, angular_input, throttle);
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendInputToHost error: " + error.ToString() + " channelID: " + reliableChannelId);
    }

    // Update is called once per frame
    void Update ()
    {
        Listen();
    }

    private void Listen()
    {
        int recHostId;
        int recConnectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                if (recHostId == hostId &&
                    recConnectionId == connectionId &&
                   (NetworkError)error == NetworkError.Ok)
                {
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
                  recConnectionId == connectionId)
                {
                    Debug.Log("Connected, error:" + error.ToString());
                }
                break;
        }
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
        GameObject newPlayer;
        SC_EntityCreated createMsg = (SC_EntityCreated)msg;
        if (clientID == createMsg.ClientID) { //when adding new types of objects, check this only for ship object.
            newPlayer = Instantiate(localPlayer, createMsg.Position, createMsg.Rotation);//localPlayer
        } else {
            newPlayer = Instantiate(remotePlayer, createMsg.Position, createMsg.Rotation);//remotePlayer
        }
       
        newPlayer.GetComponent<NetworkEntity>().EntityID = createMsg.EntityID;
        netEntities.Add(createMsg.EntityID, newPlayer.GetComponent<NetworkEntity>());
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
