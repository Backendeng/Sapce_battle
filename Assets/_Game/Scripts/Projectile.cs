using UnityEngine;

public class Projectile : NetworkEntity {
    private int clientID = 0; // owner
    private static float speed = 400f;
    
    private float timeout = 2.0f;
    private float destroyTime;
    private Rigidbody rigidBody;

    public GameObject missileExplosion;
    public GameObject PT_Explosion;

    public static float Speed { get { return speed; } }
    public int ClientID { get { return clientID; } set { clientID = value; } }

    public new void Start() {
        base.Start();
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        destroyTime = Time.time + timeout;
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update() {
        if (isServer && Time.time > destroyTime) {
            serverController.DestroyEntity(entityID);
        }
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
                Debug.Log("ERROR! Projectile on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                break;
        }
    }

    private void MoveProjUsingReceivedServerData(SC_MovementData message) {
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    // when the missile hits something
    void OnTriggerEnter(Collider other) {
        Debug.Log("HIT: " + other.name + ", tag:" + other.tag);

        if (other.CompareTag("Player")) {
            Debug.Log("hit player with client id = " + other.gameObject.GetComponent<PlayerShip>().ClientID);
        }

        // ignore self harming!
            if (other.CompareTag("Player") && clientID == other.gameObject.GetComponent<PlayerShip>().ClientID) {
            return;
        }

        // ignore bullet to bullet collision
        if (other.CompareTag("Projectile"))
            return;


        // ignore collision with boundary or other projectiles
        if (other.name == "Boundary")
            return;

        if (other.CompareTag("Player")) {
            // make hitting effects
            //Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
        }
        //gameController.AddScore(scoreValue);

        if (clientID != -1) {
            Explode();
        }
    }

    private void Explode() {
        if (PT_Explosion)
            Destroy(Instantiate(PT_Explosion, transform.position, Quaternion.identity), 1);
        GetComponentInChildren<TrailRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        if (isServer) {
            //serverController.DestroyEntity(entityID); // before enabling this line, make sure the server doesn't send timed destroy msg for the projectils
        }
    }

}
