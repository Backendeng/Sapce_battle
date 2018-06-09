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
   
    private float targetInterpolationDelay = 0.12f; 
    private float recRate = 0.06f; // this is interpolation slot size
    private const int buffSize = 10;
    private StateSnapshot[] serverUpdates = new StateSnapshot[buffSize];
    private int currPtr = -1;
    private int lastPtr = -1;
    private float lastUpdateTimestamp;
    private int firstRecCount = 6;

    //stats
    private bool useAvgDelayToCatchup = true;
    private float[] interpolationDelays = new float[buffSize];
    private int[] stalled = new int[buffSize];

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

        if (updateB == null)
            return;
        //save stats
        interpolationDelays[(currPtr + 1) % buffSize] = (lastUpdateTimestamp - updateA.time);
       // Debug.Log("delay idx" + ((currPtr + 1) % buffSize) + " delay "+ interpolationDelays[(currPtr + 1) % buffSize]);
       // Debug.Log("avg delay " + avgDelay() );
        
       // Debug.Log("stalles idx" + ((currPtr + 1) % buffSize) + " idstall " + stalled[(currPtr + 1) % buffSize]);
     
        if (updateB != null && updateB.time < updateA.time) {/// NULL REF EXP
        //    Debug.LogWarning("stallTime: "+Time.time+" No new lerp position, last valid pos: " + updateA.time + " time since last rec: "+ (Time.time - lastRecTime));
            Logger.Log(Time.time, Time.realtimeSinceStartup, entityId, "InterpolationSTALL", updateA.time.ToString());
            stalled[(currPtr + 1) % buffSize] = 1;
            Debug.Log("stalled");

            tryToExtrapolateMovement(updateA);
            return;
        }
        currPtr = (currPtr + 1) % buffSize;

        if (tryToCatchUp(updateA.time, updateB.time))///NULL REF EXP
            return;

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
    private int warmup = 10;
    private bool tryToCatchUp(float timeA, float timeB) {
        float delay = 0;
        if (warmup > 0 || !useAvgDelayToCatchup) {
            delay = (lastUpdateTimestamp - timeA) - Time.fixedDeltaTime / 2;
            warmup--;
        }
        else {
            delay = avgDelay() - recRate / 2;
        }

      
        if (delay > targetInterpolationDelay) { //make sure were not behind interpolationDelay
            if (timeB - timeA < recRate + Time.fixedDeltaTime / 2) {//slot size + fixedDelta / 2
                                                                                  // Debug.Log("Catching up to interp delay " + (lastUpdateTimestamp - updateA.time)+" from "+ updateA.time + " debug " + interpolationDelay + " " + (Time.fixedDeltaTime / 2));
                GetNextInterpolationParameters();
                Debug.Log("CatchingUp");
                return true;/// posibly we need to allow for a small gap before catching up. sometimes 1 pack arrives early and there is no need to catch up
            }//==> we need to keep some sort of stats that help us decide when to do this,(and extrapolate sometimes?) and not like this
        }
        return false;
    }
    ///IMPORTENT: localy when not extrapolating we get good sync, catching up may skew it but the stall seems to resync it.(sometimes)
    ///// also we stall mainly when we wait a long time to get recs, which is probably normal. + after catching up
    //// strangly enough we get wait more for new updates when shooting etc...
    /// maybe we need a mechanisem that takes into consideration the avg wait time or new updates?
    private bool tryToExtrapolateMovement(StateSnapshot updateA) {
        if (avgDelay() < targetInterpolationDelay - recRate / 2)
            return false;
        lerpDeltaTime = recRate;
        lerpStartPos = updateA.position;
        lerpEndPos = lerpStartPos + lerpDeltaTime * updateA.velocity;
        Debug.LogWarning(" extrapolating till " + (updateA.time + lerpDeltaTime)); //TODO extrapolate rotation
        Logger.Log(Time.time, Time.realtimeSinceStartup, entityId, "Extrapolation", updateA.time.ToString());
        lastPtr = (lastPtr + 1) % buffSize;
        serverUpdates[(currPtr + 2) % buffSize] = new StateSnapshot(updateA.time + lerpDeltaTime, updateA.position, updateA.rotation, updateA.velocity); // next snapshot is bogus , maybe this is wrong. computed interpolation delay is wrong after this
        currPtr = (currPtr + 1) % buffSize;
        //lastUpdateTimestamp = updateA.time + lerpDeltaTime;
        return true;
    }

    private float avgDelay() {
        float sum = 0f;
        for(int i = 0; i < interpolationDelays.Length; i++) {
            sum += interpolationDelays[i];
        }
        return sum / interpolationDelays.Length;
    }

}
