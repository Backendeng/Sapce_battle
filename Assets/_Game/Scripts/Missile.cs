using System.Collections;
using UnityEngine;

public class Missile : NetworkEntity {

    public GameObject missileExplosion;

    private Rigidbody rigid_body;
    private Transform target;
    
    public float speed = 50f;
    public float timeout = 5.0f;
    public static float LERP_MUL = 3f;
	public Transform Target { set { target = value; } }

    public new void Start() {
        base.Start();
        if (isServer) {
            rigid_body = GetComponent<Rigidbody>();
            if (target == null)
            	GetComponent<Rigidbody>().velocity = transform.forward * speed;
        }
    }

    private void Update() {
		
        if (incomingQueue.Count == 0)
            return;
        NetMsg netMessage = incomingQueue.Dequeue();
        switch (netMessage.Type) {
            case (byte)NetMsg.MsgType.SC_MovementData:
                MoveProjUsingReceivedServerData((SC_MovementData)netMessage);
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
            transform.LookAt(target);
         
            rigid_body.velocity = transform.forward * speed; 
          
        }
    }

    private void MoveProjUsingReceivedServerData(SC_MovementData message) {
        GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    
    void OnTriggerEnter(Collider other) {

         if (//other.CompareTag("Projectile") || // ignore bullet to bullet collision
             other.CompareTag("Boundary") ) // ignore collision with boundary
             // || other.CompareTag("Player") && ClientID == other.gameObject.GetComponent<PlayerShip>().ClientID) // ignore self harming!
         {
             return;
         }
 
        NetworkEntity hitObj = other.gameObject.GetComponent<NetworkEntity>();
        if (hitObj != null)
            Debug.Log("missile hit entity = " + hitObj.EntityID);
        else
            Debug.Log("missile hit obj = " + other.name);

        if (!isServer) { // do local effect only
            Debug.Log("BOOM " + EntityID);
           // Destroy(Instantiate(missileExplosion, transform.position, Quaternion.identity), 1);  seems that we dont need to explode also on client  (tested localy)
            return;
        }

        // On Server:

        if (other.CompareTag("Player")) {
            Debug.Log("Missile hit: player with client id = " + other.gameObject.GetComponent<PlayerShip>().ClientID);
            //gameController.AddScore(scoreValue);
        }
        Explode();
    }

    private void Explode() {
        // On Server:
      
        Destroy(Instantiate(missileExplosion, transform.position, Quaternion.identity), 1);// cosmetic
        //GetComponentInChildren<TrailRenderer>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;

        serverController.DestroyEntity(EntityID); 
    }

    private void DestroyMissile() {
        if (!isServer) { // if the projectile was destroyed before it did an explosion effect on the client, do it now.
            Destroy(Instantiate(missileExplosion, transform.position, Quaternion.identity), 1);// whats wrong with spawning new objects in onDestroy?
            Destroy(gameObject);
            return;
        }
        else {
            Destroy(gameObject);
        }
    }

}
