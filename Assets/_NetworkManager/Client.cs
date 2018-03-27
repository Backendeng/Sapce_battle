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
    public Transform playerSpawn;     // player spawn location


    // Use this for initialization
    void Start ()
    {
        NetworkTransport.Init();
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

    public void SendHost()
    {
        //create movementMessage/... and send it to server

       // byte[] buffer = new byte[1024];
       // Stream stream = new MemoryStream(buffer);
        //BinaryFormatter formatter = new BinaryFormatter();
      //  formatter.Serialize(stream, "HelloServer");

        //int bufferSize = 1024;

       // NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, bufferSize, out error);
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
                GameObject newPlayer = Instantiate(player, playerSpawn.position, playerSpawn.rotation); //spawn local player upon connection
                break;
            case NetworkEventType.DataEvent:
                //distribute messages to network entities
                //if we got a createMessage then  Instantiate(player,  ...

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
}
