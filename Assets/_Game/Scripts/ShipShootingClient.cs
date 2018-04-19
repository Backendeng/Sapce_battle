using UnityEngine;

public class ShipShootingClient : MonoBehaviour {

    [Header("Shooting")]
    private float fireRate1 = 0.1f;
    private float nextFire1;
    private float fireRate2 = 0.5f;
    private float nextFire2;
    public GameObject projectile;   // projectile prefab
    public GameObject missile;      // missile prefab
    public Transform shotSpawn;     // shooting spawn location

    // Shooting : Phaser
    public GameObject phaser;
    public ParticleSystem phaserSparks;
    float rayRange = 500f;

    LocalPlayerShip ship;
    protected Client clientController;
    protected int entityID;

    private void Awake() {

        //missile = Resources.Load<GameObject>("Prefabs/Missile");
        //phaser = Resources.Load<GameObject>("Prefabs/Phaser");
        ship = GetComponent<LocalPlayerShip>();
    }

    public void Init(Client clientController, int entityID) {
        this.clientController = clientController;
        this.entityID = entityID;
    }

    public void HandleShooting() {
        // Secondary Shot - Missiles
        if (Time.time > nextFire2 && (Input.GetButtonDown("LeftTrigger") || Input.GetMouseButtonDown(1))) {
            nextFire2 = Time.time + fireRate2;
            //Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            ShootMissile(shotSpawn.position, shotSpawn.rotation);
            
        }
        // Primary Shot - Projectile
        if (Time.time > nextFire1 && (Input.GetButton("RightTrigger") || Input.GetMouseButton(0))) {
            nextFire1 = Time.time + fireRate1;
            ShootProjectile(shotSpawn.position, shotSpawn.rotation);
        }
    }

    private void ShootMissile(Vector3 pos, Quaternion rot) {
        clientController.SendShotToHost((byte)NetworkEntity.ObjType.Missile, entityID, pos, rot, (byte)NetworkEntity.ObjType.Missile);
        GetComponent<AudioSource>().Play();
    }

    private void ShootProjectile(Vector3 pos, Quaternion rot) {
        GameObject simProjectile = Instantiate(projectile, pos, rot);
        clientController.SendShotToHost((byte)NetworkEntity.ObjType.Projectile, entityID, pos, rot, (byte)NetworkEntity.ObjType.Projectile);
        GetComponent<AudioSource>().Play();

        simProjectile.GetComponent<Projectile>().ClientID = -1; // mark as locally simulated projectile

    }

    private void ShootRay() {
        RaycastHit hit;
        //GetComponent<LineRenderer>().enabled = true;
        GameObject phaser1 = Instantiate(phaser, shotSpawn);
        if (Physics.Raycast(shotSpawn.position, ship.playerCamera.transform.forward, out hit, rayRange)) {

            Debug.Log("Hit: " + hit.transform.name);
        }
    }


}
