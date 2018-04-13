using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipShootingClient : MonoBehaviour {

    [Header("Shooting")]
    public float fireRate1 = 0.05f;
    protected float nextFire1;
    public float fireRate2 = 0.5f;
    protected float nextFire2;
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
        if (Time.time > nextFire2 && (Input.GetButton("LeftTrigger") || Input.GetMouseButtonDown(1))) {
            nextFire2 = Time.time + fireRate2;
            //Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            SendMissileToServer(shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
        // Primary Shot - Ray
        if (Time.time > nextFire1 && (Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0))) {
            nextFire1 = Time.time + fireRate1;
            ShootRay();
        }
    }

    private void SendMissileToServer(Vector3 pos, Quaternion rot) {
        clientController.SendMissileShotToHost(entityID, pos, rot);
    }

    private void ShootRay() {
        RaycastHit hit;
        //GetComponent<LineRenderer>().enabled = true;
        GameObject phaser1 = Instantiate(phaser, shotSpawn);
        if (Physics.Raycast(shotSpawn.position, ship.playerCamera.transform.forward, out hit, rayRange)) {

            Debug.Log("Hit: " + hit.transform.name);
        }
    }

    private void PerformPhaserAnimation() {
        phaser.SetActive(true);
    }
}
