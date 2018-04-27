using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MessagesHandler {
    public static int headerSize = 1;

    public static byte[] NetMsgPack(NetMsg message) {
        byte[] bytesMessage;
        string jsonMsg;

        switch (message.Type) {
            case (byte)NetMsg.MsgType.SC_EntityCreated:
                jsonMsg = JsonConvert.SerializeObject( (SC_EntityCreated)message );
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                jsonMsg = JsonConvert.SerializeObject( (SC_EntityDestroyed)message );
                break;
            case (byte)NetMsg.MsgType.SC_MovementData:
                //jsonMsg = JsonConvert.SerializeObject( (SC_MovementData)message );
                bytesMessage = PackMovementMsg((SC_MovementData)message);
                return bytesMessage;
                //break;
            case (byte)NetMsg.MsgType.CS_InputData:
                jsonMsg = JsonConvert.SerializeObject( (CS_InputData)message );
                break;
            case (byte)NetMsg.MsgType.SC_AllocClientID:
                jsonMsg = JsonConvert.SerializeObject((SC_AllocClientID)message);
                break;
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
                unpackedMessage = JsonConvert.DeserializeObject<SC_EntityCreated>(jsonMsg);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                unpackedMessage = JsonConvert.DeserializeObject<SC_EntityDestroyed>(jsonMsg);
                break;
            case (byte)NetMsg.MsgType.SC_MovementData:
                //unpackedMessage = JsonConvert.DeserializeObject<SC_MovementData>(jsonMsg);
                unpackedMessage = UnpackMovementMsg(packedMessage);
                break;
            case (byte)NetMsg.MsgType.CS_InputData:
                unpackedMessage = JsonConvert.DeserializeObject<CS_InputData>(jsonMsg);
                break;
            case (byte)NetMsg.MsgType.SC_AllocClientID:
                unpackedMessage = JsonConvert.DeserializeObject<SC_AllocClientID>(jsonMsg);
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
}

