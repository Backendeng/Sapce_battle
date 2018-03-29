using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetMsg {
    public int entityID;
    protected byte msgType;
    public float timeStamp;

    public byte Type { get { return msgType; } }

    public enum MsgType : byte {
        SC_EntityCreated,
        SC_EntityDestroyed,
        SC_MovementData,
        CS_InputData,
    }

    public NetMsg(int entityID, float timeStamp) {
        this.entityID = entityID;
        this.timeStamp = timeStamp;
    }
}

public abstract class MovementData : NetMsg {
    public Vector3 position;
    public Quaternion rotation;

    public MovementData(int entityID, float timeStamp, Vector3 position, Quaternion rotation) :
        base (entityID, timeStamp) {
        this.position = position;
        this.rotation = rotation;
    }
}

public class SC_EntityCreated : MovementData {
    public int connectionID;
    public SC_EntityCreated(int entityID, float timeStamp, Vector3 position, Quaternion rotation, int connId) : 
        base(entityID, timeStamp, position, rotation) {
        msgType = (byte)MsgType.SC_EntityCreated;
        connectionID = connId;
    }
}

public class SC_EntityDestroyed : NetMsg {

    public SC_EntityDestroyed(int entityID, float timeStamp) : base(entityID, timeStamp) {
        msgType = (byte)MsgType.SC_EntityDestroyed;
    }
}

public class SC_MovementData : MovementData {

    public SC_MovementData(int entityID, float timeStamp, Vector3 position, Quaternion rotation) : 
        base(entityID, timeStamp, position, rotation) {
        msgType = (byte)MsgType.SC_EntityCreated;
    }
}

public class CS_InputData : NetMsg {
    protected Vector3 angularInput;
    public float throttle;

    public CS_InputData(int entityID, float timeStamp, Vector3 angularInput, float throttle) : base(entityID, timeStamp) {
        this.angularInput = angularInput;
        this.throttle = throttle;
    }
}


