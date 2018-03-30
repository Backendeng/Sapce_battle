using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerShip : PlayerShip {

    private Vector3 lastLinearInput = new Vector3();
    private Vector3 lastAngularInput = new Vector3();

    private void FixedUpdate() {

        // pass player input to the physics
        Vector3 linear_input = new Vector3(0.0f, 0.0f, input.throttle);
        Vector3 angular_input = new Vector3(input.pitch, input.yaw, input.roll);

        physics.SetPhysicsInput(linear_input, angular_input);

        if (linear_input != lastLinearInput || lastAngularInput != angular_input) {
            networkController.GetComponent<Client>().SendInputToHost(entityID, input.throttle, angular_input);
            lastLinearInput = linear_input;
            lastAngularInput = angular_input;
        }

        // shooting
        if ((Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0)) && Time.time > nextFire) {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }

        if (isPlayer)
            activeShip = this;
    }
}
