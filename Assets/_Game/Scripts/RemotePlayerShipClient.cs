using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingClient))]

public class RemotePlayerShipClient : PlayerShip {
    private ShipShootingClient shooting;
    private Vector3 lastReceivedVelocity;

    // Lerping State
    public bool doLerp = true;
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

        MoveShipUsingReceivedServerData();

        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();

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


    //use lerp state to lerp 
    private void MoveShipUsingReceivedServerData() {
        if (!doLerp) {
            GetComponent<Transform>().SetPositionAndRotation(lerpEndPos, lerpEndRot);
            return;
        }
	// GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
        if (lerpingStartTime == -1f)
            return;
        float lerpPercentage = (Time.time - lerpingStartTime) / lerpDeltaTime;
        transform.position = Vector3.Lerp(lerpStartPos, lerpEndPos, lerpPercentage);
        transform.rotation = Quaternion.Slerp(lerpStartRot, lerpEndRot, lerpPercentage);
    }

}

