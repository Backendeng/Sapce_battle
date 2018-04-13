using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerShip : PlayerShip {

   
    private Vector3 lastReceivedVelocity;

    public static float LERP_MUL = 1f;

    private float lerpingStartTime;
    private float lerpDeltaTime;
    private Vector3 lerpStartPos;
    private Vector3 lerpEndPos;
    private Quaternion lerpStartRot;
    private Quaternion lerpEndRot;

    public new void Start() {
        base.Start();
        lerpingStartTime = -1f;
    }

    private void Update() {

        if (!isServer) {
            MoveShipUsingReceivedServerData();
        }

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
                    AddSnapshotToHistoryOnClient((SC_MovementData)netMessage); // for interpolation and extrapolation
                    ReceiveServerStateUpdate();// uses snapshotHistory
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

    private void ReceiveServerStateUpdate() {
        int lastIdx = GetHistoryLastIdx();

        if (lastIdx == 0)
            return;

        int prevIdx = lastIdx - 1;
        StateSnapshot lastSnap = GetLastSnapshotAt(lastIdx);
        StateSnapshot prevSnap = GetLastSnapshotAt(prevIdx);

        //lerp state update
        lerpingStartTime = Time.time;

        lerpDeltaTime = lastSnap.time - prevSnap.time;
        lerpStartPos = prevSnap.position;
        lerpEndPos = lastSnap.position;
        lerpStartRot = prevSnap.rotation;
        lerpEndRot = lastSnap.rotation;
    }

    // transform.position = Vector3.Lerp(transform.position, message.Position, LERP_MUL * Time.deltaTime);
    //transform.rotation = Quaternion.Slerp(transform.rotation, message.Rotation, LERP_MUL * Time.deltaTime);
    // GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);

    //use lerp state to lerp 
    private void MoveShipUsingReceivedServerData() {
        if (lerpingStartTime == -1f)
            return;
        float lerpPercentage = (Time.time - lerpingStartTime) / lerpDeltaTime;
        transform.position = Vector3.Lerp(lerpStartPos, lerpEndPos, lerpPercentage);
        transform.rotation = Quaternion.Slerp(lerpStartRot, lerpEndRot, lerpPercentage);
    }

    private void MoveShipUsingReceivedClientData(SC_MovementData message) {
        lastReceivedStateTime = message.TimeStamp;
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    private void LagCompShooting(SC_MovementData message) {
        float lagTime = 0f; // assign deleay here
        
        
    }

}

