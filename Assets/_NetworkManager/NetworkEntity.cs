using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkEntity : MonoBehaviour {

    Queue<NetMsg> incomingQueue = new Queue<NetMsg>();

    public GameObject networkController;
    protected Vector3 linear_input_sent;
    protected Vector3 angular_input_sent;
    protected int entityID = -1;
    public bool isServer = true;

    public int EntityID {
        get { return entityID; }
        set { entityID = value; }
    }

    public void RemoteInput(Vector3 linear_input, Vector3 angular_input)
    {
        linear_input_sent = linear_input;
        angular_input_sent = angular_input;
        Debug.Log("incoming Input");
    }

    public void AddRecMessage(NetMsg msg) {
        incomingQueue.Enqueue(msg);
    }

}
