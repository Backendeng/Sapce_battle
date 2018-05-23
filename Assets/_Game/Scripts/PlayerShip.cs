using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerShipPhysics))]
[RequireComponent(typeof(PlayerShipInput))]


public abstract class PlayerShip : NetworkEntity
{
    protected int clientID = -1;
    public int Health { get; set; }
    private int initialHealth = 100;
    public int Score { get; set; }

    public bool isPlayer = true;
    protected static PlayerShip activeShip;

    protected PlayerShipInput input;
    protected PlayerShipPhysics physics;

    public int ClientID { get { return clientID; } set { clientID = value; } }
    public static PlayerShip ActiveShip { get { return activeShip; } }

    public GameObject ShipExplosion;

    public Vector3 Velocity
    {
        get { return physics.Rigidbody.velocity; }
    }

    public float Throttle
    {
        get { return input.throttle; }
    }
	
	private void Update ()
    {
        if (Health == 0) { // TEMP
            Destroy(Instantiate(ShipExplosion, transform.position, Quaternion.identity), 3);
            Health = 1;
        }
    }

    protected void Awake() {
        input = GetComponent<PlayerShipInput>();
        physics = GetComponent<PlayerShipPhysics>();
        Health = initialHealth;
    }
}
