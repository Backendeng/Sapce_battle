using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerShipPhysics))]
[RequireComponent(typeof(PlayerShipInput))]


public abstract class PlayerShip : NetworkEntity {

    public GameObject ShipExplosion;

    protected PlayerShipInput input;
    protected PlayerShipPhysics physics;

    public int Health { get; set; }
    private readonly int initialHealth = 100;
    public int Score { get; set; }
    protected static PlayerShip activeShip;

    public static PlayerShip ActiveShip { get { return activeShip; } }
    public Vector3 Velocity { get { return physics.Rigidbody.velocity; } }
    public float Throttle { get { return input.throttle; } }
    public float Boost { get; set; }

    protected void Awake() {
        input = GetComponent<PlayerShipInput>();
        physics = GetComponent<PlayerShipPhysics>();
        Health = initialHealth;
    }
}
