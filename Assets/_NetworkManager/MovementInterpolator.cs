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
    private float lastExtrapolationTimestamp;
    private int firstRecCount = 6;
    private bool doExtraploation = true;

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
        if (msg.TimeStamp < lastExtrapolationTimestamp + Time.fixedDeltaTime / 2)//throw away extrapolated slots
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
     
        if (updateB != null && updateB.time < updateA.time) {
        //    Debug.LogWarning("stallTime: "+Time.time+" No new lerp position, last valid pos: " + updateA.time + " time since last rec: "+ (Time.time - lastRecTime));
            
            stalled[(currPtr + 1) % buffSize] = 1;
            Debug.Log("stalled");

            bool didExtrapolation = tryToExtrapolateMovement(updateA);
            if (!didExtrapolation)
                Logger.Log(Time.time, Time.realtimeSinceStartup, entityId, "InterpolationSTALL", updateA.time.ToString());

            return;
        }
        currPtr = (currPtr + 1) % buffSize;

        if (tryToCatchUp(updateA.time, updateB.time))///NULL REF EXP // consequtive catch ups (eliminated with edge case checking?)
            return;

        lerpDeltaTime = updateB.time - updateA.time;

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

            GetNextInterpolationParameters();// if stalled and not extrapolated we should return here?(set percentage 1?)
            lerpPercentage = 0;

        }

        transform.position = Vector3.Lerp(lerpStartPos, lerpEndPos, lerpPercentage);
        transform.rotation = Quaternion.Slerp(lerpStartRot, lerpEndRot, lerpPercentage);
        // Debug.Log("Interpolating pos " + lerpPercentage);

    }
    private int warmup = 10;
    private bool tryToCatchUp(float timeA, float timeB) {
        float delay = 0;
        float immediateDelay = (lastUpdateTimestamp - timeA) - Time.fixedDeltaTime / 2;
        if (warmup > 0 || !useAvgDelayToCatchup) {
            delay = immediateDelay;
            warmup--;
        }
        else {
            delay = avgDelay() - recRate / 2;
        }

        if (immediateDelay < recRate) // edge case where looking at the avg brings us too close
            return false;
      
        if (delay > targetInterpolationDelay) { //make sure were not behind interpolationDelay
            if (timeB - timeA < recRate + Time.fixedDeltaTime / 2) {//slot size + fixedDelta / 2
             // Debug.Log("Catching up to interp delay " + (lastUpdateTimestamp - updateA.time)+" from "+ updateA.time + " debug " + interpolationDelay + " " + (Time.fixedDeltaTime / 2));
                GetNextInterpolationParameters();
                Debug.Log("CatchingUp");
                return true;
            }
        }
        return false;
    }
  
    //// strangly enough we get wait more for new updates when shooting etc...
    private bool tryToExtrapolateMovement(StateSnapshot updateA) {
        if (!doExtraploation)
            return false;
        if (avgDelay() < targetInterpolationDelay - recRate / 2)
            return false;
        lerpDeltaTime = recRate;
        lerpStartPos = updateA.position;
        lerpEndPos = lerpStartPos + lerpDeltaTime * updateA.velocity;
        Debug.LogWarning(" extrapolating till " + (updateA.time + lerpDeltaTime)); //TODO extrapolate rotation
        Logger.Log(Time.time, Time.realtimeSinceStartup, entityId, "Extrapolation", updateA.time.ToString());
        lastPtr = (lastPtr + 1) % buffSize;
        serverUpdates[(currPtr + 2) % buffSize] = new StateSnapshot(updateA.time + lerpDeltaTime, updateA.position, updateA.rotation, updateA.velocity); // next snapshot is bogus , maybe this is wrong. computed interpolation delay is wrong after this (is it??)
        currPtr = (currPtr + 1) % buffSize;
        lastExtrapolationTimestamp = updateA.time + lerpDeltaTime;
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
