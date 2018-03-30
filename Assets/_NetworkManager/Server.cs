using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

    int reliableChannelId;
    int hostId;
    int outPort = 8888;
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
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        reliableChannelId = config.AddChannel(QosType.Reliable);

        int maxConnections = 10;
        HostTopology topology = new HostTopology(config, maxConnections);

        hostId = NetworkTransport.AddHost(topology, outPort);
        Debug.Log("Socket Open. SocketId is: " + hostId);

        lastEntityId = 1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Listen();//first listen
        BroadcastState();//then broadcast

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

                if (!connectedPlayers.ContainsKey(recConnectionId))
                {
                    GameObject newPlayer = Instantiate(remotePlayer, playerSpawn.position, playerSpawn.rotation);
                    lastEntityId++;
                    newPlayer.GetComponent<NetworkEntity>().EntityID = lastEntityId;
                    connectedPlayers.Add(recConnectionId, newPlayer);
                    netEntities.Add(lastEntityId, newPlayer.GetComponent<NetworkEntity>());

                    //broadcast new entity to all
                    SC_EntityCreated msg = new SC_EntityCreated(lastEntityId, Time.fixedTime, playerSpawn.position, playerSpawn.rotation, recConnectionId);
                    outgoingMessages.Enqueue(msg);
                }
                break;
            case NetworkEventType.DataEvent:       //3
                Debug.Log("incoming message event received from: " + recConnectionId);
                //TODO: interpolation data will be sent as regular pos update, is this fast enough?
                //process message and send input to playerObject on this server 
                NetMsg mssg = MessagesHandler.NetMsgUnpack(recBuffer);
                connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().AddRecMessage(mssg);
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("remote client event disconnected id: " + recConnectionId);
                //send destroy to all 
                int entityIdToDestroy = connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().EntityID;
                SC_EntityDestroyed destroyMsg = new SC_EntityDestroyed(entityIdToDestroy, Time.fixedTime);
                outgoingMessages.Enqueue(destroyMsg);
                connectedPlayers.Remove(recConnectionId);
                netEntities.Remove(entityIdToDestroy);
                break;
        }
    }

    // send world state to all clients.
    private void BroadcastState() {
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


}
