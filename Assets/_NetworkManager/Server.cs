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
    Queue<NetworkMessage> outgoingMessages = new Queue<NetworkMessage>();

    public GameObject player;         // player prefab
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
            case NetworkEventType.Nothing:         //1
                break;
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("incoming connection event received");

                if (!connectedPlayers.ContainsKey(recConnectionId))
                {
                    GameObject newPlayer = Instantiate(player, playerSpawn.position, playerSpawn.rotation);
                    connectedPlayers.Add(recConnectionId, newPlayer);
                    netEntities.Add(lastEntityId++, newPlayer.GetComponent<NetworkEntity>());
                }
                //broadcast new entity to all
                break;
            case NetworkEventType.DataEvent:       //3
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                Debug.Log("incoming message event received: " + message);

                //process message and send input to playerObject on this server
                string[] splitFloats = message.Split('|');
                float linX = float.Parse(splitFloats[0]);
                float linY = float.Parse(splitFloats[1]);
                float linZ = float.Parse(splitFloats[2]);
                float rotX = float.Parse(splitFloats[3]);
                float rotY = float.Parse(splitFloats[4]);
                float rotZ = float.Parse(splitFloats[5]);
                Vector3 linear_in = new Vector3(linX, linY, linZ);
                Vector3 rot_in = new Vector3(rotX, rotY, rotZ);
                connectedPlayers[recConnectionId].GetComponent<NetworkEntity>().RemoteInput(linear_in, rot_in);
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("remote client event disconnected");
                break;
        }
    }

}
