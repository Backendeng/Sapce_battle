using System.Collections;
using UnityEngine;

public class Missile : NetworkEntity {

    private bool created = false;
    private Rigidbody rigid_body;

    private static readonly float speed = 80f;
    public static float Speed { get { return speed; } }
    public static readonly float timeout = 10f;
    private float destroyTime;
    private float invisibleTime;
    public static readonly float timetillvisible = 0.3f;
    public int OwnerID { get; set; }

    [Tooltip("Missile Guidance")]
    private Transform target;
    private PlayerShip targetScript;
    public Transform Target { set { target = value; } }
    public PlayerShip TargetScript { set { targetScript = value; } }
    private bool isTargetingPlayer = false;
    public bool IsTargetingPlayer { set { isTargetingPlayer = value; } get { return isTargetingPlayer; }  }

    [Tooltip("Missile Explosion")]
    public GameObject missileExplosion;
    private float explosionRadius = 35.0f;
    private float explosionPower = 20.0f;
    private int missileDamage = 25;

    [Tooltip("Lerping")]
    public static bool doLerp = true;
    MovementInterpolator movementInterpolator;

    private bool explodeOnInit = false;
    public bool ExplodeOnInit { set { explodeOnInit = value; } }

    public new void Start() {
        base.Start();
        rigid_body = GetComponent<Rigidbody>();
           
        if (isServer) {
            destroyTime = Time.time + timeout;
            if (target == null)
            	GetComponent<Rigidbody>().velocity = transform.forward * speed;
        } else {
            movementInterpolator = new MovementInterpolator(transform, EntityID);
            //  GetComponent<CapsuleCollider>().enabled = false;
        }
        created = true;

        if (isServer && explodeOnInit) {
            Explode();
        }

        if(!isServer && isTargetingPlayer) { //  lock warnning for local player 
            clientController.gameManager.LocalPlayerLockCounter++;
        }

        if (!isServer && OwnerID > -1) {
            invisibleTime = Time.time + timetillvisible;
            GetComponentInChildren<Renderer>().enabled = false;
            GetComponentInChildren<ParticleSystem>().Pause();
           
        }
        

    }

    private void Update() {

        if (isServer && Time.time > destroyTime) {
            Explode();
        }
        if (!isServer) {
            if (Time.time > invisibleTime) {
                GetComponentInChildren<Renderer>().enabled = true;
                GetComponentInChildren<ParticleSystem>().Play();
            }
        }

        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.SC_MovementData:
                MoveProjUsingReceivedServerData((SC_MovementData)netMessage);// set state not interpolated
                movementInterpolator.RecUpdate((SC_MovementData)netMessage);
                break;
            case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                Debug.Log("Missile Destroyed " + EntityID);
                DestroyMissile();
                break;
            default:
                Debug.Log("ERROR! RemoteProjectile on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                break;
        }

    }

    private void FixedUpdate() {

        if (isServer) {
            if (target == null) { // TODO: handle case when target is suddenly gone
                return;
            }
            if (targetScript.IsDead) { //target died,  remove lock
                GetComponent<Rigidbody>().velocity = transform.forward * speed;
                isTargetingPlayer = false; 
                target = null;
            }
            transform.LookAt(target);
         
            rigid_body.velocity = transform.forward * speed; 
          
        }

        if (!isServer && doLerp) {
            movementInterpolator.InterpolateMovement();
        }
    }

    private void MoveProjUsingReceivedServerData(SC_MovementData message) {
        if (doLerp) {
            return;
        }

        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    private bool exploaded = false;
    public void Explode() {
        if (exploaded || !created)
            return;
        exploaded = true;

        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        ArrayList hitPlayers = new ArrayList();

        //Debug.Log("Expl cnt " + colliders.Length);
        foreach (Collider hit in colliders) {
          //  Debug.Log("EXPL HIT" + hit.name);
            if (hit.CompareTag("Player")) {
                
                if (!hitPlayers.Contains(hit.gameObject.GetComponent<PlayerShip>().ClientID)) {//prevent mult dmg to same player
                    Debug.Log("Expl hit: player with client id = " + hit.gameObject.GetComponent<PlayerShip>().ClientID);
                    hit.gameObject.GetComponent<Target>().TakeDamage(missileDamage, OwnerID);
                }
                hitPlayers.Add(hit.gameObject.GetComponent<PlayerShip>().ClientID);
                continue;
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == rigid_body) { // ignore the exploding missile
                continue;
            }

            if (hit.CompareTag("Missile")) {
                hit.GetComponent<Missile>().Explode();
                continue;
            }
            if (rb != null) {
                rb.AddExplosionForce(explosionPower, explosionPos, explosionRadius, 0, ForceMode.Impulse); // players should not be effected
            }
        }
        GetComponent<CapsuleCollider>().enabled = false;
       
        serverController.DestroyEntity(EntityID);//nullreff here
    }

 
    void OnTriggerEnter(Collider other) {

         if (//other.CompareTag("Projectile") || // ignore bullet to bullet collision
             other.CompareTag("Boundary") ) // ignore collision with boundary
             // || other.CompareTag("Player") && ClientID == other.gameObject.GetComponent<PlayerShip>().ClientID) // ignore self harming!
         {
             return;
         }
 
        NetworkEntity hitObj = other.gameObject.GetComponent<NetworkEntity>();
       /* if (hitObj != null)
            Debug.Log("missile hit entity = " + hitObj.EntityID);
        else
            Debug.Log("missile hit obj = " + other.name);
            */
        if (!isServer) { // do local effect only
           // Destroy(Instantiate(missileExplosion, transform.position, Quaternion.identity), 1);  seems that we dont need to explode also on client  (tested localy)
            return;
        }

        // On Server:

        // direct hit
        if (other.CompareTag("Player")) {
            //Debug.Log("Missile hit: player with client id = " + other.gameObject.GetComponent<PlayerShip>().ClientID);
            //gameController.AddScore(scoreValue);
        }

        Explode();
    }

   public void DestroyMissile() {
        Destroy(Instantiate(missileExplosion, transform.position, Quaternion.identity), 2); // on client and server.
        Destroy(gameObject);

        if (!isServer && isTargetingPlayer && clientController.gameManager.LocalPlayerLockCounter > 0) { //  lock warnning for local player 
            clientController.gameManager.LocalPlayerLockCounter--;
        }

    }


}
