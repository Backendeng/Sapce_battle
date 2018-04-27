using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingClient))]

public class RemotePlayerShipClient : PlayerShip {
    private ShipShootingClient shooting;

    //note: if interpolationDelay does not match update rate we get jitter (we constantly try to catch up)
    private float interpolationDelay = 0.12f; //there is a bug here if we make this public, keeps old value when changing here
    private const int buffSize = 5;
    private StateSnapshot[] serverUpdates = new StateSnapshot[buffSize];
    private int currPtr = -1;
    private int lastPtr = -1;
    private float lastUpdateTimestamp;
    private int firstRecCount = buffSize;

    // Lerping State
    public bool doLerp = true;
    private float lerpPercentage = 1;
    private float lerpingStartTime;
    private float lerpDeltaTime = 1;
    private Vector3 lerpStartPos;
    private Vector3 lerpEndPos;
    private Quaternion lerpStartRot;
    private Quaternion lerpEndRot;

    private float lerpDeltaTimeRot;

    public new void Start() {
        base.Start();
        lerpingStartTime = -1f;
    }

    Vector3 lastPos;


    float lastRecTime = -1;
    private void FixedUpdate() {

       // Debug.Log("Dpos: " + Vector3.Magnitude(transform.position - lastPos));
        lastPos = transform.position;

        MoveShipUsingReceivedServerData();//lerp
        
        if (incomingQueue.Count == 0)
            return;

        while (incomingQueue.Count > 0) { 
            NetMsg netMessage = incomingQueue.Dequeue();

            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    float ts = ((SC_MovementData)netMessage).TimeStamp;
                   // Debug.Log("recDelta: "+ (Time.time - lastRecTime)+" ts "+ ts);
                    lastRecTime = Time.time;

                    RecUpdate((SC_MovementData)netMessage);

                    SetShipState((SC_MovementData)netMessage);

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

    private void SetShipState(SC_MovementData msg) {
        if (doLerp) {
            return;
        }

        GetComponent<Transform>().SetPositionAndRotation(msg.Position, msg.Rotation);
    }

    private void RecUpdate(SC_MovementData msg) {
        if (msg.TimeStamp < 0)
            return;
        if (msg.TimeStamp < lastUpdateTimestamp + Time.fixedDeltaTime/2)
            return;

        if (firstRecCount > 0) {
            firstRecCount--;
        }

        lastPtr = (lastPtr + 1) % buffSize;
        serverUpdates[lastPtr] = new StateSnapshot(msg.TimeStamp, msg.Position, msg.Rotation, msg.Velocity);
        lastUpdateTimestamp = msg.TimeStamp;

    }

    private void GetNextInterpolationParameters() {
       
        StateSnapshot updateA = serverUpdates[(currPtr + 1) % buffSize];
        int nextPtr = (currPtr + 2) % buffSize;
        StateSnapshot updateB = serverUpdates[nextPtr];

        if (updateB.time < updateA.time) {
            Debug.Log("No new lerp position, last valid pos: " + updateA.time);
            return;
        }
        currPtr = (currPtr + 1) % buffSize;
        
        if((lastUpdateTimestamp - updateA.time) > (interpolationDelay + Time.fixedDeltaTime/2)) { //make sure were not behind interpolationDelay
            Debug.Log("Catching up to interp delay " + (lastUpdateTimestamp - updateA.time) + " debug " + interpolationDelay +" "+ (Time.fixedDeltaTime / 2));
            GetNextInterpolationParameters();
            return;
        }

        //lerp state update
        lerpingStartTime = Time.time;

        lerpDeltaTime = updateB.time - updateA.time;
        Debug.Log("dt " + lerpDeltaTime + " update stamp: B: idx" + nextPtr + " " + updateB.time + " A: idx" + currPtr + " " + updateA.time + " intDelay " + (lastUpdateTimestamp - updateA.time));//////////////////////////////////////
       
        lerpStartPos = updateA.position;
        lerpEndPos = updateB.position;

        lerpStartRot = updateA.rotation;
        lerpEndRot = updateB.rotation;

    }

    float lastLerp = -1;
    
    private void MoveShipUsingReceivedServerData() {
        if (!doLerp) {
            return;
        }
        if (firstRecCount > 0) 
            return;

        lerpPercentage += (Time.fixedDeltaTime / lerpDeltaTime);//// 0.02 / 0.06 

        if (lerpPercentage >= 0.95f) {
            Debug.Log("updating lerp params " + "dt since last lerpStart: " + (Time.time - lastLerp));
            lastLerp = Time.time;
     
            GetNextInterpolationParameters();
            lerpPercentage = 0;

        }

        transform.position = Vector3.Lerp(lerpStartPos, lerpEndPos, lerpPercentage);
        transform.rotation = Quaternion.Slerp(lerpStartRot, lerpEndRot, lerpPercentage);

        Debug.Log("Interpolating pos " + lerpPercentage);

    }


}

