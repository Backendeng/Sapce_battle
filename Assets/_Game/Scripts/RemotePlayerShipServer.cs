using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingServer))]
[RequireComponent(typeof(Target))]


public class RemotePlayerShipServer : PlayerShip {
    
    private Target target;
    private bool dead = false;
    private ShipShootingServer shooting;

    public static float LERP_MUL = 1f;

    public new void Start() {
        base.Start();

        // target init
        target = GetComponent<Target>();
        target.Init(serverController, entityID, clientID); // clientID is assigned in Server script @ ProccessAllocClientID
    }

    private void FixedUpdate() {
        HandleIncomingMessages();

        UpdateGameData();
    }

    public override Vector3 GetVelocity() {
        return lastReceivedVelocity;
    }

    private void HandleIncomingMessages() {
        if (incomingQueue.Count == 0) {
            return;
        }
 
        NetMsg netMessage = incomingQueue.Dequeue();

        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.SC_MovementData:
                MoveShipUsingReceivedClientData((SC_MovementData)netMessage);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                Destroy(gameObject);
                break;
            default:
                Debug.Log("ERROR! RemotePlayerShip on Server reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                break;
        }
    }


    private void MoveShipUsingReceivedClientData(SC_MovementData message) {

        lastReceivedStateTime = message.TimeStamp;
        lastReceivedVelocity = message.Velocity;
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);

        AddSnapshotToQueue(message.TimeStamp, message.Position, message.Rotation, message.Velocity);
        Debug.Log("registered ts "+ lastReceivedStateTime);////////////////////////////////////////////
    }
    private void UpdateGameData() {
        if (target.Health == 0 && dead == false) {
            Destroy(Instantiate(ShipExplosion, transform.position, Quaternion.identity), 1);
            GetComponentInChildren<MeshRenderer>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<TrailRenderer>().enabled = false;
            dead = true;
        }


    }
    // we run this till we get null
    public override SC_MovementData GetNextSnapshot(int entityId) {
        StateSnapshot ss = GetNextSnapshotFromQueue();
        if (ss == null)
            return null;

        SC_MovementData msg = new SC_MovementData(entityId, ss.time, ss.position, ss.rotation, ss.velocity);
        return msg;
    }

}

