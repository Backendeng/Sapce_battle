using UnityEngine;
using System.Collections;

public class RemotePlayerShipServer : PlayerShip {

    private float respawnCooldown = 7f;
    private float respawnTimer;
    public GameObject spawnPoints;

    private void Update() {
        if (isDead && Time.time > respawnTimer) {
            RespawnEnd();
        }
    }

    private void RespawnEnd() {
        isDead = false;
        serverController.gameManager.UpdatePlayerHealth(PlayerID, initialHealth);
        Health = initialHealth;

        Transform[] points = spawnPoints.GetComponentsInChildren<Transform>(); // why we get spawn in a not existent point? 

        // pick point with max dist from nearest player
        Transform chosenPoint = points[0];
        float maxNearestDist = -1f;
        foreach(Transform point in points) {
            float nearestDist = serverController.GetNearestDistFrom(point, ClientID);
            Debug.Log("Point " + point.position+" nearestDis "+ nearestDist);
            if (maxNearestDist == -1f || nearestDist > maxNearestDist) {
                maxNearestDist = nearestDist;
                chosenPoint = point;
                Debug.Log("Point chosen: " + point.position);
            }

        }
       

        GetComponent<Transform>().SetPositionAndRotation(chosenPoint.position, chosenPoint.rotation);

        Vector3 velocity = new Vector3(0, 0, 0);
        AddSnapshotToQueue(-1, points[0].position, points[0].rotation, velocity);
        StartCoroutine("RespawnEndDelayed");
    }

    IEnumerator RespawnEndDelayed() {
       
        Debug.Log("respawn end before");
        yield return new WaitForSeconds(1.0f);
        Debug.Log("respawn end after");
        
        foreach(Collider c in GetComponents<Collider>()) {
            c.enabled = true;
        }
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            r.enabled = true;
        }
        yield return null;
    }

    public void Respawn() {
        isDead = true;
        respawnTimer = Time.time + respawnCooldown;
        foreach (Collider c in GetComponents<Collider>()) {
            c.enabled = false;
        }
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            r.enabled = false;
        }
    }



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

