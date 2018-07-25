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

    public int PlayerID { get { return EntityID; } }
    public int Health { get; set; }
    private readonly int initialHealth = 100;
    public int Score { get; set; }
    protected static LocalPlayerShip activeShip;
    
    public static LocalPlayerShip ActiveShip { get { return activeShip; } }
    public virtual Vector3 Velocity { get { return physics.Rigidbody.velocity; } }
    public float Throttle { get { return input.throttle; } }
    public float Boost { get; set; }

    protected void Awake() {
        input = GetComponent<PlayerShipInput>();
        physics = GetComponent<PlayerShipPhysics>();
        Health = initialHealth;
    }
}
