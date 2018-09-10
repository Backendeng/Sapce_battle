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
        CS_ProjectileRequest,
		CS_MissileRequest,
        SC_PlayerData,
        MSG_ShipCreated,
        MSG_NewPlayerRequest,
        MSG_PlayerKilled,
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

public class CS_ProjectileRequest : NetMsg {
    protected byte objectType;
    protected Vector3 position;
    protected Quaternion rotation;

    public byte ObjectType { get { return objectType; } }
    public Vector3 Position { get { return position; } }
    public Quaternion Rotation { get { return rotation; } }

    public CS_ProjectileRequest(float timeStamp, Vector3 position, Quaternion rotation, byte objectType) : base(timeStamp) {
        msgType = (byte)MsgType.CS_ProjectileRequest;
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
    public int PlayerID { get; private set; }
    public int Health { get; private set; }
    public int Score { get; private set; }
    public int Deaths { get; private set; }

    public SC_PlayerData(int playerID, float timeStamp, int health, int score, int deaths) : base(timeStamp) {
        msgType = (byte)MsgType.SC_PlayerData;
        PlayerID = playerID;
        Health = health;
        Score = score;
        Deaths = deaths;
    }
}


public class MSG_ShipCreated : NetMsg {
    protected int entityID;
    protected Vector3 position;
    protected Quaternion rotation;
    protected int clientID;
    protected byte shipType;
    protected string playerName;

    public int EntityID { get { return entityID; } } // PlayerID
    public Vector3 Position { get { return position; } }
    public Quaternion Rotation { get { return rotation; } }
    public int ClientID { get { return clientID; } }
    public byte ShipType { get { return shipType; } }
    public string PlayerName { get { return playerName; } }


    public MSG_ShipCreated(int entityID, float timeStamp, Vector3 position, Quaternion rotation, int clientID, byte shipType, string playerName) : base(timeStamp) {
        msgType = (byte)MsgType.MSG_ShipCreated;
        this.entityID = entityID;
        this.position = position;
        this.rotation = rotation;
        this.clientID = clientID;
        this.shipType = shipType;
        this.playerName = playerName;
    }
}

public class MSG_NewPlayerRequest : NetMsg {
    protected int entityID;
    protected int clientID;
    protected string playerName;
    protected byte shipType;

    public int EntityID { get { return entityID; } }
    public int ClientID { get { return clientID; } }
    public byte ShipType { get { return shipType; } } // TODO: implement usage of this value
    public string PlayerName { get { return playerName; } }


    public MSG_NewPlayerRequest(int entityID, float timeStamp, int clientID, byte shipType, string playerName) : base(timeStamp) {
        msgType = (byte)MsgType.MSG_NewPlayerRequest;
        this.entityID = entityID;
        this.clientID = clientID;
        this.playerName = playerName;
        this.shipType = shipType;
    }
}

public class MSG_PlayerKilled : NetMsg {
    protected int killerID;
    protected int victimID;
    protected byte weapon; // 1 = projectile, 2 = missile

    public int KillerID { get { return killerID; } }
    public int VictimID { get { return victimID; } }
    public byte Weapon { get { return weapon; } }

    public MSG_PlayerKilled(int killerID, int victimID, byte weapon, float timeStamp) : base(timeStamp) {
        msgType = (byte)MsgType.MSG_PlayerKilled;
        this.killerID = killerID;
        this.victimID = victimID;
        this.weapon = weapon;
    }
}


