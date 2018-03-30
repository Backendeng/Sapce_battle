using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerShip : PlayerShip {

    private void Awake() {
        input = GetComponent<PlayerShipInput>();
        physics = GetComponent<PlayerShipPhysics>();
    }

    private void Update() {
        // pass player input to the physics
        Vector3 linear_input = new Vector3(input.strafe, 0.0f, input.throttle);
        Vector3 angular_input = new Vector3(input.pitch, input.yaw, input.roll);

        if (!isServer) {
            physics.SetPhysicsInput(linear_input, angular_input);
            //networkController.GetComponent<Client>().SendHost(linear_input, angular_input);
        }
        else {
            physics.SetPhysicsInput(linear_input_sent, angular_input_sent);
        }

        if (isPlayer)
            activeShip = this;

        // shooting
        if ((Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0)) && Time.time > nextFire) {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }
}
