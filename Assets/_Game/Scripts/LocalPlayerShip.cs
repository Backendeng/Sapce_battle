using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerShip : PlayerShip {

    public float positionThreshold = 0.3f;
    public float rotationThreshold = 5f;

    public float sendInputRate = 0.05f;
    private float nextInputSendTime;

    
    private class ShipSnapshot {
        public float time;
        public float deltaTime;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 deltaPosition;

        public ShipSnapshot(float time, float deltaTime, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 dp) {
            this.time = time;
            this.deltaTime = deltaTime;
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
            this.deltaPosition = dp;
        }
    }
    private List<ShipSnapshot> History = new List<ShipSnapshot>();
    private float historyDuration;

    private Vector3 lastLinearInput = new Vector3();
    private Vector3 lastAngularInput = new Vector3();

    public GameObject shadowPrefab;
    private Transform shadow;

    private float lastReturnedInputTime;
    private float latency;

    private void Start() {
        nextInputSendTime = Time.time;


        networkController = GameObject.Find("ClientNetworkController");
        if (networkController == null)
            Debug.LogWarning("ERROR! networkController not found");

        if (shadowPrefab != null)
            shadow = Instantiate(shadowPrefab, new Vector3(), new Quaternion()).transform;
    }

    private void Update() {
        /*
        if (Input.GetButton("Jump")) {
            Debug.Log("History: " + History);
            foreach (ShipSnapshot snapshot in History) {
                Debug.Log("time = " + snapshot.time + " timeDelta = " + snapshot.deltaTime + " position = " + snapshot.position + " rotation = " + snapshot.rotation);
            }
            
        }*/

        // get player input for movement
        Vector3 linearInput = new Vector3(0.0f, 0.0f, input.throttle);
        Vector3 angularInput = new Vector3(input.pitch, input.yaw, input.roll);

        // apply movement physics using player input
        physics.SetPhysicsInput(linearInput, angularInput);

       

        HandleMessagesFromServer();

        // shooting
        HandleShooting();
        SendInputToServer(linearInput, angularInput);
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

    private void SendInputToServer(Vector3 linearInput, Vector3 angularInput) {
        if (Time.time > nextInputSendTime /*&& (linearInput != lastLinearInput || lastAngularInput != angularInput)*/) {
            networkController.GetComponent<Client>().SendInputToHost(entityID, input.throttle, angularInput);
            lastLinearInput = linearInput;
            lastAngularInput = angularInput;
            nextInputSendTime = Time.time + sendInputRate;

            //Debug.Log("Input send time = " + Time.time);
        }
        
    }

    private void HandleMessagesFromServer() {
        if (!isServer && incomingQueue.Count != 0) {
            NetMsg netMessage = incomingQueue.Dequeue();
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    SyncPositionWithServer((SC_MovementData)netMessage);
                    MoveShadow((SC_MovementData)netMessage);
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

        Vector3 dp = new Vector3();
        if (History.Count > 0) {
            dp = transform.position - History[History.Count - 1].position;
        } 
        History.Add(new ShipSnapshot(Time.time, Time.deltaTime, transform.position, transform.rotation, physics.Rigidbody.velocity, dp));
        historyDuration += Time.deltaTime;

        // shouldn't be executed, but just in case - let's limit the History list to 200 snapshots.
        // (if everything goes as should, the History size should be managed when messages are received from the server and be far less than that)
        if (History.Count > 200) {
            History.RemoveAt(0);
        }
        if (History.Count > 0) {
            //Debug.Log("Added To History: Time = " +Time.time + " timeDelta = " + History[History.Count - 1].deltaTime + " position = " + History[History.Count - 1].position + " rotation = " + History[History.Count - 1].rotation);
        }
        
    }

    private void SyncPositionWithServer(SC_MovementData message) {
        if (message.TimeStamp > lastReturnedInputTime) {
            latency = Time.time - message.TimeStamp;
        }
        
        lastReturnedInputTime = message.TimeStamp;
        float dt = Mathf.Max(0, historyDuration - latency);
        float ratio = 0f;
        // remove time from History untill it's duration equals to the latency
        while (History.Count > 1 && dt > 0) {
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

        //Debug.Log("Server Message: Time = " + Time.time + " inputTime = " + message.TimeStamp + " delta = " + (Time.time - message.TimeStamp) + " position = " + message.Position + " rotation = " + message.Rotation);
        if (History.Count == 0) {
            return;
        }
        Vector3 historyPosition = History[0].position;
        Quaternion historyRotation = History[0].rotation;
        Vector3 historyVelocity = History[0].velocity;

        if ((Vector3.Distance(message.Position, historyPosition) > positionThreshold) ||
            (Quaternion.Angle(message.Rotation, historyRotation) > rotationThreshold) ) {

            //  Vector3 deltaPosition = transform.position - historyPosition;
          Vector3 deltaPosition = new Vector3();
          foreach (ShipSnapshot ss in History) {
                deltaPosition += ss.deltaPosition;
           }
           Vector3 predictedPosition = message.Position + deltaPosition;
           Vector3 extrapolatedPosition = predictedPosition + physics.Rigidbody.velocity * latency;

           transform.position = Vector3.Lerp(transform.position, extrapolatedPosition, Time.deltaTime);
           Debug.Log("Correcting... lat = " + latency);
        }
    }

    private void CorrectPositionUsingSnapshot(SC_MovementData message) {
        //transform.position = Vector3.Lerp(transform.position, message.Position, Time.deltaTime);
        //transform.rotation = Quaternion.Lerp(transform.rotation, message.Rotation, Time.deltaTime);
       

    }

    private void MoveShadow(SC_MovementData message) {
        Vector3 pos = this.gameObject.GetComponent<Transform>().position;
        Quaternion rot = this.gameObject.GetComponent<Transform>().rotation;

        shadow.GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);

    }

}
