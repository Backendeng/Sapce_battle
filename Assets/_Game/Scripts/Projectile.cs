using UnityEngine;


public class Projectile : NetworkEntity {

    public GameObject missileExplosion;
    public GameObject PT_Explosion;

    public int OwnerID { get; set; } // -1 means mock projectile.

    public static readonly int Damage = 10;
    private static readonly float speed = 800f;
    public static float Speed { get { return speed; } }

    private bool active = true;
    private bool hit = false;
    private readonly float timeout = 2.0f;
    private float destroyTime;

    [Tooltip("Sound Effect")]
    private AudioSource projectileSound;

    public new void Start() {
        base.Start();
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        destroyTime = Time.time + timeout;

        // Sound for other players
        if (!isServer && OwnerID > 0 && PlayerShip.ActiveShip && PlayerShip.ActiveShip.PlayerID != OwnerID) {
            GetComponent<AudioSource>().Play();
        }
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
            other.CompareTag("Player") && OwnerID == other.gameObject.GetComponent<PlayerShip>().PlayerID) // ignore self harming!
        {
            return;
        }
        hit = true;
        //Debug.Log("Projectile hit: = " + other.name + ", entityID = " + other.GetComponent<NetworkEntity>().EntityID + ", projectile owner = " + OwnerID);

        if (!isServer && OwnerID == -1) { // do local effect only for mock projectile. server should calculate hit later
            Destroy(Instantiate(PT_Explosion, transform.position, Quaternion.identity), 1); 
            active = false;
            ////
            DestroyProjectile();
            
            return;
        }

        // On Server:
        if (isServer) {
            //Debug.Log("Projectile hit: " + other.name + ", tag:" + other.tag);

            if (other.CompareTag("Player")) {
                //Debug.Log("Projectile hit: player with playerID = " + other.gameObject.GetComponent<PlayerShip>().PlayerID);
                //gameManager.AddScore(OwnerID, scoreValue);
            }
            if (OwnerID != 0) {
                Explode();
            }
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
        GetComponentInChildren<TrailRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;

        if (!isServer && hit && active) { // if the projectile was destroyed before it did an explosion effect on the client, do it now.
            Destroy(Instantiate(PT_Explosion, transform.position, Quaternion.identity), 1);
        }

        if (!isServer) {
            Destroy(gameObject, 1f);
        }
        else {
            Destroy(gameObject);
        }
    }
}
