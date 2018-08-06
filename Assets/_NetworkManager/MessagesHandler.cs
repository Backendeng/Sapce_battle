using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessagesHandler {
    public static int headerSize = 1;
    private static int name_size = 20;


    public static byte[] NetMsgPack(NetMsg message) {
        byte[] bytesMessage;
        string jsonMsg;

        switch (message.Type) {
            case (byte)NetMsg.MsgType.SC_EntityCreated:
                bytesMessage = PackEntityCreatedMsg((SC_EntityCreated)message);
                return bytesMessage;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                bytesMessage = PackEntityDestroyedMsg((SC_EntityDestroyed)message);
                return bytesMessage;
            case (byte)NetMsg.MsgType.SC_MovementData:
                bytesMessage = PackMovementMsg((SC_MovementData)message);
                return bytesMessage;
            case (byte)NetMsg.MsgType.CS_ProjectileRequest:
                bytesMessage = PackCreationRequestMsg((CS_ProjectileRequest)message);
                return bytesMessage;
			case (byte)NetMsg.MsgType.CS_MissileRequest:
				bytesMessage = PackMissileRequestMsg((CS_MissileRequest)message);
				return bytesMessage;
            case (byte)NetMsg.MsgType.SC_PlayerData:
                bytesMessage = PackPlayerDataMsg((SC_PlayerData)message);
                return bytesMessage;
            case (byte)NetMsg.MsgType.MSG_ShipCreated:
                bytesMessage = PackShipCreatedMsg((MSG_ShipCreated)message);
                return bytesMessage;
            case (byte)NetMsg.MsgType.SC_AllocClientID:
                jsonMsg = JsonConvert.SerializeObject((SC_AllocClientID)message);
                break;
            case (byte)NetMsg.MsgType.MSG_NewPlayerRequest:
                bytesMessage = PackNewPlayerRequestMsg((MSG_NewPlayerRequest)message);
                return bytesMessage;
            default:
                jsonMsg = "";
                break;
        }
        bytesMessage = System.Text.Encoding.ASCII.GetBytes(jsonMsg);
        byte[] packedMessage = new byte[headerSize + bytesMessage.Length];
        packedMessage[0] = message.Type;
        // copy bytesMessage to packedMessage from index = headerSize
        bytesMessage.CopyTo(packedMessage, headerSize);

        return packedMessage;
    }

    public static NetMsg NetMsgUnpack(byte[] packedMessage) {
        NetMsg unpackedMessage = null;
        byte[] bytesMessage = packedMessage.Skip(headerSize).ToArray();

        string jsonMsg = System.Text.ASCIIEncoding.ASCII.GetString(bytesMessage);

        byte msgType = packedMessage[0];
        switch (msgType) {
            case (byte)NetMsg.MsgType.SC_EntityCreated:
                unpackedMessage = UnpackEntityCreatedMsg(packedMessage);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                unpackedMessage = UnpackEntityDestroyedMsg(packedMessage);
                break;
            case (byte)NetMsg.MsgType.SC_MovementData:
                unpackedMessage = UnpackMovementMsg(packedMessage);
                break;
            case (byte)NetMsg.MsgType.CS_ProjectileRequest:
                unpackedMessage = UnpackCreationRequestMsg(packedMessage);
                break;
			case (byte)NetMsg.MsgType.CS_MissileRequest:
				unpackedMessage = UnpackMissileRequestMsg(packedMessage);
				break;
            case (byte)NetMsg.MsgType.SC_PlayerData:
                unpackedMessage = UnpackPlayerDataMsg(packedMessage);
                break;
            case (byte)NetMsg.MsgType.MSG_ShipCreated:
                unpackedMessage = UnpackShipCreatedMsg(packedMessage);
                break;
            case (byte)NetMsg.MsgType.SC_AllocClientID:
                unpackedMessage = JsonConvert.DeserializeObject<SC_AllocClientID>(jsonMsg);
                break;
            case (byte)NetMsg.MsgType.MSG_NewPlayerRequest:
                unpackedMessage = UnpackNewPlayerRequestMsg(packedMessage);
                break;
            default:
                unpackedMessage = null;
                break;
        }

        return unpackedMessage;
    }

    private static byte[] PackMovementMsg(SC_MovementData message) {
        byte[] packedMessage = new byte[48 + headerSize];
        packedMessage[0] = message.Type;

        byte[] entityID = System.BitConverter.GetBytes(message.EntityID); // 1
        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp); // 5
        byte[] position_x = System.BitConverter.GetBytes(message.Position.x); // 9
        byte[] position_y = System.BitConverter.GetBytes(message.Position.y); // 13
        byte[] position_z = System.BitConverter.GetBytes(message.Position.z); // 17
        byte[] rotation_x = System.BitConverter.GetBytes(message.Rotation.x); // 21
        byte[] rotation_y = System.BitConverter.GetBytes(message.Rotation.y); // 25
        byte[] rotation_z = System.BitConverter.GetBytes(message.Rotation.z); // 29
        byte[] rotation_w = System.BitConverter.GetBytes(message.Rotation.w); // 33
        byte[] velocity_x = System.BitConverter.GetBytes(message.Velocity.x); // 37
        byte[] velocity_y = System.BitConverter.GetBytes(message.Velocity.y); // 41
        byte[] velocity_z = System.BitConverter.GetBytes(message.Velocity.z); // 45


        System.Buffer.BlockCopy(entityID, 0, packedMessage, 1, 4);
        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 5, 4);
        System.Buffer.BlockCopy(position_x, 0, packedMessage, 9, 4);
        System.Buffer.BlockCopy(position_y, 0, packedMessage, 13, 4);
        System.Buffer.BlockCopy(position_z, 0, packedMessage, 17, 4);
        System.Buffer.BlockCopy(rotation_x, 0, packedMessage, 21, 4);
        System.Buffer.BlockCopy(rotation_y, 0, packedMessage, 25, 4);
        System.Buffer.BlockCopy(rotation_z, 0, packedMessage, 29, 4);
        System.Buffer.BlockCopy(rotation_w, 0, packedMessage, 33, 4);
        System.Buffer.BlockCopy(velocity_x, 0, packedMessage, 37, 4);
        System.Buffer.BlockCopy(velocity_y, 0, packedMessage, 41, 4);
        System.Buffer.BlockCopy(velocity_z, 0, packedMessage, 45, 4);

        return packedMessage;
    }

    private static SC_MovementData UnpackMovementMsg(byte[] packedMessage) {

        int entityID = System.BitConverter.ToInt32(packedMessage, 1);
        float timeStamp = System.BitConverter.ToSingle(packedMessage, 5);
        float position_x = System.BitConverter.ToSingle(packedMessage, 9);
        float position_y = System.BitConverter.ToSingle(packedMessage, 13);
        float position_z = System.BitConverter.ToSingle(packedMessage, 17);
        float rotation_x = System.BitConverter.ToSingle(packedMessage, 21);
        float rotation_y = System.BitConverter.ToSingle(packedMessage, 25);
        float rotation_z = System.BitConverter.ToSingle(packedMessage, 29);
        float rotation_w = System.BitConverter.ToSingle(packedMessage, 33);
        float velocity_x = System.BitConverter.ToSingle(packedMessage, 37);
        float velocity_y = System.BitConverter.ToSingle(packedMessage, 41);
        float velocity_z = System.BitConverter.ToSingle(packedMessage, 45);

        Vector3 position = new Vector3(position_x, position_y, position_z);
        Quaternion rotation = new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);
        Vector3 velocity = new Vector3(velocity_x, velocity_y, velocity_z);

        SC_MovementData unpacked = new SC_MovementData(entityID, timeStamp, position, rotation, velocity);

        return unpacked;
    }

    private static byte[] PackCreationRequestMsg(CS_ProjectileRequest message) {
        byte[] packedMessage = new byte[33 + headerSize];
        packedMessage[0] = message.Type;

        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp); // 1
        byte[] position_x = System.BitConverter.GetBytes(message.Position.x); // 5
        byte[] position_y = System.BitConverter.GetBytes(message.Position.y); // 9
        byte[] position_z = System.BitConverter.GetBytes(message.Position.z); // 13
        byte[] rotation_x = System.BitConverter.GetBytes(message.Rotation.x); // 17
        byte[] rotation_y = System.BitConverter.GetBytes(message.Rotation.y); // 21
        byte[] rotation_z = System.BitConverter.GetBytes(message.Rotation.z); // 25
        byte[] rotation_w = System.BitConverter.GetBytes(message.Rotation.w); // 29
        // object type is a byte so no need to use GetBytes                   // 33

        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
        System.Buffer.BlockCopy(position_x, 0, packedMessage, 5, 4);
        System.Buffer.BlockCopy(position_y, 0, packedMessage, 9, 4);
        System.Buffer.BlockCopy(position_z, 0, packedMessage, 13, 4);
        System.Buffer.BlockCopy(rotation_x, 0, packedMessage, 17, 4);
        System.Buffer.BlockCopy(rotation_y, 0, packedMessage, 21, 4);
        System.Buffer.BlockCopy(rotation_z, 0, packedMessage, 25, 4);
        System.Buffer.BlockCopy(rotation_w, 0, packedMessage, 29, 4);
        packedMessage[33] = message.ObjectType;
        return packedMessage;
    }

	private static CS_MissileRequest UnpackMissileRequestMsg(byte[] packedMessage) {

        float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
        float position_x = System.BitConverter.ToSingle(packedMessage, 5);
        float position_y = System.BitConverter.ToSingle(packedMessage, 9);
        float position_z = System.BitConverter.ToSingle(packedMessage, 13);
        float rotation_x = System.BitConverter.ToSingle(packedMessage, 17);
        float rotation_y = System.BitConverter.ToSingle(packedMessage, 21);
        float rotation_z = System.BitConverter.ToSingle(packedMessage, 25);
        float rotation_w = System.BitConverter.ToSingle(packedMessage, 29);
		int targetId = System.BitConverter.ToInt32(packedMessage, 33);

        Vector3 position = new Vector3(position_x, position_y, position_z);
        Quaternion rotation = new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);

		CS_MissileRequest unpacked = new CS_MissileRequest(timeStamp, position, rotation, targetId);

        return unpacked;
    }

	private static byte[] PackMissileRequestMsg(CS_MissileRequest message) {
		byte[] packedMessage = new byte[36 + headerSize];
		packedMessage[0] = message.Type;

		byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp); // 1
		byte[] position_x = System.BitConverter.GetBytes(message.Position.x); // 5
		byte[] position_y = System.BitConverter.GetBytes(message.Position.y); // 9
		byte[] position_z = System.BitConverter.GetBytes(message.Position.z); // 13
		byte[] rotation_x = System.BitConverter.GetBytes(message.Rotation.x); // 17
		byte[] rotation_y = System.BitConverter.GetBytes(message.Rotation.y); // 21
		byte[] rotation_z = System.BitConverter.GetBytes(message.Rotation.z); // 25
		byte[] rotation_w = System.BitConverter.GetBytes(message.Rotation.w); // 29
		byte[] targetId = System.BitConverter.GetBytes(message.TargetId); // 33

		System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
		System.Buffer.BlockCopy(position_x, 0, packedMessage, 5, 4);
		System.Buffer.BlockCopy(position_y, 0, packedMessage, 9, 4);
		System.Buffer.BlockCopy(position_z, 0, packedMessage, 13, 4);
		System.Buffer.BlockCopy(rotation_x, 0, packedMessage, 17, 4);
		System.Buffer.BlockCopy(rotation_y, 0, packedMessage, 21, 4);
		System.Buffer.BlockCopy(rotation_z, 0, packedMessage, 25, 4);
		System.Buffer.BlockCopy(rotation_w, 0, packedMessage, 29, 4);
		System.Buffer.BlockCopy(targetId, 0, packedMessage, 33, 4);
		return packedMessage;
	}

	private static CS_ProjectileRequest UnpackCreationRequestMsg(byte[] packedMessage) {

		float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
		float position_x = System.BitConverter.ToSingle(packedMessage, 5);
		float position_y = System.BitConverter.ToSingle(packedMessage, 9);
		float position_z = System.BitConverter.ToSingle(packedMessage, 13);
		float rotation_x = System.BitConverter.ToSingle(packedMessage, 17);
		float rotation_y = System.BitConverter.ToSingle(packedMessage, 21);
		float rotation_z = System.BitConverter.ToSingle(packedMessage, 25);
		float rotation_w = System.BitConverter.ToSingle(packedMessage, 29);
		byte objectType = packedMessage[33];

		Vector3 position = new Vector3(position_x, position_y, position_z);
		Quaternion rotation = new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);

		CS_ProjectileRequest unpacked = new CS_ProjectileRequest(timeStamp, position, rotation, objectType);

		return unpacked;
	}

    private static byte[] PackEntityCreatedMsg(SC_EntityCreated message) {
        byte[] packedMessage = new byte[41 + headerSize];
        packedMessage[0] = message.Type;

        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp);   // 1
        byte[] position_x = System.BitConverter.GetBytes(message.Position.x); // 5
        byte[] position_y = System.BitConverter.GetBytes(message.Position.y); // 9
        byte[] position_z = System.BitConverter.GetBytes(message.Position.z); // 13
        byte[] rotation_x = System.BitConverter.GetBytes(message.Rotation.x); // 17
        byte[] rotation_y = System.BitConverter.GetBytes(message.Rotation.y); // 21
        byte[] rotation_z = System.BitConverter.GetBytes(message.Rotation.z); // 25
        byte[] rotation_w = System.BitConverter.GetBytes(message.Rotation.w); // 29
        // object type is a byte so no need to use GetBytes                   // 33
        byte[] entityID = System.BitConverter.GetBytes(message.EntityID);     // 34
        byte[] clientID = System.BitConverter.GetBytes(message.ClientID);     // 38

        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
        System.Buffer.BlockCopy(position_x, 0, packedMessage, 5, 4);
        System.Buffer.BlockCopy(position_y, 0, packedMessage, 9, 4);
        System.Buffer.BlockCopy(position_z, 0, packedMessage, 13, 4);
        System.Buffer.BlockCopy(rotation_x, 0, packedMessage, 17, 4);
        System.Buffer.BlockCopy(rotation_y, 0, packedMessage, 21, 4);
        System.Buffer.BlockCopy(rotation_z, 0, packedMessage, 25, 4);
        System.Buffer.BlockCopy(rotation_w, 0, packedMessage, 29, 4);
        packedMessage[33] = message.ObjectType;
        System.Buffer.BlockCopy(entityID, 0, packedMessage, 34, 4);
        System.Buffer.BlockCopy(clientID, 0, packedMessage, 38, 4);

        return packedMessage;
    }

    private static SC_EntityCreated UnpackEntityCreatedMsg(byte[] packedMessage) {

        float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
        float position_x = System.BitConverter.ToSingle(packedMessage, 5);
        float position_y = System.BitConverter.ToSingle(packedMessage, 9);
        float position_z = System.BitConverter.ToSingle(packedMessage, 13);
        float rotation_x = System.BitConverter.ToSingle(packedMessage, 17);
        float rotation_y = System.BitConverter.ToSingle(packedMessage, 21);
        float rotation_z = System.BitConverter.ToSingle(packedMessage, 25);
        float rotation_w = System.BitConverter.ToSingle(packedMessage, 29);
        byte objectType = packedMessage[33];
        int entityID = System.BitConverter.ToInt32(packedMessage, 34);
        int clientID = System.BitConverter.ToInt32(packedMessage, 38);


        Vector3 position = new Vector3(position_x, position_y, position_z);
        Quaternion rotation = new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);

        SC_EntityCreated unpacked = new SC_EntityCreated(entityID, timeStamp, position, rotation, clientID, objectType);

        return unpacked;
    }

    private static byte[] PackEntityDestroyedMsg(SC_EntityDestroyed message) {
        byte[] packedMessage = new byte[8 + headerSize];
        packedMessage[0] = message.Type;

        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp);   // 1
        byte[] entityID = System.BitConverter.GetBytes(message.EntityID);     // 5

        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
        System.Buffer.BlockCopy(entityID, 0, packedMessage, 5, 4);

        return packedMessage;
    }

    private static SC_EntityDestroyed UnpackEntityDestroyedMsg(byte[] packedMessage) {

        float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
        int entityID = System.BitConverter.ToInt32(packedMessage, 5);

        SC_EntityDestroyed unpacked = new SC_EntityDestroyed(entityID, timeStamp);

        return unpacked;
    }

    private static byte[] PackPlayerDataMsg(SC_PlayerData message) {
        byte[] packedMessage = new byte[16 + headerSize];
        packedMessage[0] = message.Type;

        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp);   // 1
        byte[] playerID = System.BitConverter.GetBytes(message.PlayerID);     // 5
        byte[] health = System.BitConverter.GetBytes(message.Health);     // 9
        byte[] score = System.BitConverter.GetBytes(message.Score);     // 13


        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
        System.Buffer.BlockCopy(playerID, 0, packedMessage, 5, 4);
        System.Buffer.BlockCopy(health, 0, packedMessage, 9, 4);
        System.Buffer.BlockCopy(score, 0, packedMessage, 13, 4);

        return packedMessage;
    }

    private static SC_PlayerData UnpackPlayerDataMsg(byte[] packedMessage) {

        float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
        int playerID = System.BitConverter.ToInt32(packedMessage, 5);
        int health = System.BitConverter.ToInt32(packedMessage, 9);
        int score = System.BitConverter.ToInt32(packedMessage, 13);

        SC_PlayerData unpacked = new SC_PlayerData(playerID, timeStamp, health, score);

        return unpacked;
    }


    private static byte[] PackShipCreatedMsg(MSG_ShipCreated message) {
        byte[] packedMessage = new byte[41 + headerSize + name_size];
        packedMessage[0] = message.Type;

        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp);   // 1
        byte[] position_x = System.BitConverter.GetBytes(message.Position.x); // 5
        byte[] position_y = System.BitConverter.GetBytes(message.Position.y); // 9
        byte[] position_z = System.BitConverter.GetBytes(message.Position.z); // 13
        byte[] rotation_x = System.BitConverter.GetBytes(message.Rotation.x); // 17
        byte[] rotation_y = System.BitConverter.GetBytes(message.Rotation.y); // 21
        byte[] rotation_z = System.BitConverter.GetBytes(message.Rotation.z); // 25
        byte[] rotation_w = System.BitConverter.GetBytes(message.Rotation.w); // 29
        // ship type is a byte so no need to use GetBytes                     // 33
        byte[] entityID = System.BitConverter.GetBytes(message.EntityID);     // 34
        byte[] clientID = System.BitConverter.GetBytes(message.ClientID);     // 38
        byte[] playerNamePadded = System.Text.Encoding.ASCII.GetBytes(message.PlayerName.PadRight(name_size, ' ')); // 42

        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
        System.Buffer.BlockCopy(position_x, 0, packedMessage, 5, 4);
        System.Buffer.BlockCopy(position_y, 0, packedMessage, 9, 4);
        System.Buffer.BlockCopy(position_z, 0, packedMessage, 13, 4);
        System.Buffer.BlockCopy(rotation_x, 0, packedMessage, 17, 4);
        System.Buffer.BlockCopy(rotation_y, 0, packedMessage, 21, 4);
        System.Buffer.BlockCopy(rotation_z, 0, packedMessage, 25, 4);
        System.Buffer.BlockCopy(rotation_w, 0, packedMessage, 29, 4);
        packedMessage[33] = message.ShipType;
        System.Buffer.BlockCopy(entityID, 0, packedMessage, 34, 4);
        System.Buffer.BlockCopy(clientID, 0, packedMessage, 38, 4);
        System.Buffer.BlockCopy(playerNamePadded, 0, packedMessage, 42, name_size);

        return packedMessage;
    }

    private static MSG_ShipCreated UnpackShipCreatedMsg(byte[] packedMessage) {

        float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
        float position_x = System.BitConverter.ToSingle(packedMessage, 5);
        float position_y = System.BitConverter.ToSingle(packedMessage, 9);
        float position_z = System.BitConverter.ToSingle(packedMessage, 13);
        float rotation_x = System.BitConverter.ToSingle(packedMessage, 17);
        float rotation_y = System.BitConverter.ToSingle(packedMessage, 21);
        float rotation_z = System.BitConverter.ToSingle(packedMessage, 25);
        float rotation_w = System.BitConverter.ToSingle(packedMessage, 29);
        byte shipType = packedMessage[33];
        int entityID = System.BitConverter.ToInt32(packedMessage, 34);
        int clientID = System.BitConverter.ToInt32(packedMessage, 38);
        string playerNamePadded = System.Text.Encoding.ASCII.GetString(packedMessage, 42, name_size);
        string playerName = playerNamePadded.TrimEnd();

        Vector3 position = new Vector3(position_x, position_y, position_z);
        Quaternion rotation = new Quaternion(rotation_x, rotation_y, rotation_z, rotation_w);

        MSG_ShipCreated unpacked = new MSG_ShipCreated(entityID, timeStamp, position, rotation, clientID, shipType, playerName);

        return unpacked;
    }


    private static byte[] PackNewPlayerRequestMsg(MSG_NewPlayerRequest message) {
        byte[] packedMessage = new byte[13 + headerSize + name_size];
        packedMessage[0] = message.Type;

        byte[] timeStamp = System.BitConverter.GetBytes(message.TimeStamp);   // 1
        // ship type is a byte so no need to use GetBytes                   // 5
        byte[] entityID = System.BitConverter.GetBytes(message.EntityID);     // 6
        byte[] clientID = System.BitConverter.GetBytes(message.ClientID);     // 10
        byte[] playerNamePadded = System.Text.Encoding.ASCII.GetBytes(message.PlayerName.PadRight(name_size, ' ')); // 14

        System.Buffer.BlockCopy(timeStamp, 0, packedMessage, 1, 4);
        packedMessage[5] = message.ShipType;
        System.Buffer.BlockCopy(entityID, 0, packedMessage, 6, 4);
        System.Buffer.BlockCopy(clientID, 0, packedMessage, 10, 4);
        System.Buffer.BlockCopy(playerNamePadded, 0, packedMessage, 14, name_size);

        return packedMessage;
    }

    private static MSG_NewPlayerRequest UnpackNewPlayerRequestMsg(byte[] packedMessage) {

        float timeStamp = System.BitConverter.ToSingle(packedMessage, 1);
        byte shipType = packedMessage[5];
        int entityID = System.BitConverter.ToInt32(packedMessage, 6);
        int clientID = System.BitConverter.ToInt32(packedMessage, 10);
        string playerNamePadded = System.Text.Encoding.ASCII.GetString(packedMessage, 14, name_size);
        string playerName = playerNamePadded.TrimEnd();

        MSG_NewPlayerRequest unpacked = new MSG_NewPlayerRequest(entityID, timeStamp, clientID, shipType, playerName);

        return unpacked;
    }
}
