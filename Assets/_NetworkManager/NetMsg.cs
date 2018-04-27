using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetMsg {
    protected int entityID;
    protected byte msgType;
    protected float timeStamp;

    public byte Type { get { return msgType; } }
    public int EntityID { get { return entityID; } }
    public float TimeStamp { get { return timeStamp; } }

    public enum MsgType : byte {
        SC_EntityCreated,
        SC_EntityDestroyed,
        SC_MovementData,
        SC_AllocClientID, 
        CS_InputData,
    }

    public NetMsg(int entityID, float timeStamp) {
        this.entityID = entityID;
        this.timeStamp = timeStamp;
    }
}

public class SC_EntityCreated : NetMsg {
    protected Vector3 position;
    protected Quaternion rotation;
    protected int clientID;
    protected byte objectType;

    public Vector3 Position { get { return position; } }
    public Quaternion Rotation { get { return rotation; } }
    public int ClientID { get { return clientID; } }
    public byte ObjectType { get { return objectType; } }

    public SC_EntityCreated(int entityID, float timeStamp, Vector3 position, Quaternion rotation, int clientID, byte objectType) : base(entityID, timeStamp) {
        msgType = (byte)MsgType.SC_EntityCreated;
        this.position = position;
        this.rotation = rotation;
        this.clientID = clientID;
        this.objectType = objectType;
    }
}

public class SC_EntityDestroyed : NetMsg {

    public SC_EntityDestroyed(int entityID, float timeStamp) : base(entityID, timeStamp) {
        msgType = (byte)MsgType.SC_EntityDestroyed;
    }
}

public class SC_MovementData : NetMsg {
    protected Vector3 position;
    protected Quaternion rotation;
    protected Vector3 velocity;

    public Vector3 Position { get { return position; } }
    public Vector3 Velocity { get { return velocity; } }
    public Quaternion Rotation { get { return rotation; } }

    public SC_MovementData(int entityID, float timeStamp, Vector3 position, Quaternion rotation, Vector3 velocity) : base(entityID, timeStamp) {
        msgType = (byte)MsgType.SC_MovementData;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
    }
}

public class CS_InputData : NetMsg {
    protected Vector3 angularInput;
    public float throttle;

    public Vector3 AngularInput { get { return angularInput; } }
    public float Throttle { get { return throttle; } }

    public CS_InputData(int entityID, float timeStamp, Vector3 angularInput, float throttle) : base(entityID, timeStamp) {
        msgType = (byte)MsgType.CS_InputData;
        this.angularInput = angularInput;
        this.throttle = throttle;
    }
}

public class SC_AllocClientID : NetMsg {
    protected int clientID;
    public int ClientID { get { return clientID; } }

    public SC_AllocClientID(int entityID, float timeStamp, int clientID) : base(entityID, timeStamp) {
        this.clientID = clientID;
        msgType = (byte)MsgType.SC_AllocClientID;
    }
}

