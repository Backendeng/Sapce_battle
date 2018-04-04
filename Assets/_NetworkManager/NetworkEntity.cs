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
    public bool isServer;

    // History
    private class ShipSnapshot {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public ShipSnapshot(float time, Vector3 position, Quaternion rotation, Vector3 velocity) {
            this.time = time;
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
        }
    }
    private List<ShipSnapshot> History = new List<ShipSnapshot>();
    private static int historySize = 200;

    public int EntityID {
        get { return entityID; }
        set { entityID = value; }
    }

    public void AddRecMessage(NetMsg msg) {
        incomingQueue.Enqueue(msg);
       // Debug.Log("incoming message to NetworkEntity");
    }

    private void AddSnapshotToHistory() {

        History.Add(new ShipSnapshot(Time.time, transform.position, transform.rotation, GetVelocity() ));

        // let's limit thje History size
        if (History.Count > historySize) {
            History.RemoveAt(0);
        }
    }

    public virtual Vector3 GetVelocity() {
        return GetComponent<Rigidbody>().velocity; // overridden in RemotePlayerShip to return last received velocity (because it's rigid body has 0 velocity)
    }


}
