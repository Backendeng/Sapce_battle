using UnityEngine;


public class Projectile : NetworkEntity {

    public GameObject missileExplosion;
    public GameObject PT_Explosion;

    public static readonly int Damage = 10;
    private static readonly float speed = 400f;
    
    private bool active = true;
    private bool hit = false;
    private readonly float timeout = 2.0f;
    private float destroyTime;

    public static float Speed { get { return speed; } }

    public new void Start() {
        base.Start();
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        destroyTime = Time.time + timeout;
    }

    private void Update() {
        if (isServer && Time.time > destroyTime && active) {
            serverController.DestroyEntity(EntityID);
        }
        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.SC_MovementData:
                MoveProjUsingReceivedServerData((SC_MovementData)netMessage);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                DestroyProjectile();
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

        if (other.CompareTag("Projectile") || // ignore bullet to bullet collision
            other.CompareTag("Boundary") || // ignore collision with boundary
            other.CompareTag("Player") && ClientID == other.gameObject.GetComponent<PlayerShip>().ClientID) // ignore self harming!
        {
            return;
        }
        hit = true;

        if (!isServer) { // do local effect only
            Destroy(Instantiate(PT_Explosion, transform.position, Quaternion.identity), 1);
            active = false;
            return;
        }

        // On Server:

        //Debug.Log("Projectile hit: " + other.name + ", tag:" + other.tag);

        if (other.CompareTag("Player")) {
            Debug.Log("Projectile hit: player with client id = " + other.gameObject.GetComponent<PlayerShip>().ClientID);
            //gameController.AddScore(scoreValue);
        }

        if (ClientID != 0) {
            Explode();
        }
    }

    private void Explode() {
        // On Server:
        Destroy(Instantiate(PT_Explosion, transform.position, Quaternion.identity), 1);
        GetComponentInChildren<TrailRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;

        serverController.DestroyEntity(EntityID);
        active = false;
    }

    private void DestroyProjectile() {
        if (!isServer && hit && active) { // if the projectile was destroyed before it did an explosion effect on the client, do it now.
            Destroy(Instantiate(PT_Explosion, transform.position, Quaternion.identity), 1);
            Destroy(gameObject);
            return;
        }
        else {
            Destroy(gameObject);
        }
    }
}
