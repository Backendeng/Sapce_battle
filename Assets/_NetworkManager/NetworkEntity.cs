using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkEntity : MonoBehaviour {

    protected Queue<NetMsg> incomingQueue = new Queue<NetMsg>();

    protected GameObject networkController;
    protected int entityID = -1;
    public bool isServer = true;

    public int EntityID {
        get { return entityID; }
        set { entityID = value; }
    }

    public void AddRecMessage(NetMsg msg) {
        incomingQueue.Enqueue(msg);
       // Debug.Log("incoming message to NetworkEntity");
    }

    private void Start() {
        if (isServer) {
            networkController = GameObject.Find("ServerNetworkController");
        }
        else {
            networkController = GameObject.Find("ClientNetworkController");
        }
        if (networkController == null)
            Debug.Log("ERROR! networkController not found");
    }

}
