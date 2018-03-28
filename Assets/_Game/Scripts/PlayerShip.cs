using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerShipPhysics))]
[RequireComponent(typeof(PlayerShipInput))]

public class PlayerShip : NetworkEntity
{

    public bool isPlayer = true;
    private PlayerShipInput input;
    private PlayerShipPhysics physics;

    // shooting variables
    public float fireRate = 0.5f;
    private float nextFire = 0.0f;
    public GameObject shot;         // bullet prefab
    public Transform shotSpawn;     // bullet spawn location


    public static PlayerShip ActiveShip
    {
        get { return activeShip; }
    }
    private static PlayerShip activeShip;

    // Getters for external objects
    public bool UsingMouseInput
    {
        get { return input.useMouseInput; }
    }

    public Vector3 Velocity
    {
        get { return physics.GetComponent<Rigidbody>().velocity; }
    }

    public float Throttle
    {
        get { return input.throttle; }
    }


    private void Awake()
    {
        input = GetComponent<PlayerShipInput>();
        physics = GetComponent<PlayerShipPhysics>();
    }
	
	private void Update ()
    {
        // pass player input to the physics
        Vector3 linear_input = new Vector3(input.strafe, 0.0f, input.throttle);
        Vector3 angular_input = new Vector3(input.pitch, input.yaw, input.roll);
        if (!isServer) {
            physics.SetPhysicsInput(linear_input, angular_input);
            network.GetComponent<Client>().SendHost(linear_input, angular_input);
        } else {
            physics.SetPhysicsInput(linear_input_sent, angular_input_sent);
        }
        

        if (isPlayer)
            activeShip = this;

        // shooting
        if ((Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0)) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }
}
