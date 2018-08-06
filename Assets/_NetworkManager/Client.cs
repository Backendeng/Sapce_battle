using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour {

    public GameManager gameManager;

    //PUT THIS INTO A DICT AND THEN processEntityCreated can use it instead of a switch clause
    public GameObject localPlayer;         // player prefab
    public GameObject remotePlayer;
    public GameObject missile;
    public GameObject astroid;
    public GameObject projectile;

    private static string serverIP = "127.0.0.1";
    private static int outPort = 8888;
    private byte error;
    private int unreliableChannelId;
    private int reliableChannelId;
    private int hostId;
    private int connectionId;
    private int clientID = -1;
    private bool playerAvatarCreated = false;
    private static readonly int bufferSize = 1024;
    private byte[] recBuffer = new byte[bufferSize];
    private int dataSize;

    private Dictionary<int, NetworkEntity> netEntities = new Dictionary<int, NetworkEntity>();
    public Dictionary<int, GameObject> mockProjectiles = new Dictionary<int, GameObject>();
    public GameObject mockMissile;

    public int ClientID { get { return clientID; } }
    public static string ServerIP { get { return serverIP; } set { serverIP = value; } }
    public bool PlayerAvatarCreated { get { return playerAvatarCreated; } }

    public struct ClientInitData {
        public static string PlayerName { get; set; }
        public static byte ShipType { get; set; }
    }

    // Use this for initialization
    void Start() {
        Logger.AddPrefix("Client");
        NetworkTransport.Init();
        Connect();
    }

    private void Update() {
        //   Debug.Log("============================================================>> frame: " + Time.frameCount + " time: " + Time.time + " realtime: " + Time.realtimeSinceStartup);
        Logger.Log(Time.time, Time.realtimeSinceStartup, -1, "frame", Time.frameCount.ToString());
        Listen();
    }

    public void Connect() {
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);
        Debug.Log("reliableChannelId open id: " + reliableChannelId);
        unreliableChannelId = config.AddChannel(QosType.Unreliable);
        Debug.Log("unreliableChannelId open id: " + unreliableChannelId);

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, 0);
        Debug.Log("Socket Open. SocketID is: " + hostId);

        connectionId = NetworkTransport.Connect(hostId, serverIP, outPort, 0, out error);
        Debug.Log("Connecting to server. ConnectionId: " + connectionId);
    }

    public void Disconnect() {
        NetworkTransport.Disconnect(hostId, connectionId, out error);
    }

    void OnDestroy() {
        Disconnect();
    }

    public void SendStateToHost(int selfEntityId, Vector3 pos, Quaternion rot, Vector3 vel) {
        //create movementMessage/... and send it to server
        SC_MovementData msg = new SC_MovementData(selfEntityId, Time.fixedTime, pos, rot, vel);//////////////////////////////fixedTime
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        NetworkTransport.Send(hostId, connectionId, unreliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendStateToHost error: " + error.ToString() + " channelID: " + unreliableChannelId);
        Logger.Log(Time.time, Time.realtimeSinceStartup, selfEntityId, "sendState", Time.fixedTime.ToString());
    }

	//TODO remove objType
    public void SendShotToHost(byte shotObjType, Vector3 pos, Quaternion rot, byte shotObjectType, int networkTimeStamp) {
        CS_ProjectileRequest msg = new CS_ProjectileRequest(networkTimeStamp, pos, rot, shotObjectType);
        byte[] buffer = MessagesHandler.NetMsgPack(msg);
        NetworkTransport.Send(hostId, connectionId, unreliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendcreateRequestToHost error: " + error.ToString() + " channelID: " + reliableChannelId);
        Logger.Log(Time.time, Time.realtimeSinceStartup, -1, "sendShot", Time.fixedTime.ToString());
    }

	public void SendMissileToHost(byte shotObjType, Vector3 pos, Quaternion rot, int targetId, int networkTimeStamp) {
		CS_MissileRequest msg = new CS_MissileRequest(networkTimeStamp, pos, rot, targetId);
		byte[] buffer = MessagesHandler.NetMsgPack(msg);
		NetworkTransport.Send(hostId, connectionId, unreliableChannelId, buffer, buffer.Length, out error);
		if (error != 0)
			Debug.LogError("SendcreateMissileToHost error: " + error.ToString() + " channelID: " + reliableChannelId);
		Logger.Log(Time.time, Time.realtimeSinceStartup, -1, "sendMissile", Time.fixedTime.ToString());
	}

    void OnApplicationQuit() {
        Logger.OutputToFile();
        Logger.OutputInterpolationDEBUGToFile();
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
                    break;
                case NetworkEventType.DataEvent:
                    //distribute messages to network entities
                    NetMsg msg = MessagesHandler.NetMsgUnpack(recBuffer);
                    byte type = msg.Type;
                    switch (type) {
                        case (byte)NetMsg.MsgType.SC_AllocClientID:
                            ProcessAllocClientID(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_EntityCreated://BUG: 2nd player gets ERROR, update for netEntity that does not exist in client . maybe he gets state updates for enteties before he created them?
                            ProcessEntityCreated(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_MovementData:
                            ProcessMovementData(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_PlayerData:
                            ProcessPlayerData(msg);
                            break;
                        case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                            ProcessEntityDestroyed(msg);
                            break;
                        case (byte)NetMsg.MsgType.MSG_ShipCreated:
                            ProcessShipCreated(msg);
                            break;
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    if (recHostId == hostId && recConnectionId == connectionId) {
                        Debug.Log("Connected, error:" + error.ToString());
                        if (error == 6) {
                            MainMenu.ErrorNum = 6;
                            MainMenu.LoadScene(0);
                        }
                    }
                    break;
            } // end of switch case

        } while (recData != NetworkEventType.Nothing);

    }

    private void ProcessAllocClientID(NetMsg msg) {
        SC_AllocClientID allocIdMsg = (SC_AllocClientID)msg;

        clientID = allocIdMsg.ClientID;
        Debug.Log("Client got his ID: " + clientID);

        MSG_NewPlayerRequest playerRequestMsg = new MSG_NewPlayerRequest(-1, Time.fixedTime, clientID, ClientInitData.ShipType, ClientInitData.PlayerName);
        byte[] buffer = MessagesHandler.NetMsgPack(playerRequestMsg);
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
        if (error != 0)
            Debug.LogError("SendConnectionAckToHost error: " + error.ToString() + " channelID: " + reliableChannelId);

    }

    private void ProcessEntityCreated(NetMsg msg) {
        SC_EntityCreated createMsg = (SC_EntityCreated)msg;
        int type = createMsg.ObjectType;
        GameObject newObject = null;
        //Debug.Log("Entity Created, ofType: " + type);
        switch (type) {
            case (byte)NetworkEntity.ObjType.Player:
                /////
                break;
            case (byte)NetworkEntity.ObjType.Missile:
                newObject = OnRecieveMissileCreation(createMsg);
                break;
            case (byte)NetworkEntity.ObjType.Astroid:
                newObject = Instantiate(astroid, createMsg.Position, createMsg.Rotation);
                break;
            case (byte)NetworkEntity.ObjType.Projectile:
                newObject = OnReceivedProjectileCreation(createMsg);
                break;
        }
        if (newObject != null) {
            NetworkEntity newNetEntity = newObject.GetComponent<NetworkEntity>();
            newNetEntity.EntityID = createMsg.EntityID;
            newNetEntity.ObjectType = (byte)type;
            newNetEntity.ClientID = createMsg.ClientID;
        }
        else {
            Debug.LogError("Entity Creation failed, id: " + createMsg.EntityID);
        }
        netEntities.Add(createMsg.EntityID, newObject.GetComponent<NetworkEntity>());
        //Debug.Log("Entity Created, id: " + createMsg.EntityID);
    }

    private void ProcessShipCreated(NetMsg msg) {
        MSG_ShipCreated createMsg = (MSG_ShipCreated)msg;
        GameObject newShip = null;

        //TODO: add switch case to create different ships
        if (clientID == createMsg.ClientID) { 
            newShip = Instantiate(localPlayer, createMsg.Position, createMsg.Rotation);//localPlayer
            playerAvatarCreated = true;
        } else {
            newShip = Instantiate(remotePlayer, createMsg.Position, createMsg.Rotation);//remotePlayer
        }

        if (newShip == null) {
            Debug.LogError("Ship Creation failed, id: " + createMsg.EntityID);
            return;
        }
        newShip.GetComponent<PlayerShip>().SetInitShipData(createMsg.EntityID, createMsg.ClientID, createMsg.PlayerName, createMsg.ShipType);
        netEntities.Add(createMsg.EntityID, newShip.GetComponent<NetworkEntity>());//BUG: get key already exists for new player
        gameManager.AddPlayer(createMsg.EntityID, createMsg.ClientID, newShip, createMsg.PlayerName);

        Debug.Log("Player with playerID = " + createMsg.EntityID + ", clientID = " + createMsg.ClientID + ", playerName = " + createMsg.PlayerName + ", shipType = " + createMsg.ShipType + " joined the game.");
    }

    private GameObject OnRecieveMissileCreation(SC_EntityCreated msg) {

        GameObject newMissile = Instantiate(missile, msg.Position, msg.Rotation);
        if (playerAvatarCreated && msg.ClientID == PlayerShip.ActiveShip.EntityID) // check if this player is the target of this missile NOTE: clientID is used for entity ID here
            newMissile.GetComponent<Missile>().IsTargetingPlayer = true;
        return newMissile;
    }

    private GameObject OnReceivedProjectileCreation(SC_EntityCreated msg) {
        GameObject proj = Instantiate(projectile, msg.Position, msg.Rotation);
        proj.GetComponent<Projectile>().OwnerID = gameManager.GetPlayerID(msg.ClientID); // mark the owner of the received (syned from server) projectile
        int key = (int)msg.TimeStamp;
        if (clientID != msg.ClientID) {
            return proj;
        }
        // the projectile is originated from this client
        if (!mockProjectiles.ContainsKey(key)) {
            Debug.LogWarning("mock projectile wasn't found! timestamp = " + key);
        }
        else {
            Destroy(mockProjectiles[key]);
            mockProjectiles.Remove(key);
        }
        return proj;
    }

    private void ProcessMovementData(NetMsg msg) {
        // player creation msg is sent after the other creation nessages when starting this client, so when hes created we can receive updates
        if (!playerAvatarCreated)
            return;
        SC_MovementData moveMsg = (SC_MovementData)msg;
        if (netEntities.ContainsKey(moveMsg.EntityID))
            netEntities[moveMsg.EntityID].AddRecMessage(moveMsg);
        //else
        //Debug.LogWarning("update movement for netEntity that does not exist in client with entityId:" + moveMsg.EntityID);
        Logger.Log(Time.time, Time.realtimeSinceStartup, moveMsg.EntityID, "recState", moveMsg.TimeStamp.ToString());
    }

    private void ProcessEntityDestroyed(NetMsg msg) {
        SC_EntityDestroyed destroyMsg = (SC_EntityDestroyed)msg;
        if (netEntities.ContainsKey(destroyMsg.EntityID)) {
            NetworkEntity netEntityToDestroy = netEntities[destroyMsg.EntityID];
            if (netEntityToDestroy.ObjectType == (byte)NetworkEntity.ObjType.Player) {
                gameManager.RemovePlayer(destroyMsg.EntityID);
                Debug.Log("Ship Destroyed, id: " + destroyMsg.EntityID);
            }
            netEntityToDestroy.AddRecMessage(destroyMsg);
            netEntities.Remove(destroyMsg.EntityID);
            //Debug.Log("Entity Destroyed, id: " + destroyMsg.EntityID);
        }
        else
            Debug.Log("ERROR, destroy for netEntity that does not exist in client with entityId:" + destroyMsg.EntityID);
    }

    private void ProcessPlayerData(NetMsg msg) {
        SC_PlayerData playerDataMsg = (SC_PlayerData)msg;
        gameManager.UpdatePlayerData(playerDataMsg.PlayerID, playerDataMsg.Health, playerDataMsg.Score);
    }

    public int GetShipClientID(int entityID) {
        if (netEntities.ContainsKey(entityID)) {
            return netEntities[entityID].ClientID;
        }
        else {
            return -1;
        }
    }

    public bool IsMissileLockedOnPlayer() {
        foreach(KeyValuePair<int, NetworkEntity> entity in netEntities) {
            if (entity.Value.ObjectType == (byte)NetworkEntity.ObjType.Missile) {
                if (entity.Value.GetComponent<Missile>().IsTargetingPlayer)
                    return true;
            }
        }
        return false;
    }

}
