using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerShip : PlayerShip {

    public float positionThreshold = 1f;
    public float rotationThreshold = 0.5f;

    public float sendInputRate = 0.05f;
    private float nextInputSendTime;

    
    private class ShipSnapshot {
        public float deltaTime;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public ShipSnapshot(float deltaTime, Vector3 position, Quaternion rotation, Vector3 velocity) {
            this.deltaTime = deltaTime;
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
        }
    }
    private List<ShipSnapshot> History = new List<ShipSnapshot>();
    private float historyDuration;

    private Vector3 lastLinearInput = new Vector3();
    private Vector3 lastAngularInput = new Vector3();

    private void Start() {
        nextInputSendTime = Time.time;
        historyDuration = 0f;

        networkController = GameObject.Find("ClientNetworkController");
        if (networkController == null)
            Debug.LogWarning("ERROR! networkController not found");
    }

    private void Update() {
        // get player input for movement
        Vector3 linear_input = new Vector3(0.0f, 0.0f, input.throttle);
        Vector3 angular_input = new Vector3(input.pitch, input.yaw, input.roll);

        // apply movement physics using player input
        physics.SetPhysicsInput(linear_input, angular_input);

        HandleMessagesFromServer();

        // shooting
        HandleShooting();

        AddSnapshotToHistory();

        if (isPlayer)
            activeShip = this;
    }

    private void HandleShooting() {
        if ((Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0)) && Time.time > nextFire) {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }

    private void HandleMessagesFromServer() {
        if (!isServer && incomingQueue.Count != 0) {
            NetMsg netMessage = incomingQueue.Dequeue();
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    SyncPositionWithServer((SC_MovementData)netMessage);
                    break;
                case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                    Destroy(gameObject);
                    break;
                default:
                    Debug.Log("ERROR! LocalPlayerShip on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                    break;
            }
        }
    }

    private void AddSnapshotToHistory() {
        // correct previous snapshot's deltaTime and historyDuration.
        if (History.Count > 0) {
            historyDuration -= History[History.Count - 1].deltaTime;
            historyDuration += Time.deltaTime;
            History[History.Count - 1].deltaTime = Time.deltaTime;
        }
        
        History.Add(new ShipSnapshot(Time.deltaTime, transform.position, transform.rotation, physics.Rigidbody.velocity));
        historyDuration += Time.deltaTime;

        // shouldn't be executed, but just in case - let's limit the History list to 200 snapshots.
        // (if everything goes as should, the History size should be managed when messages are received from the server and be far less than that)
        if (History.Count > 200) {
            History.RemoveAt(0);
        }
        if (History.Count > 1) {
            Debug.Log("Added To History: timeDelta = " + History[History.Count - 1].deltaTime + " position = " + History[History.Count - 1].position + " rotation = " + History[History.Count - 1].rotation);
        }
        
    }

    private void SyncPositionWithServer(SC_MovementData message) {
        float latency = Time.time - message.TimeStamp;
        float dt = Mathf.Max(0, historyDuration - latency);
        float ratio = 0f;
        // remove time from History untill it's duration equals to the latency
        while (History.Count > 0 && dt > 0) {
            if (dt >= History[0].deltaTime) {
                dt -= History[0].deltaTime;
                History.RemoveAt(0);
            }
            else {
                ratio = 1 - (dt / History[0].deltaTime);
                History[0].deltaTime -= dt;
                if (History.Count > 1) {
                    History[0].position = (History[0].position + History[1].position) * ratio;
                    //History[0].rotation =
                    break;
                }
            }
        }
        Debug.Log("Server Message: inputTime = " + message.TimeStamp + " position = " + message.Position + " rotation = " + message.Rotation);

        // 
        /*
        Vector3 historyPosition; // = history[time - L]
        Quaternion historyRotation;

        if ((Vector3.Distance(message.Position, historyPosition) > positionThreshold) ||
            (Quaternion.Angle(message.Rotation, historyRotation) > rotationThreshold) ) {
            CorrectPositionUsingSnapshot(message);




        }
        */
    }

    private void CorrectPositionUsingSnapshot(SC_MovementData message) {
        //transform.position = Vector3.Lerp(transform.position, message.Position, Time.deltaTime);
        //transform.rotation = Quaternion.Lerp(transform.rotation, message.Rotation, Time.deltaTime);
       

    }
}
