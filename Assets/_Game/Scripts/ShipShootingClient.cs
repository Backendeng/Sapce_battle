using UnityEngine;
using UnityEngine.Networking;

public class ShipShootingClient : MonoBehaviour {

    protected Client clientController;

    private AudioSource projectileSound;
    private Camera playerCamera;

    public AudioClip projectileClip;
    public GameObject projectile;   // projectile prefab
    public GameObject missile;      // missile prefab
    public Transform shotSpawn;     // shooting spawn location

    [Header("Shooting")]
    private float fireRate1 = 0.1f;
    private float nextFire1;
    private float fireRate2 = 0.5f;
    private float nextFire2;
    private readonly float rayRange = 1000f;

    
    private void Awake() {
        projectileSound = GetComponent<AudioSource>();
        projectileSound.clip = projectileClip;
        playerCamera = GetComponentInChildren<Camera>();
    }

    public void Init(Client clientController, int entityID) {
        this.clientController = clientController;
    }

    public void HandleShooting() {
        // Secondary Shot - Missiles
        if (Time.time > nextFire2 && (Input.GetButtonDown("LeftTrigger") || Input.GetMouseButtonDown(1))) {
            nextFire2 = Time.time + fireRate2;
            ShootMissile(shotSpawn.position, shotSpawn.rotation);
            
        }
        // Primary Shot - Projectile
        if (Time.time > nextFire1 && (Input.GetButton("RightTrigger") || Input.GetMouseButton(0))) {
            nextFire1 = Time.time + fireRate1;
            ShootProjectile(shotSpawn.position, shotSpawn.rotation);
        }
    }

    private void ShootMissile(Vector3 pos, Quaternion rot) {
        int netTimeStamp = NetworkTransport.GetNetworkTimestamp();
        int targetId = ShootRay();
        Debug.Log("Hit id: " + targetId);
        clientController.SendMissileToHost((byte)NetworkEntity.ObjType.Missile, pos, rot, targetId, netTimeStamp);
        GetComponent<AudioSource>().Play();
    }

    private void ShootProjectile(Vector3 pos, Quaternion rot) {
        float netTimeStamp = NetworkTransport.GetNetworkTimestamp(); // NetworkTimeStamp is int, but we use float because NetMsg uses float for time stamps.
        GameObject mockProjectile = Instantiate(projectile, pos, rot); // destroyed when the real projectile from the server is instantiated.
        projectileSound.Play();
        mockProjectile.GetComponent<Projectile>().ClientID = -1; // // mark as mock projectile (locally simulated untill the real projectile is instantiated)
        clientController.mockProjectiles.Add((int)netTimeStamp, mockProjectile);
        clientController.SendShotToHost((byte)NetworkEntity.ObjType.Projectile, pos, rot, (byte)NetworkEntity.ObjType.Projectile, (int)netTimeStamp);
    }

    private int ShootRay() {
        
        RaycastHit hit;
       // GetComponent<LineRenderer>().enabled = true;
       
        if (Physics.Raycast(shotSpawn.position, playerCamera.transform.forward, out hit, rayRange)) {

            Debug.Log("Hit: " + hit.transform.name);
            NetworkEntity targetEntity = hit.transform.GetComponentInParent<NetworkEntity>();
            if (targetEntity != null)
                return targetEntity.EntityID;
            else
                return -1;
        }
        return -1;
    }


}
