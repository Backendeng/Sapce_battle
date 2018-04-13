using System.Collections;
using UnityEngine;

public class Missile : NetworkEntity {
    public float speed;
    public GameObject missileExplosion;
    public float timeout = 5.0f;
    public static float LERP_MUL = 3f;

    public new void Start() {
        base.Start();
        if (isServer) {
            GetComponent<Rigidbody>().velocity = transform.forward * speed;
        }
    }

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
        //transform.position = Vector3.Lerp(transform.position, message.Position, LERP_MUL * Time.deltaTime);
        //transform.rotation = Quaternion.Lerp(transform.rotation, message.Rotation, LERP_MUL * Time.deltaTime);
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    private void Awake() {
      //  Destroy(gameObject, timeout);
    }

    public void OnBecameInvisible() {
        //Destroy(gameObject);
    }

    // when the missile hits something
    void OnTriggerEnter(Collider other) {
        // ignore bullet to bullet collision
        if (other.name == "Missile")
            return;

        // ignore collision with boundary or other projectiles
        if (other.name == "Boundary")
            return;

        if (missileExplosion)
            Instantiate(missileExplosion, transform.position, transform.rotation);

        if (other.CompareTag("Player")) {
            // make hitting effects
            //Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
        }
        //gameController.AddScore(scoreValue);

        //Destroy(gameObject);
    }

}
