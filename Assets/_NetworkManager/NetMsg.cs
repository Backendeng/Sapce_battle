using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetMsg {
    protected byte msgType;
    protected float timeStamp;

    public byte Type { get { return msgType; } }
    public float TimeStamp { get { return timeStamp; } }

    public enum MsgType : byte {
        SC_EntityCreated,
        SC_EntityDestroyed,
        SC_MovementData,
        SC_AllocClientID, 
        CS_InputData,
        CS_CreationRequest,
		CS_MissileRequest,
        SC_PlayerData,
    }

    public NetMsg(float timeStamp) {
        this.timeStamp = timeStamp;
    }
}

public class SC_EntityCreated : NetMsg {
    protected int entityID;
    protected Vector3 position;
    protected Quaternion rotation;
    protected int clientID;
    protected byte objectType;

    public int EntityID { get { return entityID; } }
    public Vector3 Position { get { return position; } }
    public Quaternion Rotation { get { return rotation; } }
    public int ClientID { get { return clientID; } }
    public byte ObjectType { get { return objectType; } }

    public SC_EntityCreated(int entityID, float timeStamp, Vector3 position, Quaternion rotation, int clientID, byte objectType) : base(timeStamp) {
        msgType = (byte)MsgType.SC_EntityCreated;
        this.entityID = entityID;
        this.position = position;
        this.rotation = rotation;
        this.clientID = clientID;
        this.objectType = objectType;
    }
}

public class SC_EntityDestroyed : NetMsg {
    protected int entityID;
    public int EntityID { get { return entityID; } }

    public SC_EntityDestroyed(int entityID, float timeStamp) : base(timeStamp) {
        this.entityID = entityID;
        msgType = (byte)MsgType.SC_EntityDestroyed;
    }
}

public class SC_MovementData : NetMsg {
    protected int entityID;
    protected Vector3 position;
    protected Quaternion rotation;
    protected Vector3 velocity;

    public int EntityID { get { return entityID; } }
    public Vector3 Position { get { return position; } }
    public Vector3 Velocity { get { return velocity; } }
    public Quaternion Rotation { get { return rotation; } }

    public SC_MovementData(int entityID, float timeStamp, Vector3 position, Quaternion rotation, Vector3 velocity) : base(timeStamp) {
        msgType = (byte)MsgType.SC_MovementData;
        this.entityID = entityID;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
    }
}

public class CS_InputData : NetMsg {
    protected int entityID;
    protected Vector3 angularInput;
    public float throttle;

    public int EntityID { get { return entityID; } }
    public Vector3 AngularInput { get { return angularInput; } }
    public float Throttle { get { return throttle; } }

    public CS_InputData(int entityID, float timeStamp, Vector3 angularInput, float throttle) : base(timeStamp) {
        msgType = (byte)MsgType.CS_InputData;
        this.entityID = entityID;
        this.angularInput = angularInput;
        this.throttle = throttle;
    }
}

public class SC_AllocClientID : NetMsg {
    protected int entityID;
    protected int clientID;

    public int EntityID { get { return entityID; } }
    public int ClientID { get { return clientID; } }

    public SC_AllocClientID(int entityID, float timeStamp, int clientID) : base(timeStamp) {
        this.entityID = entityID;
        this.clientID = clientID;
        msgType = (byte)MsgType.SC_AllocClientID;
    }
}
//chsnge name to request Shot
public class CS_CreationRequest : NetMsg {
    protected byte objectType;
    protected Vector3 position;
    protected Quaternion rotation;

    public byte ObjectType { get { return objectType; } }
    public Vector3 Position { get { return position; } }
    public Quaternion Rotation { get { return rotation; } }

    public CS_CreationRequest(float timeStamp, Vector3 position, Quaternion rotation, byte objectType) : base(timeStamp) {
        msgType = (byte)MsgType.CS_CreationRequest;
        this.position = position;
        this.rotation = rotation;
        this.objectType = objectType;
    }
}

public class CS_MissileRequest : NetMsg {
	protected int targetId;
	protected Vector3 position;
	protected Quaternion rotation;

	public int TargetId { get { return targetId; } }
	public Vector3 Position { get { return position; } }
	public Quaternion Rotation { get { return rotation; } }

	public CS_MissileRequest(float timeStamp, Vector3 position, Quaternion rotation, int targetId) : base(timeStamp) {
		msgType = (byte)MsgType.CS_MissileRequest;
		this.position = position;
		this.rotation = rotation;
		this.targetId = targetId;
	}
}

public class SC_PlayerData : NetMsg {
    public int ClientID { get; private set; }
    public int Health { get; private set; }
    public int Score { get; private set; }

    public SC_PlayerData(int clientID, float timeStamp, int health, int score) : base(timeStamp) {
        msgType = (byte)MsgType.SC_PlayerData;
        this.ClientID = clientID;
        this.Health = health;
        this.Score = score;
    }
}
