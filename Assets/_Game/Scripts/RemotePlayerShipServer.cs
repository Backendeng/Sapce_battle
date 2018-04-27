using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingServer))]

public class RemotePlayerShipServer : PlayerShip {
    private ShipShootingServer shooting;

    public static float LERP_MUL = 1f;

    public new void Start() {
        base.Start();
    }

    private void Update() {
        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
    
        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.CS_InputData:
                // Handle Shooting
                break;
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
    }


}

