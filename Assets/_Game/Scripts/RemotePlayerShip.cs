using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerShip : PlayerShip {

    private float lastReceivedStateTime;
    private Vector3 lastReceivedVelocity;

    public static float LERP_MUL = 3f;

    // for RemotePlayerShip on Server
    public float LastReceivedStateTime { get { return lastReceivedStateTime; } }

    private void Start() {
        if (isServer) {
            networkController = GameObject.Find("ServerNetworkController");
        }
        else {
            networkController = GameObject.Find("ClientNetworkController");
        }
        if (networkController == null)
            Debug.LogError("ERROR! networkController not found");

        lastReceivedStateTime = -1f;
    }

    private void FixedUpdate() {
        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
       
        if (isServer) {
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

    public override Vector3 GetVelocity() {
        return lastReceivedVelocity;
    }

    private void MoveShipUsingReceivedServerData(SC_MovementData message) {
       transform.position = Vector3.Lerp(transform.position, message.Position, LERP_MUL * Time.deltaTime);
       transform.rotation = Quaternion.Lerp(transform.rotation, message.Rotation, LERP_MUL * Time.deltaTime);
    }

    private void MoveShipUsingReceivedClientData(SC_MovementData message) {
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    private void LagCompShooting(SC_MovementData message) {
        float lagTime = 0f; // assign deleay here
        
        
    }

}

