using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

    public float sendRate = 0.05f;
    private float nextStagingTime;


    int unreliableChannelId;
    int reliableChannelId;
    int hostId;
    static int inPort = 8888;
    int connectionId;
    static int bufferSize = 1024;
    int dataSize;
    static byte[] recBuffer = new byte[bufferSize];

    Dictionary<int, GameObject> connectedPlayers = new Dictionary<int, GameObject>();
    Queue<NetMsg> outgoingReliable = new Queue<NetMsg>();
    Queue<NetMsg> outgoingUnReliable = new Queue<NetMsg>();


    //move these to some kind of dict by object type  FACTOR THIS OUT
    public GameObject remotePlayer;   // player prefab                  TODO: spawner
    public Transform playerSpawn;     // player spawn location
    public GameObject missile;
    public GameObject projectile;

    EntityManager entityManager;

    //debug
    private float lastSendTime; 

    // Use this for initialization
    void Start ()
    {
        nextStagingTime = Time.time;
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);
        Debug.Log("reliableChannelId open id: " + reliableChannelId);
        unreliableChannelId = config.AddChannel(QosType.Unreliable);
        Debug.Log("unreliableChannelId open id: " + unreliableChannelId);


        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, inPort);
        Debug.Log("Socket Open. SocketId is: " + hostId);


        entityManager = new EntityManager();
        InitializeWorld();
    }

    void InitializeWorld() {
        GameObject[] worldEntities = GameObject.FindGameObjectsWithTag("WorldNetEntities");

        foreach (GameObject entity in worldEntities) {
            int id = entityManager.RegisterEntity(entity);
            Transform tr = entity.transform;

            //broadcast new entity to all
            SC_EntityCreated msg1 = new SC_EntityCreated(id, Time.time, tr.position, tr.rotation, -1, entity.GetComponent<NetworkEntity>().ObjectType);
            outgoingReliable.Enqueue(msg1);
        }
    }

    // Update is called once per frame
    private void Update ()
    {
        Listen();//first listen
        StageAllEntities();// put all entity positions and rotations on queue
        BroadcastAllMessages(outgoingUnReliable, unreliableChannelId);//then broadcast unreliable
        BroadcastAllMessages(outgoingReliable, reliableChannelId);//then broadcast reliable

    }

    private void Listen()
    {
        NetworkEventType recData = NetworkEventType.Nothing;
        byte error;
        int recHostId;
        int recConnectionId;
        int channelId;

        do {
            System.Array.Clear(recBuffer, 0, dataSize);
            recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData) {
                case NetworkEventType.Nothing:         //1
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    Debug.Log("incoming connection event received id: " + recConnectionId);
                    ProccessConnectionRequest(recConnectionId);
                    break;
                case NetworkEventType.DataEvent:       //3
                     //Debug.Log("incoming message event received from: " + recConnectionId);
                    NetMsg msg = MessagesHandler.NetMsgUnpack(recBuffer);
                    switch (msg.Type) {
                        case (byte)NetMsg.MsgType.SC_MovementData:
                            ProccessStateMessage(msg, recConnectionId);
                            break;
                        case (byte)NetMsg.MsgType.SC_AllocClientID:
                            ProccessAllocClientID((SC_AllocClientID)msg, recConnectionId);
                            break;
                        case (byte)NetMsg.MsgType.CS_CreationRequest://this is a request from client to create an object
                            ProccessEntityCreateRequest(msg, recConnectionId);
                            break;
                    }
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    Debug.Log("remote client event disconnected id: " + recConnectionId);
                    ProccessDisconnection(recConnectionId); //BUG: servers sends data for a non existing entity after cliend disconnects
                    break;
            } // end of switch case

        } while (recData != NetworkEventType.Nothing);
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

    private void ProccessStateMessage(NetMsg msg, int recConnectionId) {
        //process message and send pos and rot to playerObject on this server 

        connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().AddRecMessage(msg);
    }

    private void ProccessAllocClientID(SC_AllocClientID msg, int recConnectionId) {

        //send all netEntites to new player 
        SendAllEntitiesToNewClient(recConnectionId);

        int entityId;
        GameObject newPlayer = entityManager.CreateEntity(remotePlayer, playerSpawn.position, playerSpawn.rotation, (byte)NetworkEntity.ObjType.Player, out entityId);
        newPlayer.GetComponent<RemotePlayerShipServer>().ClientID = recConnectionId;
        connectedPlayers[recConnectionId] = newPlayer;

        //broadcast new entity to all
        SC_EntityCreated msg1 = new SC_EntityCreated(entityId, Time.time, playerSpawn.position, playerSpawn.rotation, recConnectionId, (byte)NetworkEntity.ObjType.Player);
        outgoingReliable.Enqueue(msg1);
    }

    private void ProccessDisconnection(int recConnectionId) {
        if (!connectedPlayers.ContainsKey(recConnectionId))
            return;

        int entityIdToDestroy = connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().EntityID;

        SC_EntityDestroyed destroyMsg = new SC_EntityDestroyed(entityIdToDestroy, Time.time);
        outgoingReliable.Enqueue(destroyMsg); //send destroy to all 
        entityManager.netEntities[entityIdToDestroy].AddRecMessage(destroyMsg); //destroy on server

        connectedPlayers.Remove(recConnectionId);//end connection
        entityManager.RemoveEntity(entityIdToDestroy);
    }

    // when a client wants to create an obj he sends CS_CreationRequest to the server and the server creates it and then distributes the new object
    private void ProccessEntityCreateRequest(NetMsg msg, int clientID) {
        CS_CreationRequest createMsg = (CS_CreationRequest)msg;
        //Debug.Log("Object creation request received: objType: " + createMsg.ObjectType + " from " + createMsg.ClientID);
        byte objectType = createMsg.ObjectType;
        GameObject newObject = null;
        int entityId = -1;
        switch (objectType) {
            case (byte)NetworkEntity.ObjType.Missile://TODO: mark this obj as originated from clientID
                newObject = entityManager.CreateEntity(missile, createMsg.Position, createMsg.Rotation, (byte)NetworkEntity.ObjType.Missile, out entityId);
                break;
            case (byte)NetworkEntity.ObjType.Projectile://TODO: mark this obj as originated from clientID
                newObject = CreateRequestedProjectile(createMsg, clientID, out entityId);
                break;
            case (byte)NetworkEntity.ObjType.Player:
                Debug.LogError("Entity Creation failed, client should not request to createa player object ,id: ");
                break;
        }
        if (newObject == null || entityId == -1)
            Debug.LogError("Entity Creation failed");

        SC_EntityCreated mssg = new SC_EntityCreated(entityId, createMsg.TimeStamp, createMsg.Position, createMsg.Rotation, clientID, objectType);
        outgoingReliable.Enqueue(mssg);
        //Debug.Log("Entity Created, id: " + entityId);
    }

    private void BroadcastAllMessages(Queue<NetMsg> queue, int channelId) {
        if (queue.Count == 0)
            return;
        byte error;

        float start = Time.realtimeSinceStartup;
        int size = 0;

        foreach (NetMsg msg in queue) {
            byte[] buffer = MessagesHandler.NetMsgPack(msg);
            if (buffer.Length > size)
                size = buffer.Length;
            foreach (KeyValuePair<int, GameObject> client in connectedPlayers) {
                NetworkTransport.Send(hostId, client.Key, channelId, buffer, buffer.Length, out error);
            }
        }
        queue.Clear();

        //Debug.Log("Queue cleared, time: " + Time.time + " duration " + (Time.realtimeSinceStartup - start) + " dt: " + (Time.time - lastSendTime) + " size " + size);
        lastSendTime = Time.time;
    }

    //this is called once every sendRate time and puts the positions of all entities on the queue
    private void StageAllEntities() {
        if (Time.time < nextStagingTime)
            return;

        nextStagingTime = Time.time + sendRate;

        foreach (KeyValuePair<int, NetworkEntity> entity in entityManager.netEntities) {
            if (entity.Value == null) {
                Debug.LogWarning("trying to access entity that's missing from the netEntities dictionary");
                continue;
            }
            Vector3 pos = entity.Value.gameObject.GetComponent<Transform>().position;
            Quaternion rot = entity.Value.gameObject.GetComponent<Transform>().rotation;
            float lastRecStateTime = entity.Value.LastReceivedStateTime;
            SC_MovementData msg = new SC_MovementData(entity.Key, lastRecStateTime, pos, rot);

            outgoingUnReliable.Enqueue(msg);

        }
    }

    private void SendAllEntitiesToNewClient(int connectionId) {
        byte error;
        //send all enteties as createMsg to new client
        foreach (KeyValuePair<int, NetworkEntity> entity in entityManager.netEntities) {
            Vector3 pos = entity.Value.gameObject.GetComponent<Transform>().position;
            Quaternion rot = entity.Value.gameObject.GetComponent<Transform>().rotation;

            SC_EntityCreated msg = new SC_EntityCreated(entity.Key, Time.time, pos, rot, -1, entity.Value.ObjectType);
            byte[] buffer = MessagesHandler.NetMsgPack(msg);
            NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        }
    }

    public void DestroyEntity(int entityID) {
        //Debug.Log("destroying entity id = " + entityID);
        SC_EntityDestroyed destroyMsg = new SC_EntityDestroyed(entityID, Time.time);
        outgoingReliable.Enqueue(destroyMsg); //send destroy to all 
        entityManager.netEntities[entityID].AddRecMessage(destroyMsg); //destroy on server
        entityManager.RemoveEntity(entityID);
    }

    private GameObject CreateRequestedProjectile(CS_CreationRequest msg, int clientID, out int entityID) {
        byte error;
        int newEntityID = -1;
        float msgDelayTime = (float)NetworkTransport.GetRemoteDelayTimeMS(hostId, clientID, (int)msg.TimeStamp, out error) / 1000;
        Vector3 position = msg.Position + (msg.Rotation * Vector3.forward * Projectile.Speed * msgDelayTime );
        //Debug.Log("msg delay time for shot: " + msgDelayTime);

        GameObject newObject = entityManager.CreateEntity(projectile, position, msg.Rotation, (byte)NetworkEntity.ObjType.Projectile, out newEntityID);
        if (newEntityID == -1) {
            entityID = -1;
            return null;
        }
        entityID = newEntityID;
        entityManager.netEntities[entityID].GetComponent<Projectile>().ClientID = clientID; // mark the owner of this projectile
        return newObject;
    }

}

 
