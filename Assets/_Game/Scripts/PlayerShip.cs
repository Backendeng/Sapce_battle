using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerShipPhysics))]
[RequireComponent(typeof(PlayerShipInput))]

public abstract class PlayerShip : NetworkEntity
{

    public bool isPlayer = true;
    protected static PlayerShip activeShip;

    protected PlayerShipInput input;
    protected PlayerShipPhysics physics;

    [Header("Shooting")]
    public float fireRate = 0.5f;
    protected float nextFire = 0.0f;
    public GameObject shot;         // bullet prefab
    public Transform shotSpawn;     // bullet spawn location


    public static PlayerShip ActiveShip
    {
        get { return activeShip; }
    }
    

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
	
	private void Update ()
    {

    }

    private void Awake() {
        input = GetComponent<PlayerShipInput>();
        physics = GetComponent<PlayerShipPhysics>();
    }
}
