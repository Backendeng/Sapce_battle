using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerShip : PlayerShip {

    private float lastRecievedInputTime;

    public float LastRecievedInputTime { get { return lastRecievedInputTime; } }

    private void Start() {
        if (isServer) {
            networkController = GameObject.Find("ServerNetworkController");
        }
        else {
            networkController = GameObject.Find("ClientNetworkController");
        }
        if (networkController == null)
            Debug.LogError("ERROR! networkController not found");

        lastRecievedInputTime = Time.time;
    }

    private void FixedUpdate() {
        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
       
        if (isServer) {
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.CS_InputData:
                    MoveShipUsingClientInput((CS_InputData)netMessage);
                    break;
                case (byte)NetMsg.MsgType.SC_MovementData:
                    MoveShipUsingReceivedServerData((SC_MovementData)netMessage);//this is recieved client data not server
                    break;
                case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                    Destroy(gameObject);
                    break;
                default:
                    Debug.Log("ERROR! RemotePlayerShip on Server reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                    break;
            }
        } else {
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    MoveShipUsingReceivedServerData((SC_MovementData)netMessage);
                    break;
                case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                    Destroy(gameObject);
                    break;
                default:
                    Debug.Log("ERROR! RemotePlayerShip on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                    break;
            }
        }
    }

    private void MoveShipUsingClientInput(CS_InputData message) {
        physics.SetPhysicsInput( new Vector3(0f, 0f, message.Throttle), message.AngularInput);
        lastRecievedInputTime = message.TimeStamp;
    }

    private void MoveShipUsingReceivedServerData(SC_MovementData message) {
       GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }
}
