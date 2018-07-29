using UnityEngine;


public class RemotePlayerShipServer : PlayerShip {

    Vector3 velocity;
    public override Vector3 Velocity { get { return velocity; } }

    private void FixedUpdate() {
        HandleIncomingMessages();
    }

    public override Vector3 GetVelocity() {
        return lastReceivedVelocity;
    }

    private void HandleIncomingMessages() {
        if (incomingQueue.Count == 0) {
            return;
        }
        NetMsg netMessage = incomingQueue.Dequeue();

        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.SC_MovementData:
                MoveShipUsingReceivedClientData((SC_MovementData)netMessage);
                velocity = ((SC_MovementData)netMessage).Velocity;
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                Destroy(gameObject);
                break;
            default:
                Debug.Log("Invalid Message: RemotePlayerShip on Server reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                break;
        }
    }

    private void MoveShipUsingReceivedClientData(SC_MovementData message) {

        lastReceivedStateTime = message.TimeStamp;
        lastReceivedVelocity = message.Velocity;
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);

        AddSnapshotToQueue(message.TimeStamp, message.Position, message.Rotation, message.Velocity);
        //Debug.Log("registered ts "+ lastReceivedStateTime);////////////////////////////////////////////
    }

    // we run this till we get null
    public override SC_MovementData GetNextSnapshot(int entityId) {
        StateSnapshot ss = GetNextSnapshotFromQueue();
        if (ss == null)
            return null;

        SC_MovementData msg = new SC_MovementData(entityId, ss.time, ss.position, ss.rotation, ss.velocity);
        return msg;
    }

}

