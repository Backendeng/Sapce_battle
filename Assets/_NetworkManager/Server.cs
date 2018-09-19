using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

    public GameManager gameManager;
    public float sendRate = 0.05f;
    private int timeStep = 0;
    public int rate = 2;//2 for 0.06f 

    int unreliableChannelId;
    int reliableChannelId;
    int hostId;
    public static int inPort = 8888;
    int connectionId;
    static int bufferSize = 1024;
    int dataSize;
    static byte[] recBuffer = new byte[bufferSize];

    Dictionary<int, GameObject> connectedPlayers = new Dictionary<int, GameObject>();
    Queue<NetMsg> outgoingReliable = new Queue<NetMsg>();
    Queue<NetMsg> outgoingUnReliable = new Queue<NetMsg>();

    //move these to some kind of dict by object type  FACTOR THIS OUT
    public GameObject remotePlayer1;   // player prefab                  TODO: spawner
    public GameObject remotePlayer2;   // player prefab
    public GameObject remotePlayer3;   // player prefab
    public Transform playerSpawn;     // player spawn location
    public GameObject missile;
    public GameObject projectile;

    EntityManager entityManager;

    //debug
    private float lastSendTime;

    // Use this for initialization
    void Start ()
    {
        Application.targetFrameRate = 60;

        Logger.AddPrefix("Server");
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);
        Debug.Log("reliableChannelId open id: " + reliableChannelId);
        unreliableChannelId = config.AddChannel(QosType.Unreliable);
        Debug.Log("unreliableChannelId open id: " + unreliableChannelId);


        int maxConnections = 20;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, inPort);
        if (hostId == -1) {
            MainMenu.ErrorNum = 666;
            MainMenu.LoadScene(0);
            return;
        }
        Debug.Log("Socket Open. SocketId is: " + hostId);


        entityManager = new EntityManager();
        InitializeWorld();

        AudioListener.pause = true;
    }

    private void FixedUpdate() {
        //Debug.Log("============================================================>> frame: " + Time.frameCount + " time: " + Time.time + " realtime: " + Time.realtimeSinceStartup);
        Logger.Log(Time.time, Time.realtimeSinceStartup, -1, "frame", Time.frameCount.ToString());

        Listen();//first listen
        StageAllEntities();// put all entity positions and rotations on queue
        BroadcastAllMessages(outgoingUnReliable, unreliableChannelId);//then broadcast unreliable
        BroadcastAllMessages(outgoingReliable, reliableChannelId);//then broadcast reliable

    }

    public void EnqueueReliable(NetMsg message) {
        outgoingReliable.Enqueue(message);
    }

    public void CloseServer() {
        NetworkTransport.RemoveHost(hostId);
        Debug.Log("Socked " + hostId + " Closed.");
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

    void OnApplicationQuit() {
        Logger.OutputToFile();
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
                    //Debug.Log("incoming connection event received id: " + recConnectionId);
                    ProccessConnectionRequest(recConnectionId);
                    break;
                case NetworkEventType.DataEvent:       //3
                     //Debug.Log("incoming message event received from: " + recConnectionId);
                    NetMsg msg = MessagesHandler.NetMsgUnpack(recBuffer);
                    switch (msg.Type) {
                        case (byte)NetMsg.MsgType.SC_MovementData:
                            ProccessStateMessage(msg, recConnectionId);
                            break;
                        case (byte)NetMsg.MsgType.MSG_NewPlayerRequest:
                            ProccessNewPlayerRequest((MSG_NewPlayerRequest)msg, recConnectionId);
                            break;
                        case (byte)NetMsg.MsgType.CS_ProjectileRequest://this is a request from client to create a shot TODO: change to shot request
							CreateRequestedProjectile((CS_ProjectileRequest)msg, recConnectionId);
                            break;
						case (byte)NetMsg.MsgType.CS_MissileRequest://this is a request from client to create a missile
							CreateRequestedMissile((CS_MissileRequest)msg, recConnectionId);
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
        if (!connectedPlayers.ContainsKey(recConnectionId)) {
            Debug.LogWarning("clientID = " + recConnectionId + "does not exist in the connected players dict!");
            return;
        }
        //Debug.Log("rec ts: " + ((SC_MovementData)msg).TimeStamp);///////////////////////////////////////////////
        NetworkEntity entity = connectedPlayers[recConnectionId].GetComponent<NetworkEntity>();
        entity.AddRecMessage(msg);
        Logger.Log(Time.time, Time.realtimeSinceStartup, entity.EntityID, "rec", ((SC_MovementData)msg).TimeStamp.ToString());
    }

    private void ProccessNewPlayerRequest(MSG_NewPlayerRequest msg, int recConnectionId) {
        //send all netEntites to new player 
        SendAllEntitiesToNewClient(recConnectionId);
        gameManager.SendAllGameDataToNewClient(recConnectionId);

        int entityId;
      
        GameObject remotePlayer;
        switch (msg.ShipType) {
            case 1:
                remotePlayer = remotePlayer1;
                break;
            case 2:
                remotePlayer = remotePlayer2;
                break;
            case 3:
                remotePlayer = remotePlayer3;
                break;
            default:
                remotePlayer = remotePlayer1;
                break;
        }

        GameObject newShip = entityManager.CreateEntity(remotePlayer, playerSpawn.position, playerSpawn.rotation, (byte)NetworkEntity.ObjType.Player, out entityId);
        newShip.GetComponent<PlayerShip>().SetInitShipData(entityId, recConnectionId, msg.PlayerName, msg.ShipType);

        gameManager.AddPlayer(entityId, recConnectionId, newShip, msg.PlayerName);
        entityManager.netEntities[entityId].ClientID = recConnectionId;
        connectedPlayers[recConnectionId] = newShip;

        //broadcast new entity to all
        MSG_ShipCreated msg1 = new MSG_ShipCreated(entityId, Time.time, playerSpawn.position, playerSpawn.rotation, recConnectionId, msg.ShipType, msg.PlayerName);
        outgoingReliable.Enqueue(msg1);

        Debug.Log("Player with playerID = " + newShip.GetComponent<PlayerShip>().PlayerID + ", clientID = " + recConnectionId + ", playerName = " + msg.PlayerName + ", shipType = " + msg.ShipType + " joined the game.");

        gameManager.scoreBoard.RefreshScoreBoard();
    }

    private void ProccessDisconnection(int recConnectionId) {
        if (!connectedPlayers.ContainsKey(recConnectionId)) {
            Debug.LogError("clientID = " + recConnectionId + "does not exist in the connected players dict!");
            return;
        }

        int entityIdToDestroy = connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().EntityID;

        SC_EntityDestroyed destroyMsg = new SC_EntityDestroyed(entityIdToDestroy, Time.time);
        outgoingReliable.Enqueue(destroyMsg); //send destroy to all 
        entityManager.netEntities[entityIdToDestroy].AddRecMessage(destroyMsg); //destroy on server

        connectedPlayers.Remove(recConnectionId);//end connection
        entityManager.RemoveEntity(entityIdToDestroy);

        gameManager.RemovePlayer(entityIdToDestroy);//TODO: should we keep the player data in case he comes back? if a player connects with the same name we could copy his data to the new playerID
    }

	private void CreateRequestedMissile(CS_MissileRequest msg, int clientID) {
		byte error;
		int newEntityID = -1;
		float msgDelayTime = (float)NetworkTransport.GetRemoteDelayTimeMS(hostId, clientID, (int)msg.TimeStamp, out error) / 1000;
		Vector3 position = msg.Position + (msg.Rotation * Vector3.forward * Missile.Speed * msgDelayTime * 5);
		//Debug.Log("msg delay time for shot: " + msgDelayTime);

        GameObject newObject = entityManager.CreateEntity(missile, position, msg.Rotation, (byte)NetworkEntity.ObjType.Missile, out newEntityID);

		if (newObject == null || newEntityID == -1)
			Debug.LogError("Entity Creation failed");

        entityManager.netEntities[newEntityID].GetComponent<Missile>().OwnerID = gameManager.GetPlayerID(clientID);// mark the owner of this missile
        int tergetPlayerID = -1;
        if (msg.TargetId > 0) {////more proofing needed
            entityManager.netEntities[newEntityID].GetComponent<Missile>().Target = entityManager.netEntities[msg.TargetId].transform; // mark the target of this missile
            entityManager.netEntities[newEntityID].GetComponent<Missile>().TargetScript = entityManager.netEntities[msg.TargetId].GetComponent<PlayerShip>();
            tergetPlayerID = msg.TargetId;
        }
        SC_EntityCreated mssg = new SC_EntityCreated(newEntityID, msg.TimeStamp, position, msg.Rotation, tergetPlayerID, (byte)NetworkEntity.ObjType.Missile);
		outgoingReliable.Enqueue(mssg);
	}

	private void CreateRequestedProjectile(CS_ProjectileRequest msg, int clientID) {
		byte error;
		int newEntityID = -1;
		float msgDelayTime = (float)NetworkTransport.GetRemoteDelayTimeMS(hostId, clientID, (int)msg.TimeStamp, out error) / 1000;
		Vector3 position = msg.Position + (msg.Rotation * Vector3.forward * Projectile.Speed * msgDelayTime );
		//Debug.Log("msg delay time for shot: " + msgDelayTime);

		GameObject newObject = entityManager.CreateEntity(projectile, position, msg.Rotation, (byte)NetworkEntity.ObjType.Projectile, out newEntityID);

		if (newObject == null || newEntityID == -1)
			Debug.LogError("Entity Creation failed");

		entityManager.netEntities[newEntityID].GetComponent<Projectile>().OwnerID = gameManager.GetPlayerID(clientID); // mark the owner of this projectile
        SC_EntityCreated mssg = new SC_EntityCreated(newEntityID, msg.TimeStamp, msg.Position, msg.Rotation, clientID/*no use*/, (byte)NetworkEntity.ObjType.Projectile); // TODO: change msg.Position to position and test
        outgoingReliable.Enqueue(mssg);
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

    private float lastSend;
    private float lastreal;
    //this is called once every sendRate time and puts the positions of all entities on the queue
    private float nextSendTime = 0;
    private void StageAllEntities() {
        if (Time.time < nextSendTime) 
            return;
        nextSendTime = Time.time + sendRate;

        //Debug.Log("====================send interval: " + (Time.time - lastSend));
        //Debug.Log("====================real send interval: " + (Time.realtimeSinceStartup - lastreal));
        lastSend = Time.time;
        lastreal = Time.realtimeSinceStartup;

        foreach (KeyValuePair<int, NetworkEntity> entity in entityManager.netEntities) {// nullref
            if (entity.Value == null) {
                Debug.LogWarning("trying to access entity that's missing from the netEntities dictionary");
                continue;
            }

            if(entity.Value.ObjectType == (byte)NetworkEntity.ObjType.Player) {
                SC_MovementData msg = entity.Value.GetNextSnapshot(entity.Key);
                while (msg != null) {
                    //Debug.Log("sent ts: "+ msg.TimeStamp);///////////////////////////////////////////////
                    //Logger.Log(Time.time, Time.realtimeSinceStartup, entity.Key, "sent", msg.TimeStamp.ToString());
                    outgoingUnReliable.Enqueue(msg);
                    msg = entity.Value.GetNextSnapshot(entity.Key);
                }
            } else {
                Vector3 pos = entity.Value.gameObject.GetComponent<Transform>().position;
                Quaternion rot = entity.Value.gameObject.GetComponent<Transform>().rotation;
                Vector3 vel = entity.Value.GetVelocity();

                SC_MovementData msg = new SC_MovementData(entity.Key, Time.time, pos, rot, vel);

                outgoingUnReliable.Enqueue(msg);
            }

           
        }
    }

    private void SendAllEntitiesToNewClient(int connectionId) {
        byte error;
        //send all enteties as createMsg to new client
        foreach (KeyValuePair<int, NetworkEntity> entity in entityManager.netEntities) {
            NetMsg msg;
            Vector3 pos = entity.Value.gameObject.GetComponent<Transform>().position;
            Quaternion rot = entity.Value.gameObject.GetComponent<Transform>().rotation;
            int clientID = -1;
            if (entity.Value.ObjectType == (byte)NetworkEntity.ObjType.Player) {
                clientID = entity.Value.ClientID;
                if (!connectedPlayers.ContainsKey(clientID)) {
                    Debug.LogError("clientID = " + clientID + "does not exist in the connected players dict!");
                    return;
                }
                PlayerShip ship = connectedPlayers[clientID].GetComponent<PlayerShip>();
                msg = new MSG_ShipCreated(entity.Key, Time.time, pos, rot, clientID, ship.ShipType, ship.PlayerName);
            }
            else {
                msg = new SC_EntityCreated(entity.Key, Time.time, pos, rot, clientID, entity.Value.ObjectType);
            }
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

    public void SendMsgToClient(int clientID, NetMsg msg) {
        byte error;
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        NetworkTransport.Send(hostId, clientID, reliableChannelId, buffer, buffer.Length, out error);
    }


    public void RespawnPlayer(int clientID) {
        connectedPlayers[clientID].GetComponent<RemotePlayerShipServer>().Respawn();
    }

    public float GetNearestDistFrom(Transform p, int self) {
        float minDist = -1;
        foreach (KeyValuePair<int, GameObject> client in connectedPlayers) {
            if (client.Key == self)
                continue;
            float d = Vector3.Distance(client.Value.transform.position, p.position);
            if (d < minDist || minDist == -1) {
                minDist = d;
            }
        }
        return minDist;
    }

    // returns null if can't get a ship for the given playerID
    public PlayerShip GetShipOnServer(int playerID) {
        if (!gameManager.IsValidPlayerID(playerID)) {
            Debug.Log("playerID = " + playerID + "is not a valid player ID");
            return null;
        }
        int clientID = gameManager.GetClientID(playerID);

        if (!connectedPlayers.ContainsKey(clientID)) {
            Debug.LogError("clientID = " + clientID + "does not exist in the connected players dict!");
            return null;
        }
        if (connectedPlayers[clientID] == null) {
            Debug.Log("ship of clientID = " + clientID + " is not ready yet");
            return null;
        }
        return connectedPlayers[clientID].GetComponent<PlayerShip>(); //nullref
    }

}

 
