using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

    public float sendRate = 0.05f;
    private float nextStagingTime;


    int reliableChannelId;
    int hostId;
    int inPort = 8888;
    int connectionId;

    int lastEntityId;
    Dictionary<int, GameObject> connectedPlayers = new Dictionary<int, GameObject>();
    Dictionary<int, NetworkEntity> netEntities = new Dictionary<int, NetworkEntity>();
    Queue<NetMsg> outgoingMessages = new Queue<NetMsg>();

    public GameObject remotePlayer;         // player prefab                  TODO: spawner
    public Transform playerSpawn;     // player spawn location

    // Use this for initialization
    void Start ()
    {
        nextStagingTime = Time.time;
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, inPort);
        Debug.Log("Socket Open. SocketId is: " + hostId);

        lastEntityId = 1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Listen();//first listen
        StageAllEntities();// put all entity positions and rotations on queue
        BroadcastAllMessages();//then broadcast

    }

    void OnDestroy() {
        Disconnect();
    }

    public void Disconnect() {
        byte error;
        NetworkTransport.Disconnect(hostId, connectionId, out error);
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
            case NetworkEventType.Nothing:         //1
                break;
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("incoming connection event received id: " + recConnectionId);
                ProccessConnectionRequest(recConnectionId);
                break;
            case NetworkEventType.DataEvent:       //3
                Debug.Log("incoming message event received from: " + recConnectionId);
                //TODO: interpolation data will be sent as regular pos update, is this fast enough?
                NetMsg msg = MessagesHandler.NetMsgUnpack(recBuffer);
                switch (msg.Type) {
                    case (byte)NetMsg.MsgType.SC_AllocClientID:
                        ProccessAllocClientID((SC_AllocClientID)msg, recConnectionId);
                    break;
                    case (byte)NetMsg.MsgType.CS_InputData:
                        ProccessInputMessage(msg, recConnectionId);
                    break;
                }

                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("remote client event disconnected id: " + recConnectionId);
                ProccessDisconnection(recConnectionId); //BUG: servers sends data for a non existing entity after cliend disconnects
                break;
        }
    }

    private void ProccessConnectionRequest(int recConnectionId) {
        byte error;
        if (!connectedPlayers.ContainsKey(recConnectionId)) {

            connectedPlayers.Add(recConnectionId, null);
            
            //send new player his ID (the server's recConnectedID is used as the clientID)
            SC_AllocClientID msg = new SC_AllocClientID(-1, Time.fixedTime, recConnectionId);
            byte[] buffer = MessagesHandler.NetMsgPack(msg);
            NetworkTransport.Send(hostId, recConnectionId, reliableChannelId, buffer, buffer.Length, out error);
            if (error != 0)
                Debug.LogError("Client ID alocation error: " + error.ToString());

        }
    }

    private void ProccessInputMessage(NetMsg mssg, int recConnectionId) {
        //process message and send input to playerObject on this server 
       
        connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().AddRecMessage(mssg);
    }

    private void ProccessAllocClientID(SC_AllocClientID msg, int recConnectionId) {

        //send all netEntites to new player 
        SendAllEntitiesToClient(recConnectionId);

        GameObject newPlayer = Instantiate(remotePlayer, playerSpawn.position, playerSpawn.rotation);
        newPlayer.GetComponent<NetworkEntity>().EntityID = lastEntityId++;
        connectedPlayers[recConnectionId] = newPlayer;
        netEntities.Add(lastEntityId, newPlayer.GetComponent<NetworkEntity>());

        //broadcast new entity to all
        SC_EntityCreated msg1 = new SC_EntityCreated(lastEntityId, Time.fixedTime, playerSpawn.position, playerSpawn.rotation, recConnectionId);
        outgoingMessages.Enqueue(msg1);
    }

    private void ProccessDisconnection(int recConnectionId) {
        int entityIdToDestroy = connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().EntityID;
        SC_EntityDestroyed destroyMsg = new SC_EntityDestroyed(entityIdToDestroy, Time.fixedTime);
        outgoingMessages.Enqueue(destroyMsg); //send destroy to all 
        netEntities[entityIdToDestroy].AddRecMessage(destroyMsg);
        connectedPlayers.Remove(recConnectionId);
        netEntities.Remove(entityIdToDestroy);
    }

    private void BroadcastAllMessages() {
        if (outgoingMessages.Count == 0)
            return;
        byte error;
        foreach (KeyValuePair<int, GameObject> client in connectedPlayers) {
            foreach (NetMsg msg in outgoingMessages) {
                byte[] buffer = MessagesHandler.NetMsgPack(msg);
                NetworkTransport.Send(hostId, client.Key, reliableChannelId, buffer, buffer.Length, out error);
            }
        }
        outgoingMessages.Clear();
    }


    //this is called once every sendRate time and puts the positions of all entities on the queue
    private void StageAllEntities() {
        if (Time.time < nextStagingTime)
            return;

        nextStagingTime = Time.time + sendRate;

        foreach (KeyValuePair<int, NetworkEntity> entity in netEntities) {
            Vector3 pos = entity.Value.gameObject.GetComponent<Transform>().position;
            Quaternion rot = entity.Value.gameObject.GetComponent<Transform>().rotation;
            float lastInputTime = entity.Value.gameObject.GetComponent<RemotePlayerShip>().LastRecievedInputTime;
            SC_MovementData msg = new SC_MovementData(entity.Key, lastInputTime, pos, rot);

            outgoingMessages.Enqueue(msg);
        }

    }

    private void SendAllEntitiesToClient(int connectionId) {
        byte error;

        foreach (KeyValuePair<int, NetworkEntity> entity in netEntities) {
            Vector3 pos = entity.Value.gameObject.GetComponent<Transform>().position;
            Quaternion rot = entity.Value.gameObject.GetComponent<Transform>().rotation;
            SC_EntityCreated msg = new SC_EntityCreated(entity.Key, Time.fixedTime, pos, rot, -1);
            byte[] buffer = MessagesHandler.NetMsgPack(msg);

            NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        }
    }
}
