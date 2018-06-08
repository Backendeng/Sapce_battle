using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingClient))]

public class RemotePlayerShipClient : PlayerShip {
    private ShipShootingClient shooting;

    //private float interpolationDelay = 0.12f; 

    // Lerping State
    public bool doLerp = true;
 

    MovementInterpolator movementInterpolator;

    public new void Start() {
        base.Start();
        movementInterpolator = new MovementInterpolator(transform, EntityID);
    }

   


    private void FixedUpdate() {

        while (incomingQueue.Count > 0) { 
            NetMsg netMessage = incomingQueue.Dequeue();

            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:

                    movementInterpolator.RecUpdate((SC_MovementData)netMessage);

                    SetShipState((SC_MovementData)netMessage);

                    break;
                case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                    Destroy(gameObject);
                    break;
                default:
                    Debug.Log("ERROR! RemotePlayerShip on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                    break;
            }
            
        }

        if(doLerp)
            movementInterpolator.InterpolateMovement();//lerp

        if (Health == 0) { // TODO: TEMP.
            Destroy(Instantiate(ShipExplosion, transform.position, Quaternion.identity), 3);
            Health = 1; // this is not the way to do this. just temp. remove later.
        }
    }

    private void SetShipState(SC_MovementData msg) {
        if (doLerp) {
            return;
        }

        GetComponent<Transform>().SetPositionAndRotation(msg.Position, msg.Rotation);
    }

}

