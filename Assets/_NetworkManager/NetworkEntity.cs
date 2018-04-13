using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkEntity : MonoBehaviour {

    protected Queue<NetMsg> incomingQueue = new Queue<NetMsg>();

    protected GameObject networkControllerObj;
    protected Server serverController;
    protected Client clientController;
    protected int entityID = -1;
    public bool isServer;

    protected float lastReceivedStateTime;
    public float LastReceivedStateTime { get { return lastReceivedStateTime; } }

    public enum ObjType : byte {
        Player,
        Missile,
        Astroid,
    }
    protected byte objectType;
    public byte ObjectType {
        get { return objectType; }
        set { objectType = value; }
    }

    // History
    protected class StateSnapshot {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public StateSnapshot(float time, Vector3 position, Quaternion rotation, Vector3 velocity) {
            this.time = time;
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
        }
    }
    private List<StateSnapshot> History = new List<StateSnapshot>();
    private static int historySize = 200;

    public int EntityID {
        get { return entityID; }
        set { entityID = value; }
    }

    public void Start() {
        // setting up serverController / clientController referecnce 
        networkControllerObj = GameObject.Find("ServerNetworkController");
        if (networkControllerObj != null) {
            isServer = true;
            serverController = networkControllerObj.GetComponent<Server>();
            if (serverController == null) {
                Debug.LogError("server controller wasn't found for network entity = " + entityID);
            }
        }
        else {
            networkControllerObj = GameObject.Find("ClientNetworkController");
            if (networkControllerObj != null) {
                isServer = false;
                clientController = networkControllerObj.GetComponent<Client>();
                if (clientController == null) {
                    Debug.LogError("client controller wasn't found for network entity = " + entityID);
                }
            }
            else {
                Debug.LogWarning("ERROR! networkController not found for network entity = " + entityID);
            }
        }

        lastReceivedStateTime = -1f;
    }

    private void FixedUpdate() {
        if (incomingQueue.Count > 100) {
            Debug.LogWarning("Warning: incomingQueue size > 100 for network entity = " + entityID);
        }
    }

    public void AddRecMessage(NetMsg msg) {
        incomingQueue.Enqueue(msg);
       // Debug.Log("incoming message to NetworkEntity");
    }

    protected void AddSnapshotToHistoryOnClient(SC_MovementData message) {

        History.Add(new StateSnapshot(message.TimeStamp, message.Position, message.Rotation, new Vector3() ));///TODO:pass velocity

        // let's limit thje History size
        if (History.Count > historySize) {
            History.RemoveAt(0);
        }
    }

    protected StateSnapshot GetLastSnapshotAt(int idx) {
        return History[idx];
    }

    protected int GetHistoryLastIdx() {
        return History.Count - 1;
    }

    public virtual Vector3 GetVelocity() {
        return GetComponent<Rigidbody>().velocity; // overridden in RemotePlayerShip to return last received velocity (because it's rigid body has 0 velocity)
    }

    private void OnDestroy() {
        if (isServer) {
           
        }
    }


}
