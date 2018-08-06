using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShipShootingClient))]

public class RemotePlayerShipClient : PlayerShip {
    private ShipShootingClient shooting;

    //private float interpolationDelay = 0.12f; 

    // Lerping State
    public static bool doLerp = true;
 
    MovementInterpolator movementInterpolator;

    Vector3 velocity;
    public override Vector3 Velocity { get { return velocity; } }

    public new void Start() {
        base.Start();
        movementInterpolator = new MovementInterpolator(transform, EntityID);
        velocity = Vector3.zero;
    }

    private void FixedUpdate() {

        while (incomingQueue.Count > 0) { 
            NetMsg netMessage = incomingQueue.Dequeue();

            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    SC_MovementData msg = (SC_MovementData)netMessage;
                    if (msg.TimeStamp == -1) { // respawn  
                        GetComponent<Transform>().SetPositionAndRotation(msg.Position, msg.Rotation);
                        RespawnOnClientEnd();
                    }

                    movementInterpolator.RecUpdate((SC_MovementData)netMessage);
                    velocity = ((SC_MovementData)netMessage).Velocity;
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
            movementInterpolator.InterpolateMovement();//lerp // NULLREF

    }

    private void SetShipState(SC_MovementData msg) {
        if (doLerp) {
            return;
        }

        GetComponent<Transform>().SetPositionAndRotation(msg.Position, msg.Rotation);
    }

}

