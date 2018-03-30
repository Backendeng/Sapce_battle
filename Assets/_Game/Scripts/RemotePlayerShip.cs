using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerShip : PlayerShip {

    private void Update() {

        if (!isServer) {

            //networkController.GetComponent<Client>().SendHost(linear_input, angular_input);
        }
        else {
            physics.SetPhysicsInput(linear_input_sent, angular_input_sent);
        }
    }
}
