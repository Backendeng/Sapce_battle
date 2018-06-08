using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementInterpolator {

    protected class StateSnapshot {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;

        public StateSnapshot(float time, Vector3 position, Quaternion rotation, Vector3 velocity) {
            this.time = time;
            this.position = position;
            this.rotation = rotation;
            this.velocity = velocity;
        }
    }
   
    private float interpolationDelay = 0.12f; 
    private float recRate = 0.06f; // this is interpolation slot size
    private const int buffSize = 10;
    private StateSnapshot[] serverUpdates = new StateSnapshot[buffSize];
    private int currPtr = -1;
    private int lastPtr = -1;
    private float lastUpdateTimestamp;
    private int firstRecCount = 6;

    // Lerping State
    public bool doLerp = true;
    private float lerpPercentage = 1;
    private float lerpDeltaTime = 1;
    private Vector3 lerpStartPos;
    private Vector3 lerpEndPos;
    private Quaternion lerpStartRot;
    private Quaternion lerpEndRot;

    private float lerpDeltaTimeRot;

    private Transform transform;
    private int entityId = -1;

    public MovementInterpolator(Transform t, int entityId) {
        transform = t;
        this.entityId = entityId;
    }

    public void RecUpdate(SC_MovementData msg) {
        if (msg.TimeStamp < 0)
            return;
        if (msg.TimeStamp < lastUpdateTimestamp + Time.fixedDeltaTime / 2)
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


        ///IMPORTENT: localy when not extrapolating we get good sync, catching up may skew it but the stall seems to resync it.(sometimes)
        ///// also we stall mainly when we wait a long time to get recs, which is probably normal. + after catching up
        //// strangly enough we get wait more for new updates when shooting etc...
        /// maybe we need a mechanisem that takes into consideration the avg wait time or new updates?
        if (updateB != null && updateB.time < updateA.time) {/// NULL REF EXP
        //    Debug.LogWarning("stallTime: "+Time.time+" No new lerp position, last valid pos: " + updateA.time + " time since last rec: "+ (Time.time - lastRecTime));
            Logger.Log(Time.time, Time.realtimeSinceStartup, entityId, "InterpolationSTALL", updateA.time.ToString());
            /* lerpDeltaTime = recRate;
            lerpStartPos = updateA.position;
            lerpEndPos = lerpStartPos + lerpDeltaTime*updateA.velocity;
            Debug.LogWarning(" extrapolating till " + (updateA.time + lerpDeltaTime)); //TODO extrapolate rotation
           
            lastPtr = (lastPtr + 1) % buffSize;
            serverUpdates[nextPtr] = new StateSnapshot(updateA.time + lerpDeltaTime, updateA.position, updateA.rotation, updateA.velocity); // next snapshot is bogus , maybe this is wrong. computed interpolation delay is wrong after this
            currPtr = (currPtr + 1) % buffSize;
            lastUpdateTimestamp = updateA.time + lerpDeltaTime;
           */
            return;///with extrapolation we decrease interpolationDelay and then start stalling more often, implement wait mecahisem?
        }
        currPtr = (currPtr + 1) % buffSize;
        ///NULL REF EXP
        if ((lastUpdateTimestamp - updateA.time) > (interpolationDelay + Time.fixedDeltaTime / 2)) { //make sure were not behind interpolationDelay
            if (updateB.time - updateA.time < recRate + Time.fixedDeltaTime / 2) {//slot size + fixedDelta / 2
                                                                                  // Debug.Log("Catching up to interp delay " + (lastUpdateTimestamp - updateA.time)+" from "+ updateA.time + " debug " + interpolationDelay + " " + (Time.fixedDeltaTime / 2));
                GetNextInterpolationParameters();
                return;/// posibly we need to allow for a small gap before catching up. sometimes 1 pack arrives early and there is no need to catch up
            }//==> we need to keep some sort of stats that help us decide when to do this,(and extrapolate sometimes?) and not like this
        }

        lerpDeltaTime = updateB.time - updateA.time;/// NUL REF EXP

        float lastTs = serverUpdates[lastPtr].time;///////////
                                                   // Debug.Log("lerpSetTime: " + Time.time + " state: " +" A: "+ updateA.time + " B: "+ updateB.time + " last: "+ lastTs +" dt " + lerpDeltaTime + " intDelay " + (lastUpdateTimestamp - updateA.time));
        Logger.Log(Time.time, Time.realtimeSinceStartup, entityId, "Interpolation", updateA.time.ToString());
        lerpStartPos = updateA.position;
        lerpEndPos = updateB.position;

        lerpStartRot = updateA.rotation;
        lerpEndRot = updateB.rotation;

    }

    public void InterpolateMovement() {
        if (!doLerp) {
            return;
        }
        if (firstRecCount > 0)
            return;

        lerpPercentage += (Time.fixedDeltaTime / lerpDeltaTime);//// 0.02 / 0.06 

        if (lerpPercentage >= 0.95f) {
            //Debug.Log("updating lerp params " + "dt since last lerpStart: " + (Time.time - lastLerp));

            GetNextInterpolationParameters();
            lerpPercentage = 0;

        }

        transform.position = Vector3.Lerp(lerpStartPos, lerpEndPos, lerpPercentage);
        transform.rotation = Quaternion.Slerp(lerpStartRot, lerpEndRot, lerpPercentage);

        // Debug.Log("Interpolating pos " + lerpPercentage);

    }

}
