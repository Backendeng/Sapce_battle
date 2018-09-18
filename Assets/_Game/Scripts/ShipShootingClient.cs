using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ShipShootingClient : MonoBehaviour {

    protected Client clientController;

    private AudioSource audioSource;
    public AudioClip projectileSoundClip;
    public AudioClip missileSoundClip;
    
    private Camera playerCamera;

    //public AudioClip projectileClip;
    public GameObject projectile;   // projectile prefab
    public GameObject missile;      // missile prefab
    public Transform shotSpawn;     // shooting spawn location

    private float lockRadius;
    private Vector2 screenCenter;
    public int lockTargetID = -1;
    //private float lockTime = 5f;
    //private float unlockTime = 0f;

    [Header("Shooting")]
    private readonly float fireRate1 = 0.1f;
    private float nextFire1;
    private readonly float fireRate2 = 0.5f;
    private float nextFire2;
    private readonly float rayRange = 1000f;
    private int energy = 100;
    private readonly int maxEnergy = 100;
    private readonly float energyChargeRate = 0.1f;
    private float nextEnergyCharge;
    private readonly int energyDrain = 5;
    private readonly int missileEnergyDrain = 75;

    public int Energy { get { return energy; } }

    private void Awake() {
        audioSource = GetComponents<AudioSource>()[0];
        playerCamera = GetComponentInChildren<Camera>();
        lockRadius = (130f / 1080f) * Screen.height;
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    }


    public void Update() {
       
        if (Time.time > nextEnergyCharge && energy < maxEnergy) {
            nextEnergyCharge = Time.time + energyChargeRate;
            energy++;
        }
        /*
        if (Time.time > unlockTime) {
            lockTargetID = -1;
        }
        */
    }

    public void Init(Client clientController, int entityID) {
        this.clientController = clientController;
    }

    public void ResetEnergy() {
        energy = maxEnergy;
    }

    public void HandleShooting() {
        // Secondary Shot - Missiles
        if ((Input.GetButtonDown("LeftTrigger") || Input.GetMouseButtonDown(1)) || Input.GetKeyDown(KeyCode.LeftShift)) {
            lockTargetID = LockOnTarget();
            //unlockTime = Time.time + lockTime;
        }
        if (Time.time > nextFire2 && energy > missileEnergyDrain && (Input.GetButtonUp("LeftTrigger") || Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.R)) ) {
            nextFire2 = Time.time + fireRate2;
            ShootMissile(shotSpawn.position, shotSpawn.rotation);
            //unlockTime = Time.time + Missile.timeout + 2f;
        }
        // Primary Shot - Projectile
        if (Time.time > nextFire1 && energy > energyDrain && (Input.GetButton("RightTrigger") || Input.GetMouseButton(0))) {
            nextFire1 = Time.time + fireRate1;
            ShootProjectile(shotSpawn.position, shotSpawn.rotation);
        }
    }

    private void ShootMissile(Vector3 pos, Quaternion rot) {
        int netTimeStamp = NetworkTransport.GetNetworkTimestamp();
        int targetId = lockTargetID;
        Debug.Log("Hit id: " + targetId);
        
        clientController.SendMissileToHost((byte)NetworkEntity.ObjType.Missile, pos, rot, targetId, netTimeStamp);
        energy -= missileEnergyDrain;
        //audioSource.clip = missileSoundClip;
        //audioSource.Play();
    }

    private int LockOnTarget() {
        int nearest = -1;
        float minDist = playerCamera.pixelWidth;
        foreach (KeyValuePair<int, PlayerShip> kvp in clientController.gameManager.PlayerShipsDict) {
            if (kvp.Value.IsDead) // dead players remain on the same spot with renderers and colliders disabled, so we swhould ignore them
                continue;
            Vector3 worldPoint = kvp.Value.transform.position;
            if (Vector3.Dot(transform.forward, worldPoint - transform.position) < 0) {
                continue; // ship is behind camera
            }
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(worldPoint);
            if (new Rect(0, 0, Screen.width, Screen.height).Contains(screenPoint) == false) {
                continue; // not a valid screen point
            }
            float dist = Vector3.Distance((Vector2)screenPoint, screenCenter);
            //Debug.Log("debug dist " + dist + " playerpos " + playerScreenPos + " center " + screenCenter + " rad " + lockRadius);///////////
            if (dist > lockRadius)
                continue;
            if (dist < minDist) {
                minDist = dist;
                nearest = kvp.Value.PlayerID;
                Debug.Log("Locked on playerID = " + nearest);
            }
               
        }
        return nearest;
    }

public Vector3 TargetScreenPoint() {
        if (lockTargetID == -1) {
            Debug.Log("lockTargetID == -1, can't set locking circle");
            return Vector3.zero;
        }

        if (!clientController.gameManager.IsValidPlayerID(lockTargetID)) {
            Debug.LogWarning("locked target wasn't found in Game Manager. locked PlayerID = " + lockTargetID);
            return Vector3.zero;
        }

        Vector3 worldPoint = clientController.gameManager.GetShip(lockTargetID).transform.position;
        if (Vector3.Dot(transform.forward, worldPoint - transform.position) < 0) {
            return Vector3.zero; // ship is behind camera
        }

        Vector3 screenPoint = playerCamera.WorldToScreenPoint(worldPoint);
        if (new Rect(0, 0, Screen.width, Screen.height).Contains(screenPoint) == false) {
            return Vector3.zero; // not a valid screen point
        }

        return screenPoint;
    }

    private void ShootProjectile(Vector3 pos, Quaternion rot) {
        float netTimeStamp = NetworkTransport.GetNetworkTimestamp(); // NetworkTimeStamp is int, but we use float because NetMsg uses float for time stamps.
        GameObject mockProjectile = Instantiate(projectile, pos, rot); // destroyed when the real projectile from the server is instantiated.
        audioSource.clip = projectileSoundClip;
        audioSource.Play();
        mockProjectile.GetComponent<Projectile>().OwnerID = -1; // // mark as mock projectile (locally simulated untill the real projectile is instantiated)
        clientController.mockProjectiles.Add((int)netTimeStamp, mockProjectile);
        clientController.SendShotToHost((byte)NetworkEntity.ObjType.Projectile, pos, rot, (byte)NetworkEntity.ObjType.Projectile, (int)netTimeStamp);
        energy -= energyDrain;
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
