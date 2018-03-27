using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkEntity : MonoBehaviour {

    Queue<NetworkMessage> incomingQueue = new Queue<NetworkMessage>();

    void Input()
    {
        Debug.Log("incoming Input");
    }

}
