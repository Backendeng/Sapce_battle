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
                jsonMsg = JsonConvert.SerializeObject( (SC_MovementData)message );
                break;
            case (byte)NetMsg.MsgType.CS_InputData:
                jsonMsg = JsonConvert.SerializeObject( (CS_InputData)message );
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
                unpackedMessage = JsonConvert.DeserializeObject<SC_MovementData>(jsonMsg);
                break;
            case (byte)NetMsg.MsgType.CS_InputData:
                unpackedMessage = JsonConvert.DeserializeObject<CS_InputData>(jsonMsg);
                break;
            default:
                unpackedMessage = null;
                break;
        }

        return unpackedMessage;
    }    
}
