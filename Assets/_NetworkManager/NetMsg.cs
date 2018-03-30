﻿using System.Collections;
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

public abstract class MovementData : NetMsg {
    protected Vector3 position;
    protected Quaternion rotation;

    public Vector3 Position { get { return position; } }
    public Quaternion Rotation {
        get {
            return rotation;
        }
    }

    public MovementData(int entityID, float timeStamp, Vector3 position, Quaternion rotation) :
        base(entityID, timeStamp) {
        this.position = position;
        this.rotation = rotation;
    }
}

public class SC_EntityCreated : MovementData {
    protected int clientID;
    public int ClientID { get { return clientID; } }

    public SC_EntityCreated(int entityID, float timeStamp, Vector3 position, Quaternion rotation, int clientID) : 
        base(entityID, timeStamp, position, rotation) {
        msgType = (byte)MsgType.SC_EntityCreated;
        this.clientID = clientID;
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
        msgType = (byte)MsgType.SC_MovementData;
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

public class SC_AllocClientID : MovementData {
    protected int clientID;
    public int ClientID { get { return clientID; } }

    public SC_AllocClientID(int entityID, float timeStamp, Vector3 position, Quaternion rotation, int clientID) :
        base(entityID, timeStamp, position, rotation) {
        this.clientID = clientID;
        msgType = (byte)MsgType.SC_AllocClientID;
    }
}

