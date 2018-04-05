using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astroid : NetworkEntity {

    // Use this for initialization
    new void Start () {
        base.Start();
        ObjectType = (byte)NetworkEntity.ObjType.Astroid;

    }

    // Update is called once per frame
    private void Update() {
        if (isServer)
            return;
        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.SC_MovementData:
                MoveProjUsingReceivedServerData((SC_MovementData)netMessage);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                Destroy(gameObject);
                break;
            default:
                Debug.Log("ERROR! RemoteProjectile on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                break;
        }
    }

    private void MoveProjUsingReceivedServerData(SC_MovementData message) {
        // transform.position = Vector3.Lerp(transform.position, message.Position, LERP_MUL * Time.deltaTime);
        //transform.rotation = Quaternion.Lerp(transform.rotation, message.Rotation, LERP_MUL * Time.deltaTime);
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }
}
