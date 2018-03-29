using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour {

    byte error;
    int reliableChannelId;
    int hostId;
    int outPort = 8888;
    int connectionId;

    Dictionary<int, NetworkEntity> netEntities = new Dictionary<int, NetworkEntity>();

    public GameObject player;         // player prefab


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

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, 0);
        Debug.Log("Socket Open. HostId is: " + hostId);

        connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", outPort, 0, out error);
        Debug.Log("Connected to server. ConnectionId: " + connectionId);
    }

    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostId, connectionId, out error);
    }

    public void SendInputToHost(int selfEntityId, float throttle, Vector3 angular_input)
    {
        //create movementMessage/... and send it to server
        CS_InputData msg = new CS_InputData(selfEntityId, Time.fixedTime, angular_input, throttle);
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
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
                // GameObject newPlayer = Instantiate(player, playerSpawn.position, playerSpawn.rotation); //spawn local player upon connection
                //problem: how to pass entity id here?
                break;
            case NetworkEventType.DataEvent:
                //distribute messages to network entities
                ProcessMessage(recBuffer, recConnectionId);
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

    private void ProcessMessage(byte[] buffer, int recConnectionId) {
        NetMsg msg = MessagesHandler.NetMsgUnpack(buffer);
        byte type = msg.Type;
        GameObject newPlayer;
        switch (type) {
            case (byte)NetMsg.MsgType.SC_EntityCreated:
                SC_EntityCreated createMsg = (SC_EntityCreated)msg;
                if (createMsg.connectionID == recConnectionId) 
                    newPlayer = Instantiate(player, createMsg.position, createMsg.rotation);//localPlayer TODO: change prefab
                else
                    newPlayer = Instantiate(player, createMsg.position, createMsg.rotation);//remotePlayer
                newPlayer.GetComponent<NetworkEntity>().EntityID = createMsg.entityID;
                netEntities.Add(createMsg.entityID, newPlayer.GetComponent<NetworkEntity>());
                break;
            case (byte)NetMsg.MsgType.SC_MovementData:
                SC_MovementData moveMsg = (SC_MovementData)msg;
                if (netEntities.ContainsKey(moveMsg.entityID))
                    netEntities[moveMsg.entityID].AddRecMessage(moveMsg);
                else
                    Debug.Log("ERROR, update for netEntity that does not exist in client " + moveMsg.entityID);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                SC_EntityDestroyed destroyMsg = (SC_EntityDestroyed)msg;
                if (netEntities.ContainsKey(destroyMsg.entityID)) {
                    netEntities[destroyMsg.entityID].AddRecMessage(destroyMsg);
                    netEntities.Remove(destroyMsg.entityID);
                }
                else
                    Debug.Log("ERROR, destroy for netEntity that does not exist in client " + destroyMsg.entityID);
                break;
        }
    }
}
