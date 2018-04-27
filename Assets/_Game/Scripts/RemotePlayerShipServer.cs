using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingServer))]
[RequireComponent(typeof(Target))]


public class RemotePlayerShipServer : PlayerShip {
    
    private Target target;
    private ShipShootingServer shooting;
    private Vector3 lastReceivedVelocity;

    public static float LERP_MUL = 1f;

    public new void Start() {
        base.Start();

        // target init
        target = GetComponent<Target>();
        target.Init(serverController, clientID); // clientID is assigned in Server script @ ProccessAllocClientID
    }

    private void FixedUpdate() {
        HandleIncomingMessages();

        UpdateGameData();
    }

    public override Vector3 GetVelocity() {
        return lastReceivedVelocity;
    }

    private void MoveShipUsingReceivedClientData(SC_MovementData message) {
        lastReceivedStateTime = message.TimeStamp;
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
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

    private void UpdateGameData() {
        

    }

}

